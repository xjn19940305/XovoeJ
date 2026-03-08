using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/after-sales")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminAfterSaleController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminAfterSaleController> _logger;

        public AdminAfterSaleController(XovoeJDbContext dbContext, ILogger<AdminAfterSaleController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] int? type = null)
        {
            try
            {
                var query =
                    from afterSale in _dbContext.AfterSaleOrders.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on afterSale.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { afterSale, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.afterSale.AfterSaleNo.Contains(keyword)
                        || item.afterSale.OrderNo.Contains(keyword)
                        || item.afterSale.Reason.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => item.afterSale.Status == status.Value);
                }

                if (type.HasValue)
                {
                    query = query.Where(item => item.afterSale.Type == type.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.afterSale.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminAfterSaleListItemDto
                    {
                        Id = item.afterSale.Id,
                        AfterSaleNo = item.afterSale.AfterSaleNo,
                        OrderId = item.afterSale.OrderId,
                        OrderNo = item.afterSale.OrderNo,
                        UserId = item.afterSale.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        Type = item.afterSale.Type,
                        Status = item.afterSale.Status,
                        Reason = item.afterSale.Reason,
                        RefundAmount = item.afterSale.RefundAmount,
                        ContactName = item.afterSale.ContactName,
                        ContactMobile = item.afterSale.ContactMobile,
                        AppliedAt = item.afterSale.AppliedAt,
                        AuditedAt = item.afterSale.AuditedAt,
                        RefundedAt = item.afterSale.RefundedAt,
                        UpdatedAt = item.afterSale.UpdatedAt,
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
                _logger.LogError(ex, "加载售后列表失败");
                return BadRequest(new { message = "加载售后列表失败" });
            }
        }

        [HttpGet("{afterSaleId}")]
        public async Task<IActionResult> GetDetail(string afterSaleId)
        {
            try
            {
                var detail = await (
                    from afterSale in _dbContext.AfterSaleOrders.AsNoTracking()
                    join order in _dbContext.Orders.AsNoTracking().Include(item => item.OrderItems)
                        on afterSale.OrderId equals order.Id
                    join user in _dbContext.Users.AsNoTracking() on afterSale.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    where afterSale.Id == afterSaleId
                    select new AdminAfterSaleDetailDto
                    {
                        Id = afterSale.Id,
                        AfterSaleNo = afterSale.AfterSaleNo,
                        OrderId = afterSale.OrderId,
                        OrderNo = afterSale.OrderNo,
                        UserId = afterSale.UserId,
                        UserName = user != null ? (user.NickName ?? user.UserName ?? user.PhoneNumber) : null,
                        Type = afterSale.Type,
                        Status = afterSale.Status,
                        Reason = afterSale.Reason,
                        Description = afterSale.Description,
                        RefundAmount = afterSale.RefundAmount,
                        ContactName = afterSale.ContactName,
                        ContactMobile = afterSale.ContactMobile,
                        AdminRemark = afterSale.AdminRemark,
                        AppliedAt = afterSale.AppliedAt,
                        AuditedAt = afterSale.AuditedAt,
                        RefundedAt = afterSale.RefundedAt,
                        ExchangedAt = afterSale.ExchangedAt,
                        ExchangeShippingCompany = afterSale.ExchangeShippingCompany,
                        ExchangeTrackingNo = afterSale.ExchangeTrackingNo,
                        ExchangeRemark = afterSale.ExchangeRemark,
                        UpdatedAt = afterSale.UpdatedAt,
                        EvidenceImages = ParseEvidenceImages(afterSale.EvidenceImages),
                        Items = order.OrderItems.Select(item => new AdminAfterSaleOrderItemDto
                        {
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductImage = item.ProductImage,
                            Price = item.Price,
                            Quantity = item.Quantity,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync();

                if (detail == null)
                {
                    return NotFound(new { message = "售后单不存在" });
                }

                return Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载售后详情失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "加载售后详情失败" });
            }
        }

        [HttpPost("{afterSaleId}/approve")]
        public async Task<IActionResult> Approve(string afterSaleId, [FromBody] AdminAuditAfterSaleRequest? request = null)
        {
            try
            {
                var afterSale = await _dbContext.AfterSaleOrders.FirstOrDefaultAsync(item => item.Id == afterSaleId);
                if (afterSale == null)
                {
                    return NotFound(new { message = "售后单不存在" });
                }

                if (afterSale.Status != 0)
                {
                    return BadRequest(new { message = "当前售后单不可审核通过" });
                }

                afterSale.Status = 1;
                afterSale.AdminRemark = NormalizeText(request?.AdminRemark, 500);
                afterSale.AuditedAt = DateTime.UtcNow;
                afterSale.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "审核通过成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "审核通过售后单失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "审核通过失败" });
            }
        }

        [HttpPost("{afterSaleId}/reject")]
        public async Task<IActionResult> Reject(string afterSaleId, [FromBody] AdminAuditAfterSaleRequest? request = null)
        {
            try
            {
                var afterSale = await _dbContext.AfterSaleOrders.FirstOrDefaultAsync(item => item.Id == afterSaleId);
                if (afterSale == null)
                {
                    return NotFound(new { message = "售后单不存在" });
                }

                if (afterSale.Status is not (0 or 1))
                {
                    return BadRequest(new { message = "当前售后单不可驳回" });
                }

                var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == afterSale.OrderId);
                if (order != null && afterSale.Type != 3 && order.Status == OrderStatus.Refunding)
                {
                    order.Status = (OrderStatus)afterSale.OriginalOrderStatus;
                    order.UpdatedAt = DateTime.UtcNow;
                }

                afterSale.Status = 2;
                afterSale.AdminRemark = NormalizeText(request?.AdminRemark, 500);
                afterSale.AuditedAt = DateTime.UtcNow;
                afterSale.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "驳回成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "驳回售后单失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "驳回失败" });
            }
        }

        [HttpPost("{afterSaleId}/refund")]
        public async Task<IActionResult> Refund(string afterSaleId, [FromBody] AdminRefundAfterSaleRequest? request = null)
        {
            try
            {
                var afterSale = await _dbContext.AfterSaleOrders.FirstOrDefaultAsync(item => item.Id == afterSaleId);
                if (afterSale == null)
                {
                    return NotFound(new { message = "售后单不存在" });
                }

                if (afterSale.Type == 3)
                {
                    return BadRequest(new { message = "换货售后请走换货完成流程" });
                }

                if (afterSale.Status != 1)
                {
                    return BadRequest(new { message = "当前售后单不可执行退款" });
                }

                var order = await _dbContext.Orders
                    .Include(item => item.OrderItems)
                    .FirstOrDefaultAsync(item => item.Id == afterSale.OrderId);
                if (order == null)
                {
                    return NotFound(new { message = "关联订单不存在" });
                }

                var refundAmount = request?.RefundAmount.GetValueOrDefault(afterSale.RefundAmount) ?? afterSale.RefundAmount;
                if (refundAmount <= 0 || refundAmount > order.PayAmount)
                {
                    return BadRequest(new { message = "退款金额不合法" });
                }

                // 退货退款和未发货仅退款都需要回补库存
                if (afterSale.Type == 2 || order.Status == OrderStatus.Refunding && (OrderStatus)afterSale.OriginalOrderStatus == OrderStatus.Paid)
                {
                    foreach (var item in order.OrderItems)
                    {
                        await _dbContext.ProductSkus
                            .Where(sku => sku.Id == item.SkuId)
                            .ExecuteUpdateAsync(update => update
                                .SetProperty(sku => sku.Stock, sku => sku.Stock + item.Quantity)
                                .SetProperty(sku => sku.SalesCount, sku => sku.SalesCount - item.Quantity));
                    }
                }

                afterSale.RefundAmount = refundAmount;
                afterSale.Status = 3;
                afterSale.AdminRemark = NormalizeText(request?.AdminRemark, 500);
                afterSale.RefundedAt = DateTime.UtcNow;
                afterSale.UpdatedAt = DateTime.UtcNow;
                afterSale.AuditedAt ??= DateTime.UtcNow;

                order.Status = OrderStatus.Cancelled;
                order.CancelTime = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "退款成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行售后退款失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "退款失败" });
            }
        }

        [HttpPost("{afterSaleId}/exchange")]
        public async Task<IActionResult> CompleteExchange(string afterSaleId, [FromBody] AdminExchangeAfterSaleRequest request)
        {
            try
            {
                var afterSale = await _dbContext.AfterSaleOrders.FirstOrDefaultAsync(item => item.Id == afterSaleId);
                if (afterSale == null)
                {
                    return NotFound(new { message = "售后单不存在" });
                }

                if (afterSale.Type != 3)
                {
                    return BadRequest(new { message = "当前售后单不是换货单" });
                }

                if (afterSale.Status != 1)
                {
                    return BadRequest(new { message = "当前换货单不可完成换货" });
                }

                if (string.IsNullOrWhiteSpace(request.ShippingCompany))
                {
                    return BadRequest(new { message = "请填写换货物流公司" });
                }

                if (string.IsNullOrWhiteSpace(request.TrackingNo))
                {
                    return BadRequest(new { message = "请填写换货运单号" });
                }

                var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == afterSale.OrderId);
                if (order != null)
                {
                    order.Status = (OrderStatus)afterSale.OriginalOrderStatus;
                    order.UpdatedAt = DateTime.UtcNow;
                }

                afterSale.Status = 3;
                afterSale.ExchangeShippingCompany = NormalizeText(request.ShippingCompany, 64);
                afterSale.ExchangeTrackingNo = NormalizeText(request.TrackingNo, 64);
                afterSale.ExchangeRemark = NormalizeText(request.ExchangeRemark, 500);
                afterSale.AdminRemark = NormalizeText(request.AdminRemark, 500) ?? afterSale.AdminRemark;
                afterSale.ExchangedAt = DateTime.UtcNow;
                afterSale.UpdatedAt = DateTime.UtcNow;
                afterSale.AuditedAt ??= DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "换货完成" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完成换货失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "换货处理失败" });
            }
        }

        private static string? NormalizeText(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        private static List<string> ParseEvidenceImages(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(value) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    public sealed class AdminAuditAfterSaleRequest
    {
        public string? AdminRemark { get; set; }
    }

    public sealed class AdminRefundAfterSaleRequest
    {
        public decimal? RefundAmount { get; set; }
        public string? AdminRemark { get; set; }
    }

    public sealed class AdminExchangeAfterSaleRequest
    {
        public string ShippingCompany { get; set; } = string.Empty;
        public string TrackingNo { get; set; } = string.Empty;
        public string? ExchangeRemark { get; set; }
        public string? AdminRemark { get; set; }
    }

    public class AdminAfterSaleListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string AfterSaleNo { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public string? ContactName { get; set; }
        public string? ContactMobile { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? AuditedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class AdminAfterSaleDetailDto : AdminAfterSaleListItemDto
    {
        public string? Description { get; set; }
        public string? AdminRemark { get; set; }
        public DateTime? ExchangedAt { get; set; }
        public string? ExchangeShippingCompany { get; set; }
        public string? ExchangeTrackingNo { get; set; }
        public string? ExchangeRemark { get; set; }
        public List<string> EvidenceImages { get; set; } = [];
        public List<AdminAfterSaleOrderItemDto> Items { get; set; } = [];
    }

    public sealed class AdminAfterSaleOrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
