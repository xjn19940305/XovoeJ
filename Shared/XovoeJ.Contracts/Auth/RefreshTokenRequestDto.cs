using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 刷新令牌请求DTO
    /// </summary>
    public class RefreshTokenRequestDto
    {
        /// <summary>
        /// 刷新令牌
        /// </summary>
        [Required(ErrorMessage = "刷新令牌不能为空")]
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public string UserId { get; set; } = string.Empty;
    }
}
