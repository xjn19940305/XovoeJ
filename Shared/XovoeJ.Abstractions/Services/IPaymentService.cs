using XovoeJ.Contracts.Payment;
using XovoeJ.Entities;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 支付服务。
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// 为订单创建或返回现有支付单。
        /// </summary>
        Task<PaymentOrderDto> CreatePaymentOrderAsync(string userId, string orderId);

        /// <summary>
        /// 查询支付单详情。
        /// </summary>
        Task<PaymentOrderDto?> GetPaymentOrderAsync(string userId, string paymentOrderNo);

        /// <summary>
        /// 查询订单关联的支付单。
        /// </summary>
        Task<PaymentOrderDto?> GetPaymentOrderByOrderIdAsync(string userId, string orderId);

        /// <summary>
        /// 使用钱包完成支付。
        /// </summary>
        Task<PaymentOrderDto> PayByWalletAsync(string userId, string paymentOrderNo);

        /// <summary>
        /// 关闭未支付支付单。
        /// </summary>
        Task<bool> ClosePaymentOrderAsync(string paymentOrderNo);

        /// <summary>
        /// 按订单执行退款回退。
        /// </summary>
        Task RefundOrderAsync(Order order, decimal refundAmount, string businessNo, string? remark = null);
    }
}
