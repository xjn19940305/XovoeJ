using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        [Description("待支付")]
        Pending = 0,

        /// <summary>
        /// 已支付
        /// </summary>
        [Description("已支付")]
        Paid = 1,

        /// <summary>
        /// 已发货
        /// </summary>
        [Description("已发货")]
        Shipped = 2,

        /// <summary>
        /// 已收货
        /// </summary>
        [Description("已收货")]
        Received = 3,

        /// <summary>
        /// 已完成
        /// </summary>
        [Description("已完成")]
        Completed = 4,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancelled = 5,

        /// <summary>
        /// 退款中
        /// </summary>
        [Description("退款中")]
        Refunding = 6
    }
}
