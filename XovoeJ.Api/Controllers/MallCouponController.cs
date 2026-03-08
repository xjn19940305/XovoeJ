using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Controllers
{
    [ApiController]
    [Route("api/mall/coupons")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class MallCouponController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<MallCouponController> _logger;

        public MallCouponController(XovoeJDbContext dbContext, ILogger<MallCouponController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("center")]
        [HttpGet("available")]
        public async Task<IActionResult> GetCouponCenter(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? couponType = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var now = DateTime.UtcNow;
                var query = _dbContext.CouponTemplates
                    .AsNoTracking()
                    .Where(item => item.Status == 1
                        && (!item.StartTime.HasValue || item.StartTime <= now)
                        && (!item.EndTime.HasValue || item.EndTime >= now));

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item => item.Name.Contains(keyword) || item.Code.Contains(keyword));
                }

                if (couponType.HasValue)
                {
                    query = query.Where(item => item.CouponType == couponType.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MallCouponCenterDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        CouponType = item.CouponType,
                        DiscountType = item.DiscountType,
                        DiscountValue = item.DiscountValue,
                        MinOrderAmount = item.MinOrderAmount,
                        ReceiveLimit = item.ReceiveLimit,
                        Description = item.Description,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        ClaimedCount = _dbContext.UserCoupons.Count(coupon => coupon.UserId == userId && coupon.CouponTemplateId == item.Id),
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载券中心失败。");
                return BadRequest(new { message = "加载券中心失败。" });
            }
        }

        [HttpGet("available/{couponId}")]
        public async Task<IActionResult> GetAvailableCoupon(string couponId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var item = await _dbContext.CouponTemplates
                    .AsNoTracking()
                    .Where(row => row.Id == couponId
                        && row.Status == 1
                        && (!row.StartTime.HasValue || row.StartTime <= now)
                        && (!row.EndTime.HasValue || row.EndTime >= now))
                    .Select(row => new MallCouponCenterDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Code = row.Code,
                        CouponType = row.CouponType,
                        DiscountType = row.DiscountType,
                        DiscountValue = row.DiscountValue,
                        MinOrderAmount = row.MinOrderAmount,
                        ReceiveLimit = row.ReceiveLimit,
                        Description = row.Description,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "优惠券不存在或当前不可领取。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载可领取优惠券详情失败：{CouponId}", couponId);
                return BadRequest(new { message = "加载可领取优惠券详情失败。" });
            }
        }

        [HttpPost("{couponId}/claim")]
        public async Task<IActionResult> ClaimCoupon(string couponId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var now = DateTime.UtcNow;
                var template = await _dbContext.CouponTemplates.FirstOrDefaultAsync(item => item.Id == couponId
                    && item.Status == 1
                    && (!item.StartTime.HasValue || item.StartTime <= now)
                    && (!item.EndTime.HasValue || item.EndTime >= now));

                if (template == null)
                {
                    return NotFound(new { message = "优惠券不存在或当前不可领取。" });
                }

                var claimedCount = await _dbContext.UserCoupons.CountAsync(item => item.UserId == userId && item.CouponTemplateId == couponId);
                if (template.ReceiveLimit.HasValue && claimedCount >= template.ReceiveLimit.Value)
                {
                    return BadRequest(new { message = "已达到该优惠券的领取上限。" });
                }

                if (template.TotalQuantity > 0 && template.IssuedQuantity >= template.TotalQuantity)
                {
                    return BadRequest(new { message = "优惠券已领完。" });
                }

                var userCoupon = new UserCoupon
                {
                    UserId = userId,
                    CouponTemplateId = template.Id,
                    Status = CouponStatus.Unused,
                    SourceType = "claim",
                    SnapshotName = template.Name,
                    SnapshotCouponType = template.CouponType,
                    SnapshotDiscountType = template.DiscountType,
                    SnapshotDiscountValue = template.DiscountValue,
                    SnapshotMinOrderAmount = template.MinOrderAmount,
                    ClaimedAt = now,
                    IssuedAt = now,
                    ExpiredAt = template.EndTime,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                template.IssuedQuantity += 1;
                template.UpdatedAt = now;
                _dbContext.UserCoupons.Add(userCoupon);
                await _dbContext.SaveChangesAsync();

                return Ok(MapWalletItem(userCoupon));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "领取优惠券失败：{CouponId}", couponId);
                return BadRequest(new { message = "领取优惠券失败。" });
            }
        }

        [HttpGet("wallet")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyCoupons(
            [FromQuery] int? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息。" });
                }

                var query = _dbContext.UserCoupons
                    .AsNoTracking()
                    .Where(item => item.UserId == userId);

                if (status.HasValue)
                {
                    query = query.Where(item => (int)item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => MapWalletItem(item))
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载我的优惠券失败。");
                return BadRequest(new { message = "加载我的优惠券失败。" });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static MallCouponWalletDto MapWalletItem(UserCoupon item)
        {
            return new MallCouponWalletDto
            {
                Id = item.Id,
                CouponTemplateId = item.CouponTemplateId,
                Name = item.SnapshotName,
                CouponType = item.SnapshotCouponType,
                DiscountType = item.SnapshotDiscountType,
                DiscountValue = item.SnapshotDiscountValue,
                MinOrderAmount = item.SnapshotMinOrderAmount,
                Status = (int)item.Status,
                ExpiredAt = item.ExpiredAt,
                ClaimedAt = item.ClaimedAt,
                UsedAt = item.UsedAt,
            };
        }
    }

    public sealed class MallCouponCenterDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int CouponType { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int? ReceiveLimit { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ClaimedCount { get; set; }
    }

    public sealed class MallCouponWalletDto
    {
        public string Id { get; set; } = string.Empty;
        public string CouponTemplateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int CouponType { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int Status { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime? ExpiredAt { get; set; }
    }
}
