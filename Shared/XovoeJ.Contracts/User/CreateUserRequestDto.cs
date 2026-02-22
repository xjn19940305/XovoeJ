using System.ComponentModel.DataAnnotations;
using XovoeJ.Enum;

namespace XovoeJ.Contracts.User
{
    /// <summary>
    /// 创建用户请求DTO
    /// </summary>
    public class CreateUserRequestDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [MaxLength(128, ErrorMessage = "用户名长度不能超过128个字符")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
        [MaxLength(128, ErrorMessage = "密码长度不能超过128个字符")]
        public string Password { get; set; } = string.Empty;

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
        /// 角色列表
        /// </summary>
        public List<string>? Roles { get; set; }
    }
}
