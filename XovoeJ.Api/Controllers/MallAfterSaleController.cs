using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Controllers
{
    [ApiController]
    [Route("api/mall/after-sales")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class MallAfterSaleController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<MallAfterSaleController> _logger;

        public MallAfterSaleController(XovoeJDbContext dbContext, ILogger<MallAfterSaleController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? status = null)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var query = _dbContext.AfterSaleOrders
                    .AsNoTracking()
                    .Where(item => item.UserId == userId);

                if (status.HasValue)
                {
                    query = query.Where(item => item.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new MallAfterSaleItemDto
                    {
                        Id = item.Id,
                        AfterSaleNo = item.AfterSaleNo,
                        OrderId = item.OrderId,
                        OrderNo = item.OrderNo,
                        Type = item.Type,
                        Status = item.Status,
                        Reason = item.Reason,
                        RefundAmount = item.RefundAmount,
                        AppliedAt = item.AppliedAt,
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
                _logger.LogError(ex, "加载用户售后列表失败");
                return BadRequest(new { message = "加载售后列表失败" });
            }
        }

        [HttpGet("{afterSaleId}")]
        public async Task<IActionResult> GetDetail(string afterSaleId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var detail = await (
                    from afterSale in _dbContext.AfterSaleOrders.AsNoTracking()
                    join order in _dbContext.Orders.AsNoTracking().Include(item => item.OrderItems)
                        on afterSale.OrderId equals order.Id
                    where afterSale.Id == afterSaleId && afterSale.UserId == userId
                    select new MallAfterSaleDetailDto
                    {
                        Id = afterSale.Id,
                        AfterSaleNo = afterSale.AfterSaleNo,
                        OrderId = afterSale.OrderId,
                        OrderNo = afterSale.OrderNo,
                        Type = afterSale.Type,
                        Status = afterSale.Status,
                        Reason = afterSale.Reason,
                        Description = afterSale.Description,
                        ContactName = afterSale.ContactName,
                        ContactMobile = afterSale.ContactMobile,
                        RefundAmount = afterSale.RefundAmount,
                        AdminRemark = afterSale.AdminRemark,
                        AppliedAt = afterSale.AppliedAt,
                        AuditedAt = afterSale.AuditedAt,
                        RefundedAt = afterSale.RefundedAt,
                        ExchangedAt = afterSale.ExchangedAt,
                        ExchangeShippingCompany = afterSale.ExchangeShippingCompany,
                        ExchangeTrackingNo = afterSale.ExchangeTrackingNo,
                        ExchangeRemark = afterSale.ExchangeRemark,
                        EvidenceImages = ParseEvidenceImages(afterSale.EvidenceImages),
                        Items = order.OrderItems.Select(item => new AfterSaleOrderItemDto
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
                _logger.LogError(ex, "加载用户售后详情失败: {AfterSaleId}", afterSaleId);
                return BadRequest(new { message = "加载售后详情失败" });
            }
        }

        [HttpPost("/api/mall/orders/{orderId}/after-sales")]
        public async Task<IActionResult> Create(string orderId, [FromBody] MallCreateAfterSaleRequest request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                if (request.Type is < 1 or > 3)
                {
                    return BadRequest(new { message = "当前仅支持仅退款、退货退款和换货" });
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest(new { message = "请填写售后原因" });
                }

                var order = await _dbContext.Orders
                    .AsTracking()
                    .FirstOrDefaultAsync(item => item.Id == orderId && item.UserId == userId);

                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                if (order.PayStatus != 1 || order.Status is OrderStatus.Pending or OrderStatus.Cancelled or OrderStatus.Refunding)
                {
                    return BadRequest(new { message = "当前订单不支持申请售后" });
                }

                if (request.Type == 3 && order.Status is not (OrderStatus.Shipped or OrderStatus.Received or OrderStatus.Completed))
                {
                    return BadRequest(new { message = "换货仅支持已发货订单申请" });
                }

                var hasOpenAfterSale = await _dbContext.AfterSaleOrders.AnyAsync(item =>
                    item.OrderId == orderId && item.UserId == userId && (item.Status == 0 || item.Status == 1));
                if (hasOpenAfterSale)
                {
                    return BadRequest(new { message = "该订单已有处理中售后单" });
                }

                var refundAmount = request.Type == 3 ? 0 : request.RefundAmount.GetValueOrDefault(order.PayAmount);
                if (request.Type != 3 && (refundAmount <= 0 || refundAmount > order.PayAmount))
                {
                    return BadRequest(new { message = "退款金额不合法" });
                }

                var originalStatus = order.Status;
                if (request.Type != 3)
                {
                    order.Status = OrderStatus.Refunding;
                    order.UpdatedAt = DateTime.UtcNow;
                }

                var afterSale = new AfterSaleOrder
                {
                    AfterSaleNo = GenerateAfterSaleNo(),
                    OrderId = order.Id,
                    OrderNo = order.OrderNo,
                    UserId = userId,
                    Type = request.Type,
                    Status = 0,
                    Reason = request.Reason.Trim(),
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    ContactName = string.IsNullOrWhiteSpace(request.ContactName) ? null : request.ContactName.Trim(),
                    ContactMobile = string.IsNullOrWhiteSpace(request.ContactMobile) ? null : request.ContactMobile.Trim(),
                    RefundAmount = refundAmount,
                    OriginalOrderStatus = (int)originalStatus,
                    EvidenceImages = request.EvidenceImages is { Count: > 0 } ? JsonSerializer.Serialize(request.EvidenceImages.Where(item => !string.IsNullOrWhiteSpace(item)).Distinct()) : null,
                    AppliedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _dbContext.AfterSaleOrders.Add(afterSale);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    id = afterSale.Id,
                    afterSaleNo = afterSale.AfterSaleNo,
                    message = "售后申请已提交",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交售后申请失败: {OrderId}", orderId);
                return BadRequest(new { message = "提交售后申请失败" });
            }
        }

        private static string GenerateAfterSaleNo()
        {
            return $"AS{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid().ToString("N")[..6]}";
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

    public sealed class MallCreateAfterSaleRequest
    {
        public int Type { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? ContactName { get; set; }
        public string? ContactMobile { get; set; }
        public List<string>? EvidenceImages { get; set; }
    }

    public class MallAfterSaleItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string AfterSaleNo { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public int Type { get; set; }
        public int Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public sealed class MallAfterSaleDetailDto : MallAfterSaleItemDto
    {
        public string? Description { get; set; }
        public string? ContactName { get; set; }
        public string? ContactMobile { get; set; }
        public string? AdminRemark { get; set; }
        public DateTime? AuditedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime? ExchangedAt { get; set; }
        public string? ExchangeShippingCompany { get; set; }
        public string? ExchangeTrackingNo { get; set; }
        public string? ExchangeRemark { get; set; }
        public List<string> EvidenceImages { get; set; } = [];
        public List<AfterSaleOrderItemDto> Items { get; set; } = [];
    }

    public sealed class AfterSaleOrderItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
