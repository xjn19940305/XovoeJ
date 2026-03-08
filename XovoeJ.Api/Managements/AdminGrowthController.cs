using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/growth")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminGrowthController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminGrowthController> _logger;

        public AdminGrowthController(XovoeJDbContext dbContext, ILogger<AdminGrowthController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("distributions")]
        public async Task<IActionResult> GetDistributions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.InviteRelations.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        (item.InviterName != null && item.InviterName.Contains(keyword))
                        || (item.InviteeName != null && item.InviteeName.Contains(keyword))
                        || (item.ReferralCode != null && item.ReferralCode.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new DistributionDto
                    {
                        Id = item.Id,
                        InviterName = item.InviterName,
                        InviteeName = item.InviteeName,
                        ReferralCode = item.ReferralCode,
                        Channel = item.Channel,
                        AttributionSource = item.AttributionSource,
                        TotalOrders = item.TotalOrders,
                        TotalRewardAmount = item.TotalRewardAmount,
                        Status = item.Status,
                        InvitedAt = item.InvitedAt,
                        FirstOrderAt = item.FirstOrderAt,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items,
                    total,
                    page,
                    pageSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载分销关系列表失败。");
                return BadRequest(new { message = "加载分销关系列表失败。" });
            }
        }

        [HttpGet("distributions/{distributionId}")]
        public async Task<IActionResult> GetDistributionById(string distributionId)
        {
            try
            {
                var item = await _dbContext.InviteRelations
                    .AsNoTracking()
                    .Where(relation => relation.Id == distributionId)
                    .Select(relation => new DistributionDto
                    {
                        Id = relation.Id,
                        InviterName = relation.InviterName,
                        InviteeName = relation.InviteeName,
                        ReferralCode = relation.ReferralCode,
                        Channel = relation.Channel,
                        AttributionSource = relation.AttributionSource,
                        TotalOrders = relation.TotalOrders,
                        TotalRewardAmount = relation.TotalRewardAmount,
                        Status = relation.Status,
                        InvitedAt = relation.InvitedAt,
                        FirstOrderAt = relation.FirstOrderAt,
                        CreatedAt = relation.CreatedAt,
                        UpdatedAt = relation.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "分销关系不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载分销关系详情失败：{DistributionId}", distributionId);
                return BadRequest(new { message = "加载分销关系详情失败。" });
            }
        }

        [HttpPost("distributions/{distributionId}/status")]
        public async Task<IActionResult> UpdateDistributionStatus(string distributionId, [FromBody] UpdateGrowthStatusRequest request)
        {
            try
            {
                if (request.Status is < 0 or > 2)
                {
                    return BadRequest(new { message = "分销关系状态不合法。" });
                }

                var relation = await _dbContext.InviteRelations.FirstOrDefaultAsync(item => item.Id == distributionId);
                if (relation == null)
                {
                    return NotFound(new { message = "分销关系不存在。" });
                }

                relation.Status = request.Status;
                relation.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "分销关系状态更新成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分销关系状态失败：{DistributionId}", distributionId);
                return BadRequest(new { message = "更新分销关系状态失败。" });
            }
        }

        [HttpGet("referral-links")]
        public async Task<IActionResult> GetReferralLinks(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] string? channel = null)
        {
            try
            {
                var query = _dbContext.ReferralLinks.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Name.Contains(keyword)
                        || item.Code.Contains(keyword)
                        || (item.OwnerName != null && item.OwnerName.Contains(keyword))
                        || (item.CampaignName != null && item.CampaignName.Contains(keyword))
                        || (item.AttributionSource != null && item.AttributionSource.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                if (!string.IsNullOrWhiteSpace(channel))
                {
                    query = query.Where(item => item.Channel == channel);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new ReferralLinkDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        OwnerName = item.OwnerName,
                        Channel = item.Channel,
                        LandingPath = item.LandingPath,
                        FullUrl = item.FullUrl,
                        AttributionSource = item.AttributionSource,
                        CampaignName = item.CampaignName,
                        ClickCount = item.ClickCount,
                        SignupCount = item.SignupCount,
                        FirstOrderCount = item.FirstOrderCount,
                        RewardAmount = item.RewardAmount,
                        Status = item.Status,
                        ExpireAt = item.ExpireAt,
                        LastVisitAt = item.LastVisitAt,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt,
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items,
                    total,
                    page,
                    pageSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载推广链接列表失败。");
                return BadRequest(new { message = "加载推广链接列表失败。" });
            }
        }

        [HttpGet("referral-links/{linkId}")]
        public async Task<IActionResult> GetReferralLinkById(string linkId)
        {
            try
            {
                var item = await _dbContext.ReferralLinks
                    .AsNoTracking()
                    .Where(link => link.Id == linkId)
                    .Select(link => new ReferralLinkDto
                    {
                        Id = link.Id,
                        Name = link.Name,
                        Code = link.Code,
                        OwnerName = link.OwnerName,
                        Channel = link.Channel,
                        LandingPath = link.LandingPath,
                        FullUrl = link.FullUrl,
                        AttributionSource = link.AttributionSource,
                        CampaignName = link.CampaignName,
                        ClickCount = link.ClickCount,
                        SignupCount = link.SignupCount,
                        FirstOrderCount = link.FirstOrderCount,
                        RewardAmount = link.RewardAmount,
                        Status = link.Status,
                        ExpireAt = link.ExpireAt,
                        LastVisitAt = link.LastVisitAt,
                        CreatedAt = link.CreatedAt,
                        UpdatedAt = link.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "推广链接不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载推广链接详情失败：{LinkId}", linkId);
                return BadRequest(new { message = "加载推广链接详情失败。" });
            }
        }

        [HttpPost("referral-links/{linkId}/status")]
        public async Task<IActionResult> UpdateReferralLinkStatus(string linkId, [FromBody] UpdateGrowthStatusRequest request)
        {
            try
            {
                if (request.Status is < 0 or > 2)
                {
                    return BadRequest(new { message = "推广链接状态不合法。" });
                }

                var link = await _dbContext.ReferralLinks.FirstOrDefaultAsync(item => item.Id == linkId);
                if (link == null)
                {
                    return NotFound(new { message = "推广链接不存在。" });
                }

                link.Status = request.Status;
                link.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "推广链接状态更新成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新推广链接状态失败：{LinkId}", linkId);
                return BadRequest(new { message = "更新推广链接状态失败。" });
            }
        }

        [HttpGet("commissions")]
        public async Task<IActionResult> GetCommissions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.CommissionRecords.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        (item.PromoterName != null && item.PromoterName.Contains(keyword))
                        || (item.OrderNo != null && item.OrderNo.Contains(keyword))
                        || (item.RuleName != null && item.RuleName.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new CommissionDto
                    {
                        Id = item.Id,
                        PromoterName = item.PromoterName,
                        OrderNo = item.OrderNo,
                        RuleName = item.RuleName,
                        SourceType = item.SourceType,
                        CommissionRate = item.CommissionRate,
                        EstimatedAmount = item.EstimatedAmount,
                        SettledAmount = item.SettledAmount,
                        Status = item.Status,
                        CreatedAt = item.CreatedAt,
                        SettledAt = item.SettledAt,
                        UpdatedAt = item.UpdatedAt,
                    })
                    .ToListAsync();

                return Ok(new
                {
                    items,
                    total,
                    page,
                    pageSize,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载佣金记录列表失败。");
                return BadRequest(new { message = "加载佣金记录列表失败。" });
            }
        }

        [HttpGet("commissions/{commissionId}")]
        public async Task<IActionResult> GetCommissionById(string commissionId)
        {
            try
            {
                var item = await _dbContext.CommissionRecords
                    .AsNoTracking()
                    .Where(record => record.Id == commissionId)
                    .Select(record => new CommissionDto
                    {
                        Id = record.Id,
                        PromoterName = record.PromoterName,
                        OrderNo = record.OrderNo,
                        RuleName = record.RuleName,
                        SourceType = record.SourceType,
                        CommissionRate = record.CommissionRate,
                        EstimatedAmount = record.EstimatedAmount,
                        SettledAmount = record.SettledAmount,
                        Status = record.Status,
                        CreatedAt = record.CreatedAt,
                        SettledAt = record.SettledAt,
                        UpdatedAt = record.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "佣金记录不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载佣金记录详情失败：{CommissionId}", commissionId);
                return BadRequest(new { message = "加载佣金记录详情失败。" });
            }
        }

        [HttpPost("commissions/{commissionId}/settle")]
        public async Task<IActionResult> SettleCommission(string commissionId)
        {
            try
            {
                var record = await _dbContext.CommissionRecords.FirstOrDefaultAsync(item => item.Id == commissionId);
                if (record == null)
                {
                    return NotFound(new { message = "佣金记录不存在。" });
                }

                if (record.Status == 2)
                {
                    return BadRequest(new { message = "该佣金记录已完成结算。" });
                }

                if (record.Status == 3)
                {
                    return BadRequest(new { message = "已回退的佣金记录不可结算。" });
                }

                if (record.Status != 1)
                {
                    return BadRequest(new { message = "当前佣金记录还不可结算。" });
                }

                record.Status = 2;
                record.SettledAmount = record.EstimatedAmount;
                record.SettledAt = DateTime.UtcNow;
                record.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "佣金结算成功。" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "结算佣金失败：{CommissionId}", commissionId);
                return BadRequest(new { message = "佣金结算失败。" });
            }
        }
    }

    public sealed class UpdateGrowthStatusRequest
    {
        public int Status { get; set; }
    }

    public sealed class DistributionDto
    {
        public string Id { get; set; } = string.Empty;
        public string? InviterName { get; set; }
        public string? InviteeName { get; set; }
        public string? ReferralCode { get; set; }
        public string? Channel { get; set; }
        public string? AttributionSource { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRewardAmount { get; set; }
        public int Status { get; set; }
        public DateTime? InvitedAt { get; set; }
        public DateTime? FirstOrderAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class CommissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string? PromoterName { get; set; }
        public string? OrderNo { get; set; }
        public string? RuleName { get; set; }
        public string? SourceType { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal SettledAmount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SettledAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class ReferralLinkDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? OwnerName { get; set; }
        public string? Channel { get; set; }
        public string? LandingPath { get; set; }
        public string? FullUrl { get; set; }
        public string? AttributionSource { get; set; }
        public string? CampaignName { get; set; }
        public int ClickCount { get; set; }
        public int SignupCount { get; set; }
        public int FirstOrderCount { get; set; }
        public decimal RewardAmount { get; set; }
        public int Status { get; set; }
        public DateTime? ExpireAt { get; set; }
        public DateTime? LastVisitAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
