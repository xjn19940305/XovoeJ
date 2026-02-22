using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 优惠券类型
    /// </summary>
    public enum CouponType
    {
        /// <summary>
        /// 满减券
        /// </summary>
        [Description("满减券")]
        FixedAmount = 0,

        /// <summary>
        /// 折扣券
        /// </summary>
        [Description("折扣券")]
        Percent = 1,

        /// <summary>
        /// 无门槛券
        /// </summary>
        [Description("无门槛券")]
        NoThreshold = 2
    }
}
