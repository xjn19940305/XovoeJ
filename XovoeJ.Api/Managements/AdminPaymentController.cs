using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Api.Managements
{
    [ApiController]
    [Route("api/admin/payments/orders")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AdminPaymentController : ControllerBase
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<AdminPaymentController> _logger;

        public AdminPaymentController(XovoeJDbContext dbContext, IPaymentService paymentService, ILogger<AdminPaymentController> logger)
        {
            _dbContext = dbContext;
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? keyword = null, [FromQuery] int? status = null)
        {
            try
            {
                var query =
                    from paymentOrder in _dbContext.PaymentOrders.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on paymentOrder.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    select new { paymentOrder, user };

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(item =>
                        item.paymentOrder.PaymentOrderNo.Contains(keyword)
                        || item.paymentOrder.OrderNo.Contains(keyword)
                        || item.paymentOrder.UserId.Contains(keyword)
                        || (item.user != null && (
                            (item.user.UserName != null && item.user.UserName.Contains(keyword))
                            || (item.user.NickName != null && item.user.NickName.Contains(keyword))
                            || (item.user.PhoneNumber != null && item.user.PhoneNumber.Contains(keyword)))));
                }

                if (status.HasValue)
                {
                    query = query.Where(item => (int)item.paymentOrder.Status == status.Value);
                }

                var total = await query.CountAsync();
                var items = await query
                    .OrderByDescending(item => item.paymentOrder.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(item => new AdminPaymentOrderDto
                    {
                        Id = item.paymentOrder.Id,
                        PaymentOrderNo = item.paymentOrder.PaymentOrderNo,
                        OrderId = item.paymentOrder.OrderId,
                        OrderNo = item.paymentOrder.OrderNo,
                        UserId = item.paymentOrder.UserId,
                        UserName = item.user != null ? (item.user.NickName ?? item.user.UserName ?? item.user.PhoneNumber) : null,
                        PaymentMethod = item.paymentOrder.PaymentMethod,
                        PayableAmount = item.paymentOrder.PayableAmount,
                        PaidAmount = item.paymentOrder.PaidAmount,
                        RefundedAmount = item.paymentOrder.RefundedAmount,
                        Status = item.paymentOrder.Status,
                        StatusText = item.paymentOrder.Status.ToString(),
                        ExpireAt = item.paymentOrder.ExpireAt,
                        PaidAt = item.paymentOrder.PaidAt,
                        ClosedAt = item.paymentOrder.ClosedAt,
                        FailureReason = item.paymentOrder.FailureReason,
                        CreatedAt = item.paymentOrder.CreatedAt,
                    })
                    .ToListAsync();

                return Ok(new { items, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载支付单列表失败");
                return BadRequest(new { message = "加载支付单列表失败" });
            }
        }

        [HttpGet("{paymentOrderNo}")]
        public async Task<IActionResult> GetDetail(string paymentOrderNo)
        {
            try
            {
                var result = await (
                    from paymentOrder in _dbContext.PaymentOrders.AsNoTracking()
                    join user in _dbContext.Users.AsNoTracking() on paymentOrder.UserId equals user.Id into users
                    from user in users.DefaultIfEmpty()
                    where paymentOrder.PaymentOrderNo == paymentOrderNo
                    select new AdminPaymentOrderDto
                    {
                        Id = paymentOrder.Id,
                        PaymentOrderNo = paymentOrder.PaymentOrderNo,
                        OrderId = paymentOrder.OrderId,
                        OrderNo = paymentOrder.OrderNo,
                        UserId = paymentOrder.UserId,
                        UserName = user != null ? (user.NickName ?? user.UserName ?? user.PhoneNumber) : null,
                        PaymentMethod = paymentOrder.PaymentMethod,
                        PayableAmount = paymentOrder.PayableAmount,
                        PaidAmount = paymentOrder.PaidAmount,
                        RefundedAmount = paymentOrder.RefundedAmount,
                        Status = paymentOrder.Status,
                        StatusText = paymentOrder.Status.ToString(),
                        ExpireAt = paymentOrder.ExpireAt,
                        PaidAt = paymentOrder.PaidAt,
                        ClosedAt = paymentOrder.ClosedAt,
                        FailureReason = paymentOrder.FailureReason,
                        CreatedAt = paymentOrder.CreatedAt,
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return NotFound(new { message = "支付单不存在" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载支付单详情失败: {PaymentOrderNo}", paymentOrderNo);
                return BadRequest(new { message = "加载支付单详情失败" });
            }
        }

        [HttpPost("{paymentOrderNo}/close")]
        public async Task<IActionResult> Close(string paymentOrderNo)
        {
            try
            {
                var result = await _paymentService.ClosePaymentOrderAsync(paymentOrderNo);
                if (!result)
                {
                    return NotFound(new { message = "支付单不存在" });
                }

                return Ok(new { message = "支付单关闭成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭支付单失败: {PaymentOrderNo}", paymentOrderNo);
                return BadRequest(new { message = "关闭支付单失败" });
            }
        }
    }

    public sealed class AdminPaymentOrderDto
    {
        public string Id { get; set; } = string.Empty;
        public string PaymentOrderNo { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RefundedAmount { get; set; }
        public PaymentOrderStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public DateTime? ExpireAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
