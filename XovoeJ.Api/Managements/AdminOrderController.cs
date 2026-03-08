using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Order;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/orders")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminOrderController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<AdminOrderController> _logger;

        public AdminOrderController(XovoeJDbContext dbContext, ILogger<AdminOrderController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _dbContext.Orders.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(order =>
                        order.OrderNo.Contains(keyword)
                        || _dbContext.Users.Any(user =>
                            user.Id == order.UserId
                            && (
                                (user.UserName != null && user.UserName.Contains(keyword))
                                || (user.NickName != null && user.NickName.Contains(keyword))
                                || (user.PhoneNumber != null && user.PhoneNumber.Contains(keyword))
                            )));
                }

                if (status.HasValue)
                {
                    query = query.Where(order => MapToFrontendStatus(order.Status) == status.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(order => order.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    var endExclusive = endDate.Value.Date.AddDays(1);
                    query = query.Where(order => order.CreatedAt < endExclusive);
                }

                var total = await query.CountAsync();

                var items = await (
                    from order in query
                    join user in _dbContext.Users on order.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    orderby order.CreatedAt descending
                    select new AdminOrderListItemDto
                    {
                        Id = order.Id,
                        OrderNo = order.OrderNo,
                        UserId = order.UserId,
                        UserName = user != null ? (user.NickName ?? user.UserName ?? user.PhoneNumber) : null,
                        TotalAmount = order.TotalAmount,
                        Status = MapToFrontendStatus(order.Status),
                        ReceiverName = order.ConsigneeName,
                        ReceiverPhone = order.ConsigneeMobile,
                        ReceiverAddress = order.ConsigneeAddress,
                        Remark = order.Remark,
                        NeedInvoice = order.NeedInvoice,
                        InvoiceType = order.InvoiceType,
                        InvoiceTitle = order.InvoiceTitle,
                        InvoiceTaxNo = order.InvoiceTaxNo,
                        InvoiceEmail = order.InvoiceEmail,
                        CreatedAt = order.CreatedAt,
                        UpdatedAt = order.UpdatedAt,
                        PaidAt = order.PayTime,
                        ShippedAt = order.ShipTime,
                        ShippingCompany = order.ShippingCompany,
                        TrackingNo = order.TrackingNo,
                        ShippingRemark = order.ShippingRemark,
                        CompletedAt = order.FinishTime,
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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
                _logger.LogError(ex, "加载后台订单列表失败");
                return BadRequest(new { message = "加载订单列表失败" });
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                var result = await (
                    from order in _dbContext.Orders.AsNoTracking().Include(item => item.OrderItems)
                    join user in _dbContext.Users on order.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    where order.Id == orderId
                    select new AdminOrderDetailDto
                    {
                        Id = order.Id,
                        OrderNo = order.OrderNo,
                        UserId = order.UserId,
                        UserName = user != null ? (user.NickName ?? user.UserName ?? user.PhoneNumber) : null,
                        TotalAmount = order.TotalAmount,
                        Status = MapToFrontendStatus(order.Status),
                        ReceiverName = order.ConsigneeName,
                        ReceiverPhone = order.ConsigneeMobile,
                        ReceiverAddress = order.ConsigneeAddress,
                        Remark = order.Remark,
                        NeedInvoice = order.NeedInvoice,
                        InvoiceType = order.InvoiceType,
                        InvoiceTitle = order.InvoiceTitle,
                        InvoiceTaxNo = order.InvoiceTaxNo,
                        InvoiceEmail = order.InvoiceEmail,
                        CreatedAt = order.CreatedAt,
                        UpdatedAt = order.UpdatedAt,
                        PaidAt = order.PayTime,
                        ShippedAt = order.ShipTime,
                        ShippingCompany = order.ShippingCompany,
                        TrackingNo = order.TrackingNo,
                        ShippingRemark = order.ShippingRemark,
                        CompletedAt = order.FinishTime,
                        Items = order.OrderItems.Select(item => new AdminOrderItemDto
                        {
                            Id = item.Id,
                            OrderId = item.OrderId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductImage = item.ProductImage,
                            Price = item.Price,
                            Quantity = item.Quantity,
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载订单详情失败: {OrderId}", orderId);
                return BadRequest(new { message = "加载订单详情失败" });
            }
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrder(string orderId, [FromBody] AdminUpdateOrderRequest request)
        {
            try
            {
                var order = await _dbContext.Orders
                    .Include(item => item.OrderItems)
                    .FirstOrDefaultAsync(order => order.Id == orderId);

                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                if (!string.IsNullOrWhiteSpace(request.Remark))
                {
                    order.Remark = request.Remark;
                }

                if (request.Status == 4 && order.Status != OrderStatus.Cancelled)
                {
                    foreach (var item in order.OrderItems)
                    {
                        await _dbContext.ProductSkus
                            .Where(sku => sku.Id == item.SkuId)
                            .ExecuteUpdateAsync(update => update
                                .SetProperty(sku => sku.Stock, sku => sku.Stock + item.Quantity)
                                .SetProperty(sku => sku.SalesCount, sku => sku.SalesCount - item.Quantity));
                    }

                    order.Status = OrderStatus.Cancelled;
                    order.CancelTime = DateTime.UtcNow;
                }

                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "订单更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单失败: {OrderId}", orderId);
                return BadRequest(new { message = "更新订单失败" });
            }
        }

        [HttpPost("{orderId}/ship")]
        public async Task<IActionResult> ShipOrder(string orderId, [FromBody] ShipOrderRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ShippingCompany))
                {
                    return BadRequest(new { message = "请填写物流公司" });
                }

                if (string.IsNullOrWhiteSpace(request.TrackingNo))
                {
                    return BadRequest(new { message = "请填写运单号" });
                }

                var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == orderId);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                if (order.Status != OrderStatus.Paid)
                {
                    return BadRequest(new { message = "只有已支付订单可以发货" });
                }

                order.Status = OrderStatus.Shipped;
                order.ShipStatus = 1;
                order.ShipTime = DateTime.UtcNow;
                order.ShippingCompany = request.ShippingCompany.Trim();
                order.TrackingNo = request.TrackingNo.Trim();
                order.ShippingRemark = string.IsNullOrWhiteSpace(request.ShippingRemark) ? null : request.ShippingRemark.Trim();
                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "发货成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发货失败: {OrderId}", orderId);
                return BadRequest(new { message = "发货失败" });
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            try
            {
                var order = await _dbContext.Orders.FirstOrDefaultAsync(item => item.Id == orderId);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                order.IsDeleted = true;
                order.DeletedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "订单删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单失败: {OrderId}", orderId);
                return BadRequest(new { message = "删除订单失败" });
            }
        }

        private static int MapToFrontendStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => 0,
                OrderStatus.Paid => 1,
                OrderStatus.Shipped => 2,
                OrderStatus.Received => 3,
                OrderStatus.Completed => 3,
                OrderStatus.Refunding => 5,
                OrderStatus.Cancelled => 4,
                _ => 4,
            };
        }
    }

    public sealed class AdminUpdateOrderRequest
    {
        public int? Status { get; set; }

        public string? Remark { get; set; }
    }

    public class AdminOrderListItemDto
    {
        public string Id { get; set; } = string.Empty;

        public string OrderNo { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public string? UserName { get; set; }

        public decimal TotalAmount { get; set; }

        public int Status { get; set; }

        public string? ReceiverName { get; set; }

        public string? ReceiverPhone { get; set; }

        public string? ReceiverAddress { get; set; }

        public string? Remark { get; set; }

        public bool NeedInvoice { get; set; }

        public int? InvoiceType { get; set; }

        public string? InvoiceTitle { get; set; }

        public string? InvoiceTaxNo { get; set; }

        public string? InvoiceEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime? ShippedAt { get; set; }

        public string? ShippingCompany { get; set; }

        public string? TrackingNo { get; set; }

        public string? ShippingRemark { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public class AdminOrderDetailDto : AdminOrderListItemDto
    {
        public List<AdminOrderItemDto> Items { get; set; } = [];
    }

    public class AdminOrderItemDto
    {
        public string Id { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
