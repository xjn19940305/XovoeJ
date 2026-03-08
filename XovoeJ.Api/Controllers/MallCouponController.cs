using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
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

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCoupons(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? couponType = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var query = _dbContext.CouponTemplates
                    .AsNoTracking()
                    .Where(item => item.Status == 1 && (!item.StartTime.HasValue || item.StartTime <= now) && (!item.EndTime.HasValue || item.EndTime >= now));

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
                    .Select(item => new MallCouponDto
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
                _logger.LogError(ex, "加载可领取优惠券列表失败。");
                return BadRequest(new { message = "加载可领取优惠券列表失败。" });
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
                    .Where(row => row.Id == couponId && row.Status == 1 && (!row.StartTime.HasValue || row.StartTime <= now) && (!row.EndTime.HasValue || row.EndTime >= now))
                    .Select(row => new MallCouponDto
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
    }

    public sealed class MallCouponDto
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
    }
}
