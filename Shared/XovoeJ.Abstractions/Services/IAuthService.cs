using XovoeJ.Contracts.Auth;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <param name="deviceInfo">设备信息</param>
        /// <returns>登录响应</returns>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? deviceInfo = null);

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>登录响应（注册成功后自动登录）</returns>
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// 刷新令牌
        /// </summary>
        /// <param name="request">刷新令牌请求</param>
        /// <returns>新的登录响应</returns>
        Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="refreshToken">刷新令牌</param>
        /// <returns></returns>
        Task LogoutAsync(string userId, string? refreshToken = null);

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<UserInfoDto?> GetUserInfoAsync(string userId);

        /// <summary>
        /// 更新个人信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的用户信息</returns>
        Task<UserInfoDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">修改密码请求</param>
        /// <returns></returns>
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    }
}
