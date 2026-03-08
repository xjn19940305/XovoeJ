using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/marketing")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminCouponLifecycleController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminCouponLifecycleController> _logger;

        public AdminCouponLifecycleController(XovoeJDbContext dbContext, ILogger<AdminCouponLifecycleController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("coupon-issue-batches")]
        public async Task<IActionResult> GetIssueBatches([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _dbContext.CouponIssueBatches
                .AsNoTracking()
                .OrderByDescending(item => item.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                items = items.Select(MapIssueBatch).ToList(),
                total,
                page,
                pageSize,
            });
        }

        [HttpPost("coupon-issue-batches")]
        public async Task<IActionResult> CreateIssueBatch([FromBody] CreateCouponIssueBatchRequest request)
        {
            try
            {
                if (request.CouponTemplateIds == null || request.CouponTemplateIds.Count == 0)
                {
                    return BadRequest(new { message = "请选择要发放的优惠券模板。" });
                }

                var targetUsers = await ResolveTargetUsersAsync(request);
                if (targetUsers.Count == 0)
                {
                    return BadRequest(new { message = "未匹配到可发券用户。" });
                }

                var now = DateTime.UtcNow;
                var templates = await _dbContext.CouponTemplates
                    .Where(item => request.CouponTemplateIds.Contains(item.Id))
                    .ToListAsync();

                if (templates.Count == 0)
                {
                    return BadRequest(new { message = "未找到可用的优惠券模板。" });
                }

                var batch = new CouponIssueBatch
                {
                    TargetType = request.TargetType,
                    CouponTemplateIdsJson = JsonSerializer.Serialize(request.CouponTemplateIds.Distinct()),
                    TargetUserIdsJson = request.TargetUserIds == null ? null : JsonSerializer.Serialize(request.TargetUserIds.Distinct()),
                    TargetMemberLevelsJson = request.TargetMemberLevelCodes == null ? null : JsonSerializer.Serialize(request.TargetMemberLevelCodes.Distinct()),
                    RequestedCount = targetUsers.Count * templates.Count,
                    Status = "completed",
                    CreatedBy = GetCurrentUserId(),
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                _dbContext.CouponIssueBatches.Add(batch);

                foreach (var user in targetUsers)
                {
                    foreach (var template in templates)
                    {
                        if (template.TotalQuantity > 0 && template.IssuedQuantity >= template.TotalQuantity)
                        {
                            batch.FailedCount += 1;
                            _dbContext.CouponIssueRecords.Add(new CouponIssueRecord
                            {
                                BatchId = batch.Id,
                                UserId = user.Id,
                                CouponTemplateId = template.Id,
                                Status = "failed",
                                ErrorMessage = "优惠券库存不足",
                                CreatedAt = now,
                            });
                            continue;
                        }

                        var userCoupon = new UserCoupon
                        {
                            UserId = user.Id,
                            CouponTemplateId = template.Id,
                            Status = CouponStatus.Unused,
                            SourceType = request.TargetType,
                            SourceReference = batch.Id,
                            SnapshotName = template.Name,
                            SnapshotCouponType = template.CouponType,
                            SnapshotDiscountType = template.DiscountType,
                            SnapshotDiscountValue = template.DiscountValue,
                            SnapshotMinOrderAmount = template.MinOrderAmount,
                            IssuedAt = now,
                            ClaimedAt = now,
                            ExpiredAt = template.EndTime,
                            CreatedAt = now,
                            UpdatedAt = now,
                        };

                        _dbContext.UserCoupons.Add(userCoupon);
                        _dbContext.CouponIssueRecords.Add(new CouponIssueRecord
                        {
                            BatchId = batch.Id,
                            UserId = user.Id,
                            CouponTemplateId = template.Id,
                            UserCouponId = userCoupon.Id,
                            Status = "success",
                            CreatedAt = now,
                        });

                        template.IssuedQuantity += 1;
                        template.UpdatedAt = now;
                        batch.SucceededCount += 1;
                    }
                }

                batch.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(MapIssueBatch(batch));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建发券批次失败。");
                return BadRequest(new { message = "创建发券批次失败。" });
            }
        }

        [HttpGet("member-level-reward-rules")]
        public async Task<IActionResult> GetRewardRules()
        {
            var items = await _dbContext.MemberLevelRewardRules
                .AsNoTracking()
                .OrderBy(item => item.Sort)
                .ThenBy(item => item.LevelCode)
                .ToListAsync();

            return Ok(items.Select(MapRewardRule).ToList());
        }

        [HttpPost("member-level-reward-rules")]
        public async Task<IActionResult> SaveRewardRule([FromBody] SaveMemberLevelRewardRuleRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LevelCode) || string.IsNullOrWhiteSpace(request.LevelName))
                {
                    return BadRequest(new { message = "会员等级编码和名称不能为空。" });
                }

                var rule = await _dbContext.MemberLevelRewardRules
                    .FirstOrDefaultAsync(item => item.LevelCode == request.LevelCode);

                if (rule == null)
                {
                    rule = new MemberLevelRewardRule
                    {
                        LevelCode = request.LevelCode.Trim(),
                        CreatedAt = DateTime.UtcNow,
                    };
                    _dbContext.MemberLevelRewardRules.Add(rule);
                }

                rule.LevelName = request.LevelName.Trim();
                rule.CouponTemplateIdsJson = JsonSerializer.Serialize(request.CouponTemplateIds?.Distinct() ?? []);
                rule.Status = request.Status;
                rule.Sort = request.Sort;
                rule.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
                rule.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                return Ok(MapRewardRule(rule));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存会员升级奖励规则失败。");
                return BadRequest(new { message = "保存会员升级奖励规则失败。" });
            }
        }

        private async Task<List<User>> ResolveTargetUsersAsync(CreateCouponIssueBatchRequest request)
        {
            var query = _dbContext.Users.AsQueryable();

            return request.TargetType switch
            {
                "all-users" => await query.ToListAsync(),
                "specified-users" => await query
                    .Where(item => request.TargetUserIds != null && request.TargetUserIds.Contains(item.Id))
                    .ToListAsync(),
                "member-levels" => await query
                    .Where(item => item.CurrentMemberLevelCode != null
                        && request.TargetMemberLevelCodes != null
                        && request.TargetMemberLevelCodes.Contains(item.CurrentMemberLevelCode))
                    .ToListAsync(),
                _ => new List<User>(),
            };
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static CouponIssueBatchDto MapIssueBatch(CouponIssueBatch batch)
        {
            return new CouponIssueBatchDto
            {
                Id = batch.Id,
                TargetType = batch.TargetType,
                CouponTemplateIds = DeserializeIds(batch.CouponTemplateIdsJson),
                TargetUserIds = DeserializeIds(batch.TargetUserIdsJson),
                TargetMemberLevelCodes = DeserializeIds(batch.TargetMemberLevelsJson),
                RequestedCount = batch.RequestedCount,
                SucceededCount = batch.SucceededCount,
                FailedCount = batch.FailedCount,
                Status = batch.Status,
                CreatedBy = batch.CreatedBy,
                CreatedAt = batch.CreatedAt,
                UpdatedAt = batch.UpdatedAt,
            };
        }

        private static MemberLevelRewardRuleDto MapRewardRule(MemberLevelRewardRule rule)
        {
            return new MemberLevelRewardRuleDto
            {
                Id = rule.Id,
                LevelCode = rule.LevelCode,
                LevelName = rule.LevelName,
                CouponTemplateIds = DeserializeIds(rule.CouponTemplateIdsJson),
                Status = rule.Status,
                Sort = rule.Sort,
                Description = rule.Description,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
            };
        }

        private static List<string> DeserializeIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    public sealed class CreateCouponIssueBatchRequest
    {
        public string TargetType { get; set; } = "all-users";
        public List<string> CouponTemplateIds { get; set; } = [];
        public List<string>? TargetUserIds { get; set; }
        public List<string>? TargetMemberLevelCodes { get; set; }
    }

    public sealed class SaveMemberLevelRewardRuleRequest
    {
        public string LevelCode { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public List<string>? CouponTemplateIds { get; set; }
        public int Status { get; set; } = 1;
        public int Sort { get; set; } = 100;
        public string? Description { get; set; }
    }

    public sealed class CouponIssueBatchDto
    {
        public string Id { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public List<string> CouponTemplateIds { get; set; } = [];
        public List<string> TargetUserIds { get; set; } = [];
        public List<string> TargetMemberLevelCodes { get; set; } = [];
        public int RequestedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class MemberLevelRewardRuleDto
    {
        public string Id { get; set; } = string.Empty;
        public string LevelCode { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public List<string> CouponTemplateIds { get; set; } = [];
        public int Status { get; set; }
        public int Sort { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
