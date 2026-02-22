using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 登录请求DTO
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// 账号（用户名或手机号或邮箱）
        /// </summary>
        [Required(ErrorMessage = "账号不能为空")]
        [MaxLength(128, ErrorMessage = "账号长度不能超过128个字符")]
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MaxLength(128, ErrorMessage = "密码长度不能超过128个字符")]
        public string Password { get; set; } = string.Empty;
    }
}
