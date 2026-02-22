using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 修改密码请求DTO
    /// </summary>
    public class ChangePasswordRequestDto
    {
        /// <summary>
        /// 旧密码
        /// </summary>
        [Required(ErrorMessage = "旧密码不能为空")]
        [MaxLength(128, ErrorMessage = "旧密码长度不能超过128个字符")]
        public string OldPassword { get; set; } = string.Empty;

        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        [MinLength(6, ErrorMessage = "新密码长度不能少于6位")]
        [MaxLength(128, ErrorMessage = "新密码长度不能超过128个字符")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
