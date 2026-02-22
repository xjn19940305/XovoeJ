using System.ComponentModel.DataAnnotations;
using XovoeJ.Enum;

namespace XovoeJ.Contracts.User
{
    /// <summary>
    /// 更新用户请求DTO
    /// </summary>
    public class UpdateUserRequestDto
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(128, ErrorMessage = "昵称长度不能超过128个字符")]
        public string? NickName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [MaxLength(256, ErrorMessage = "邮箱长度不能超过256个字符")]
        public string? Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Phone(ErrorMessage = "手机号格式不正确")]
        [MaxLength(32, ErrorMessage = "手机号长度不能超过32个字符")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public GenderEnum? Gender { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(128, ErrorMessage = "真实姓名长度不能超过128个字符")]
        public string? RealName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [MaxLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
        public string? Picture { get; set; }
    }
}
