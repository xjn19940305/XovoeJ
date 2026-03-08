using Microsoft.AspNetCore.Identity;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    public class User : IdentityUser
    {
        /// <summary>
        /// 用户昵称，前后端展示优先使用该字段。
        /// </summary>
        public string? NickName { get; set; }

        /// <summary>
        /// 用户性别。
        /// </summary>
        public GenderEnum? Gender { get; set; }

        /// <summary>
        /// 真实姓名。
        /// </summary>
        public string? RealName { get; set; }

        /// <summary>
        /// 英文名或名字拆分字段。
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// 英文姓氏或名字拆分字段。
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// 头像地址。
        /// </summary>
        public string? Picture { get; set; }

        /// <summary>
        /// 省份。
        /// </summary>
        public string? Province { get; set; }

        /// <summary>
        /// 城市。
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 区县。
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 详细地址。
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 出生日期。
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 最近一次资料更新时间。
        /// </summary>
        public DateTime? UpdateAt { get; set; }

        /// <summary>
        /// 账号创建时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 后台备注。
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// 最近登录时间。
        /// </summary>
        public DateTime? LastSignDate { get; set; }

        /// <summary>
        /// 是否已完善资料。
        /// </summary>
        public bool IsProfileCompleted { get; set; }

        /// <summary>
        /// 用户累计实付金额，用于会员等级计算。
        /// </summary>
        public decimal TotalSpentAmount { get; set; }

        /// <summary>
        /// 当前会员等级编码，例如 normal、silver、gold。
        /// </summary>
        public string? CurrentMemberLevelCode { get; set; }

        /// <summary>
        /// 已经发放过升级奖励的最高会员等级编码，用于避免重复送券。
        /// </summary>
        public string? RewardedMemberLevelCode { get; set; }
    }
}
