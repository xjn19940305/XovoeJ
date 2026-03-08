using System.ComponentModel;

namespace XovoeJ.Enum
{
    public enum CouponStatus
    {
        [Description("可用")]
        Unused = 0,

        [Description("已使用")]
        Used = 1,

        [Description("已过期")]
        Expired = 2,

        [Description("已锁定")]
        Locked = 3,

        [Description("已作废")]
        Revoked = 4,
    }
}
