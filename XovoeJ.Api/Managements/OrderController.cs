using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Order;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 订单控制器
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="request">提交订单请求</param>
        /// <returns>订单信息</returns>
        [HttpPost("api/orders")]
        public async Task<IActionResult> SubmitOrder([FromBody] SubmitOrderRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var order = await _orderService.SubmitOrderAsync(userId, request);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交订单失败");
                return BadRequest(new { message = "提交订单失败" });
            }
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>订单列表</returns>
        [HttpGet("api/orders")]
        public async Task<IActionResult> GetOrders([FromQuery] OrderListQueryDto query)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var result = await _orderService.GetOrdersAsync(userId, query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return BadRequest(new { message = "获取订单列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单信息</returns>
        [HttpGet("api/orders/{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
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
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单信息失败: {OrderId}", orderId);
                return BadRequest(new { message = "获取订单信息失败" });
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns></returns>
        [HttpPost("api/orders/{orderId}/cancel")]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var result = await _orderService.CancelOrderAsync(userId, orderId);
                if (!result)
                {
                    return NotFound(new { message = "订单不存在" });
                }
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
        [HttpPost("api/orders/{orderId}/confirm")]
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
        [HttpDelete("api/orders/{orderId}")]
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
    }
}
