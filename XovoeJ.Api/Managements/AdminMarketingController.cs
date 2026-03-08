using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/marketing")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminMarketingController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminMarketingController> _logger;

        public AdminMarketingController(XovoeJDbContext dbContext, ILogger<AdminMarketingController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("coupon-templates")]
        public async Task<IActionResult> GetCouponTemplates(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] int? couponType = null)
        {
            try
            {
                var query = _dbContext.CouponTemplates.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item => item.Name.Contains(keyword) || item.Code.Contains(keyword));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
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
                    .Select(item => new CouponTemplateDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        CouponType = item.CouponType,
                        DiscountType = item.DiscountType,
                        DiscountValue = item.DiscountValue,
                        MinOrderAmount = item.MinOrderAmount,
                        TotalQuantity = item.TotalQuantity,
                        IssuedQuantity = item.IssuedQuantity,
                        UsedQuantity = item.UsedQuantity,
                        Status = item.Status,
                        ReceiveLimit = item.ReceiveLimit,
                        Description = item.Description,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
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
                _logger.LogError(ex, "加载优惠券模板列表失败。");
                return BadRequest(new { message = "加载优惠券模板列表失败。" });
            }
        }

        [HttpGet("coupon-templates/{couponTemplateId}")]
        public async Task<IActionResult> GetCouponTemplateById(string couponTemplateId)
        {
            try
            {
                var item = await _dbContext.CouponTemplates
                    .AsNoTracking()
                    .Where(row => row.Id == couponTemplateId)
                    .Select(row => new CouponTemplateDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Code = row.Code,
                        CouponType = row.CouponType,
                        DiscountType = row.DiscountType,
                        DiscountValue = row.DiscountValue,
                        MinOrderAmount = row.MinOrderAmount,
                        TotalQuantity = row.TotalQuantity,
                        IssuedQuantity = row.IssuedQuantity,
                        UsedQuantity = row.UsedQuantity,
                        Status = row.Status,
                        ReceiveLimit = row.ReceiveLimit,
                        Description = row.Description,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                        CreatedAt = row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "优惠券模板不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载优惠券模板详情失败：{CouponTemplateId}", couponTemplateId);
                return BadRequest(new { message = "加载优惠券模板详情失败。" });
            }
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] int? type = null)
        {
            try
            {
                var query = _dbContext.PromotionActivities.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item => item.Name.Contains(keyword) || (item.ScopeText != null && item.ScopeText.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                if (type.HasValue)
                {
                    query = query.Where(item => item.Type == type.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .ThenByDescending(item => item.Priority)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new PromotionActivityDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Type = item.Type,
                        ScopeText = item.ScopeText,
                        Priority = item.Priority,
                        Stackable = item.Stackable,
                        OrderCount = item.OrderCount,
                        ParticipantCount = item.ParticipantCount,
                        Status = item.Status,
                        Description = item.Description,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
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
                _logger.LogError(ex, "加载营销活动列表失败。");
                return BadRequest(new { message = "加载营销活动列表失败。" });
            }
        }

        [HttpGet("promotions/{promotionId}")]
        public async Task<IActionResult> GetPromotionById(string promotionId)
        {
            try
            {
                var item = await _dbContext.PromotionActivities
                    .AsNoTracking()
                    .Where(row => row.Id == promotionId)
                    .Select(row => new PromotionActivityDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Type = row.Type,
                        ScopeText = row.ScopeText,
                        Priority = row.Priority,
                        Stackable = row.Stackable,
                        OrderCount = row.OrderCount,
                        ParticipantCount = row.ParticipantCount,
                        Status = row.Status,
                        Description = row.Description,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                        CreatedAt = row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "营销活动不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载营销活动详情失败：{PromotionId}", promotionId);
                return BadRequest(new { message = "加载营销活动详情失败。" });
            }
        }

        [HttpGet("seckills")]
        public async Task<IActionResult> GetSeckills(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.SeckillActivities.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Name.Contains(keyword)
                        || item.Code.Contains(keyword)
                        || (item.ProductName != null && item.ProductName.Contains(keyword))
                        || (item.SkuName != null && item.SkuName.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new SeckillActivityDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        ProductName = item.ProductName,
                        SkuName = item.SkuName,
                        CoverImage = item.CoverImage,
                        OriginalPrice = item.OriginalPrice,
                        SeckillPrice = item.SeckillPrice,
                        TotalStock = item.TotalStock,
                        LockedStock = item.LockedStock,
                        SoldStock = item.SoldStock,
                        PurchaseLimit = item.PurchaseLimit,
                        ParticipantCount = item.ParticipantCount,
                        OrderCount = item.OrderCount,
                        Status = item.Status,
                        WarmupStartTime = item.WarmupStartTime,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Description = item.Description,
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
                _logger.LogError(ex, "加载秒杀活动列表失败。");
                return BadRequest(new { message = "加载秒杀活动列表失败。" });
            }
        }

        [HttpGet("seckills/{seckillId}")]
        public async Task<IActionResult> GetSeckillById(string seckillId)
        {
            try
            {
                var item = await _dbContext.SeckillActivities
                    .AsNoTracking()
                    .Where(row => row.Id == seckillId)
                    .Select(row => new SeckillActivityDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Code = row.Code,
                        ProductName = row.ProductName,
                        SkuName = row.SkuName,
                        CoverImage = row.CoverImage,
                        OriginalPrice = row.OriginalPrice,
                        SeckillPrice = row.SeckillPrice,
                        TotalStock = row.TotalStock,
                        LockedStock = row.LockedStock,
                        SoldStock = row.SoldStock,
                        PurchaseLimit = row.PurchaseLimit,
                        ParticipantCount = row.ParticipantCount,
                        OrderCount = row.OrderCount,
                        Status = row.Status,
                        WarmupStartTime = row.WarmupStartTime,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                        Description = row.Description,
                        CreatedAt = row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "秒杀活动不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载秒杀活动详情失败：{SeckillId}", seckillId);
                return BadRequest(new { message = "加载秒杀活动详情失败。" });
            }
        }

        [HttpGet("group-buys")]
        public async Task<IActionResult> GetGroupBuys(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.GroupBuyActivities.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Name.Contains(keyword)
                        || item.Code.Contains(keyword)
                        || (item.ProductName != null && item.ProductName.Contains(keyword))
                        || (item.SkuName != null && item.SkuName.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new GroupBuyActivityDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        ProductName = item.ProductName,
                        SkuName = item.SkuName,
                        OriginalPrice = item.OriginalPrice,
                        GroupPrice = item.GroupPrice,
                        GroupSize = item.GroupSize,
                        PurchaseLimit = item.PurchaseLimit,
                        VirtualGroupCount = item.VirtualGroupCount,
                        ParticipantCount = item.ParticipantCount,
                        SuccessGroupCount = item.SuccessGroupCount,
                        FailedGroupCount = item.FailedGroupCount,
                        GroupLeaderReward = item.GroupLeaderReward,
                        Status = item.Status,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Description = item.Description,
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
                _logger.LogError(ex, "加载拼团活动列表失败。");
                return BadRequest(new { message = "加载拼团活动列表失败。" });
            }
        }

        [HttpGet("group-buys/{groupBuyId}")]
        public async Task<IActionResult> GetGroupBuyById(string groupBuyId)
        {
            try
            {
                var item = await _dbContext.GroupBuyActivities
                    .AsNoTracking()
                    .Where(row => row.Id == groupBuyId)
                    .Select(row => new GroupBuyActivityDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Code = row.Code,
                        ProductName = row.ProductName,
                        SkuName = row.SkuName,
                        OriginalPrice = row.OriginalPrice,
                        GroupPrice = row.GroupPrice,
                        GroupSize = row.GroupSize,
                        PurchaseLimit = row.PurchaseLimit,
                        VirtualGroupCount = row.VirtualGroupCount,
                        ParticipantCount = row.ParticipantCount,
                        SuccessGroupCount = row.SuccessGroupCount,
                        FailedGroupCount = row.FailedGroupCount,
                        GroupLeaderReward = row.GroupLeaderReward,
                        Status = row.Status,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                        Description = row.Description,
                        CreatedAt = row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "拼团活动不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载拼团活动详情失败：{GroupBuyId}", groupBuyId);
                return BadRequest(new { message = "加载拼团活动详情失败。" });
            }
        }

        [HttpGet("bargains")]
        public async Task<IActionResult> GetBargains(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null)
        {
            try
            {
                var query = _dbContext.BargainActivities.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.Name.Contains(keyword)
                        || item.Code.Contains(keyword)
                        || (item.ProductName != null && item.ProductName.Contains(keyword))
                        || (item.SkuName != null && item.SkuName.Contains(keyword)));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.UpdatedAt ?? item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new BargainActivityDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Code = item.Code,
                        ProductName = item.ProductName,
                        SkuName = item.SkuName,
                        OriginalPrice = item.OriginalPrice,
                        FloorPrice = item.FloorPrice,
                        CurrentLowestPrice = item.CurrentLowestPrice,
                        PurchaseLimit = item.PurchaseLimit,
                        ParticipantCount = item.ParticipantCount,
                        HelperCount = item.HelperCount,
                        SuccessCount = item.SuccessCount,
                        Status = item.Status,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        Description = item.Description,
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
                _logger.LogError(ex, "加载砍价活动列表失败。");
                return BadRequest(new { message = "加载砍价活动列表失败。" });
            }
        }

        [HttpGet("bargains/{bargainId}")]
        public async Task<IActionResult> GetBargainById(string bargainId)
        {
            try
            {
                var item = await _dbContext.BargainActivities
                    .AsNoTracking()
                    .Where(row => row.Id == bargainId)
                    .Select(row => new BargainActivityDto
                    {
                        Id = row.Id,
                        Name = row.Name,
                        Code = row.Code,
                        ProductName = row.ProductName,
                        SkuName = row.SkuName,
                        OriginalPrice = row.OriginalPrice,
                        FloorPrice = row.FloorPrice,
                        CurrentLowestPrice = row.CurrentLowestPrice,
                        PurchaseLimit = row.PurchaseLimit,
                        ParticipantCount = row.ParticipantCount,
                        HelperCount = row.HelperCount,
                        SuccessCount = row.SuccessCount,
                        Status = row.Status,
                        StartTime = row.StartTime,
                        EndTime = row.EndTime,
                        Description = row.Description,
                        CreatedAt = row.CreatedAt,
                        UpdatedAt = row.UpdatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    return NotFound(new { message = "砍价活动不存在。" });
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载砍价活动详情失败：{BargainId}", bargainId);
                return BadRequest(new { message = "加载砍价活动详情失败。" });
            }
        }
    }

    public sealed class CouponTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int CouponType { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int TotalQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public int UsedQuantity { get; set; }
        public int Status { get; set; }
        public int? ReceiveLimit { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class PromotionActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public string? ScopeText { get; set; }
        public int Priority { get; set; }
        public bool Stackable { get; set; }
        public int OrderCount { get; set; }
        public int ParticipantCount { get; set; }
        public int Status { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class SeckillActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? SkuName { get; set; }
        public string? CoverImage { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal SeckillPrice { get; set; }
        public int TotalStock { get; set; }
        public int LockedStock { get; set; }
        public int SoldStock { get; set; }
        public int PurchaseLimit { get; set; }
        public int ParticipantCount { get; set; }
        public int OrderCount { get; set; }
        public int Status { get; set; }
        public DateTime? WarmupStartTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class GroupBuyActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? SkuName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal GroupPrice { get; set; }
        public int GroupSize { get; set; }
        public int PurchaseLimit { get; set; }
        public int VirtualGroupCount { get; set; }
        public int ParticipantCount { get; set; }
        public int SuccessGroupCount { get; set; }
        public int FailedGroupCount { get; set; }
        public decimal GroupLeaderReward { get; set; }
        public int Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class BargainActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? SkuName { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FloorPrice { get; set; }
        public decimal CurrentLowestPrice { get; set; }
        public int PurchaseLimit { get; set; }
        public int ParticipantCount { get; set; }
        public int HelperCount { get; set; }
        public int SuccessCount { get; set; }
        public int Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
