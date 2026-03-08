using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Order;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;

namespace XovoeJ.Api.Controllers
{
    /// <summary>
    /// 用户订单控制器
    /// </summary>
    [ApiController]
    [Route("api/user/orders")]
    [Route("api/mall/orders")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IEventBus eventBus,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="request">创建订单请求</param>
        /// <returns>订单信息</returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] SubmitOrderRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                // 提交订单
                var order = await _orderService.SubmitOrderAsync(userId, request);

                // 发布订单创建事件
                await _eventBus.PublishAsync(new OrderCreatedEvent
                {
                    OrderId = order.Id,
                    UserId = userId,
                    OrderNo = order.OrderNo,
                    TotalAmount = order.PayAmount
                });

                _logger.LogInformation("订单创建成功: OrderId={OrderId}, OrderNo={OrderNo}, UserId={UserId}",
                    order.Id, order.OrderNo, userId);

                return Ok(new
                {
                    data = order,
                    message = "订单创建成功"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败");
                return BadRequest(new { message = "创建订单失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="status">订单状态（可选）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>订单列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var query = new OrderListQueryDto
                {
                    Status = status,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _orderService.GetOrdersAsync(userId, query);
                return Ok(new
                {
                    data = result,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return BadRequest(new { message = "获取订单列表失败" });
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单详情</returns>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(string orderId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var order = await _orderService.GetOrderByIdAsync(userId, orderId);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                return Ok(new
                {
                    data = order,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败: {OrderId}", orderId);
                return BadRequest(new { message = "获取订单详情失败" });
            }
        }

        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        /// <param name="orderNo">订单号</param>
        /// <returns>订单详情</returns>
        /// <summary>
        /// 获取订单物流信息
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>物流信息</returns>
        [HttpGet("{orderId}/tracking")]
        public async Task<IActionResult> GetOrderTracking(string orderId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var tracking = await _orderService.GetOrderTrackingAsync(userId, orderId);
                if (tracking == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                return Ok(new
                {
                    data = tracking,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单物流信息失败: {OrderId}", orderId);
                return BadRequest(new { message = "获取订单物流信息失败" });
            }
        }

        [HttpGet("by-no/{orderNo}")]
        public async Task<IActionResult> GetOrderByNo(string orderNo)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var order = await _orderService.GetOrderByNoAsync(userId, orderNo);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                return Ok(new
                {
                    data = order,
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败: {OrderNo}", orderNo);
                return BadRequest(new { message = "获取订单详情失败" });
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">取消请求</param>
        /// <returns></returns>
        [HttpPost("{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(string orderId, [FromBody] CancelOrderRequestDto? request = null)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var order = await _orderService.GetOrderByIdAsync(userId, orderId);
                if (order == null)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                var result = await _orderService.CancelOrderAsync(userId, orderId);
                if (!result)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                // 发布订单取消事件
                await _eventBus.PublishAsync(new OrderCancelledEvent
                {
                    OrderId = orderId,
                    OrderNo = order.OrderNo,
                    Reason = request?.Reason ?? "用户取消",
                    CancelledTime = DateTime.UtcNow
                });

                _logger.LogInformation("订单已取消: OrderId={OrderId}, UserId={UserId}", orderId, userId);

                return Ok(new { message = "订单已取消" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订单失败: {OrderId}", orderId);
                return BadRequest(new { message = "取消订单失败" });
            }
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        [HttpPost("{orderId}/confirm")]
        public async Task<IActionResult> ConfirmReceipt(string orderId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var result = await _orderService.ConfirmReceiptAsync(userId, orderId);
                if (!result)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                _logger.LogInformation("订单已确认收货: OrderId={OrderId}, UserId={UserId}", orderId, userId);

                return Ok(new { message = "已确认收货" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "确认收货失败: {OrderId}", orderId);
                return BadRequest(new { message = "确认收货失败" });
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> DeleteOrder(string orderId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var result = await _orderService.DeleteOrderAsync(userId, orderId);
                if (!result)
                {
                    return NotFound(new { message = "订单不存在" });
                }

                _logger.LogInformation("订单已删除: OrderId={OrderId}, UserId={UserId}", orderId, userId);

                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除订单失败: {OrderId}", orderId);
                return BadRequest(new { message = "删除订单失败" });
            }
        }

        /// <summary>
        /// 获取订单数量统计
        /// </summary>
        /// <returns>订单数量统计</returns>
        [HttpGet("count")]
        public async Task<IActionResult> GetOrderCount()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var query = new OrderListQueryDto { Page = 1, PageSize = 1 };

                var pendingCount = (await _orderService.GetOrdersAsync(userId, new OrderListQueryDto { Page = 1, PageSize = 1, Status = 0 })).Total;
                var paidCount = (await _orderService.GetOrdersAsync(userId, new OrderListQueryDto { Page = 1, PageSize = 1, Status = 1 })).Total;
                var shippedCount = (await _orderService.GetOrdersAsync(userId, new OrderListQueryDto { Page = 1, PageSize = 1, Status = 3 })).Total;
                var receivedCount = (await _orderService.GetOrdersAsync(userId, new OrderListQueryDto { Page = 1, PageSize = 1, Status = 4 })).Total;
                var completedCount = (await _orderService.GetOrdersAsync(userId, new OrderListQueryDto { Page = 1, PageSize = 1, Status = 5 })).Total;

                return Ok(new
                {
                    data = new
                    {
                        pendingCount,
                        paidCount,
                        shippedCount,
                        receivedCount,
                        completedCount
                    },
                    message = "获取成功"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单数量统计失败");
                return BadRequest(new { message = "获取订单数量统计失败" });
            }
        }
    }

    /// <summary>
    /// 取消订单请求
    /// </summary>
    public class CancelOrderRequestDto
    {
        /// <summary>
        /// 取消原因
        /// </summary>
        public string? Reason { get; set; }
    }
}
