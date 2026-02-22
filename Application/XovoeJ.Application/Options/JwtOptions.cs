namespace XovoeJ.Application.Options
{
    /// <summary>
    /// JWT配置选项
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// 密钥
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// 颁发者
        /// </summary>
        public string? ValidIssuer { get; set; }

        /// <summary>
        /// 受众
        /// </summary>
        public string? ValidAudience { get; set; }

        /// <summary>
        /// 访问令牌过期时间（分钟）
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// 刷新令牌过期时间（天）
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;
    }
}
