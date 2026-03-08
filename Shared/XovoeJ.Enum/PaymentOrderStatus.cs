using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 支付单状态。
    /// </summary>
    public enum PaymentOrderStatus
    {
        /// <summary>
        /// 待支付。
        /// </summary>
        [Description("待支付")]
        Pending = 0,

        /// <summary>
        /// 已支付。
        /// </summary>
        [Description("已支付")]
        Paid = 1,

        /// <summary>
        /// 已关闭。
        /// </summary>
        [Description("已关闭")]
        Closed = 2,

        /// <summary>
        /// 部分退款。
        /// </summary>
        [Description("部分退款")]
        PartiallyRefunded = 3,

        /// <summary>
        /// 已退款。
        /// </summary>
        [Description("已退款")]
        Refunded = 4
    }
}
