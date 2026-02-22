using XovoeJ.Enum;

namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 用户信息DTO
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string? NickName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public GenderEnum? Gender { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        public string? RealName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string? Picture { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string? Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 是否完善信息
        /// </summary>
        public bool IsProfileCompleted { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastSignDate { get; set; }

        /// <summary>
        /// 角色列表
        /// </summary>
        public List<string> Roles { get; set; } = new();

        /// <summary>
        /// 权限列表
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}
