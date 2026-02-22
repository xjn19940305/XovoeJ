using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 优惠券状态
    /// </summary>
    public enum CouponStatus
    {
        /// <summary>
        /// 未使用
        /// </summary>
        [Description("未使用")]
        Unused = 0,

        /// <summary>
        /// 已使用
        /// </summary>
        [Description("已使用")]
        Used = 1,

        /// <summary>
        /// 已过期
        /// </summary>
        [Description("已过期")]
        Expired = 2
    }
}
