using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Payment;

namespace XovoeJ.Api.Controllers
{
    /// <summary>
    /// 商城支付单接口。
    /// </summary>
    [ApiController]
    [Route("api/mall/payments/orders")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.USER)]
    [Authorize]
    public class MallPaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public MallPaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentOrderRequestDto request)
        {
            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "无效的用户信息" });
            }

            var paymentOrder = await _paymentService.CreatePaymentOrderAsync(userId, request.OrderId);
            return Ok(new { data = paymentOrder, message = "获取支付单成功" });
        }

        [HttpGet("{paymentOrderNo}")]
        public async Task<IActionResult> Get(string paymentOrderNo)
        {
            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "无效的用户信息" });
            }

            var paymentOrder = await _paymentService.GetPaymentOrderAsync(userId, paymentOrderNo);
            if (paymentOrder == null)
            {
                return NotFound(new { message = "支付单不存在" });
            }

            return Ok(new { data = paymentOrder, message = "获取支付单成功" });
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrder(string orderId)
        {
            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "无效的用户信息" });
            }

            var paymentOrder = await _paymentService.GetPaymentOrderByOrderIdAsync(userId, orderId);
            if (paymentOrder == null)
            {
                return NotFound(new { message = "支付单不存在" });
            }

            return Ok(new { data = paymentOrder, message = "获取支付单成功" });
        }

        [HttpPost("{paymentOrderNo}/wallet-pay")]
        public async Task<IActionResult> WalletPay(string paymentOrderNo)
        {
            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "无效的用户信息" });
            }

            var paymentOrder = await _paymentService.PayByWalletAsync(userId, paymentOrderNo);
            return Ok(new { data = paymentOrder, message = "钱包支付成功" });
        }
    }
}
