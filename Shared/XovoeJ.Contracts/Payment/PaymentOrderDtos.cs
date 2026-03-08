using XovoeJ.Enum;

namespace XovoeJ.Contracts.Payment
{
    /// <summary>
    /// 创建支付单请求。
    /// </summary>
    public class CreatePaymentOrderRequestDto
    {
        /// <summary>
        /// 订单 ID。
        /// </summary>
        public string OrderId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 支付单详情。
    /// </summary>
    public class PaymentOrderDto
    {
        /// <summary>
        /// 支付单 ID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 支付单号。
        /// </summary>
        public string PaymentOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 订单 ID。
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 订单号。
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID。
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式。
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// 应付金额。
        /// </summary>
        public decimal PayableAmount { get; set; }

        /// <summary>
        /// 实付金额。
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// 已退款金额。
        /// </summary>
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// 支付单状态。
        /// </summary>
        public PaymentOrderStatus Status { get; set; }

        /// <summary>
        /// 支付单状态描述。
        /// </summary>
        public string StatusDescription { get; set; } = string.Empty;

        /// <summary>
        /// 过期时间。
        /// </summary>
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 支付成功时间。
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// 关闭时间。
        /// </summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 最近失败原因。
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// 钱包当前可用余额快照。
        /// </summary>
        public decimal WalletAvailableBalance { get; set; }

        /// <summary>
        /// 积分当前可用快照。
        /// </summary>
        public int AvailablePoints { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
