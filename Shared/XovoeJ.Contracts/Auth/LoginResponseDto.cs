namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 登录响应DTO
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT访问令牌
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// 令牌过期时间（UTC）
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInfoDto User { get; set; } = new();
    }
}
