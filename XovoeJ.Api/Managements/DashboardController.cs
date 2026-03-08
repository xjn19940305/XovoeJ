using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Api.Swaggers;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class DashboardController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(XovoeJDbContext dbContext, ILogger<DashboardController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                var monthStart = new DateTime(today.Year, today.Month, 1);

                var totalOrders = await _dbContext.Orders.CountAsync();
                var todayOrders = await _dbContext.Orders.CountAsync(o => o.CreatedAt >= today);
                var pendingPaymentOrders = await _dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
                var pendingShipmentOrders = await _dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Paid);
                var shippedOrders = await _dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Shipped);
                var completedOrders = await _dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Received);
                var cancelledOrders = await _dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);

                var todaySalesAmount = await _dbContext.Orders
                    .Where(o => o.PayTime >= today && o.PayStatus == 1)
                    .SumAsync(o => (decimal?)o.PayAmount) ?? 0m;

                var monthSalesAmount = await _dbContext.Orders
                    .Where(o => o.PayTime >= monthStart && o.PayStatus == 1)
                    .SumAsync(o => (decimal?)o.PayAmount) ?? 0m;

                var totalProducts = await _dbContext.Products.CountAsync();
                var onSaleProducts = await _dbContext.Products.CountAsync(p => p.IsEnabled);
                var outOfStockProducts = await _dbContext.ProductSkus
                    .GroupBy(sku => sku.ProductId)
                    .CountAsync(group => group.Max(sku => sku.Stock) <= 0);
                var lowStockProducts = await _dbContext.ProductSkus
                    .Where(sku => sku.Stock > 0 && sku.Stock <= sku.LowStock)
                    .Select(sku => sku.ProductId)
                    .Distinct()
                    .CountAsync();

                var totalUsers = await _dbContext.Users.CountAsync();
                var todayUsers = await _dbContext.Users.CountAsync(u => u.CreatedAt >= today);
                var thisWeekUsers = await _dbContext.Users.CountAsync(u => u.CreatedAt >= weekStart);
                var thisMonthUsers = await _dbContext.Users.CountAsync(u => u.CreatedAt >= monthStart);

                return Ok(new
                {
                    todayOrders,
                    todayUsers,
                    todaySalesAmount,
                    monthSalesAmount,
                    totalOrders,
                    pendingPaymentOrders,
                    pendingShipmentOrders,
                    shippedOrders,
                    completedOrders,
                    cancelledOrders,
                    totalProducts,
                    onSaleProducts,
                    outOfStockProducts,
                    lowStockProducts,
                    totalUsers,
                    thisWeekUsers,
                    thisMonthUsers,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard stats.");
                return BadRequest(new { message = "Failed to load dashboard stats." });
            }
        }

        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders()
        {
            try
            {
                var orders = await (
                    from order in _dbContext.Orders
                    join user in _dbContext.Users on order.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    orderby order.CreatedAt descending
                    select new
                    {
                        id = order.Id,
                        orderNo = order.OrderNo,
                        userId = order.UserId,
                        userName = user != null ? (user.NickName ?? user.UserName ?? user.PhoneNumber) : null,
                        totalAmount = order.TotalAmount,
                        status = MapToFrontendStatus(order.Status),
                        receiverName = order.ConsigneeName,
                        receiverPhone = order.ConsigneeMobile,
                        receiverAddress = order.ConsigneeAddress,
                        remark = order.Remark,
                        createdAt = order.CreatedAt,
                        updatedAt = order.UpdatedAt,
                        paidAt = order.PayTime,
                        shippedAt = order.ShipTime,
                        completedAt = order.FinishTime,
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent orders.");
                return BadRequest(new { message = "Failed to load recent orders." });
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
}
