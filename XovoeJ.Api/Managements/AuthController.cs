using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Application.Services;
using XovoeJ.Contracts.Auth;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 认证控制器
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录令牌和用户信息</returns>
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var deviceInfo = Request.Headers["User-Agent"].ToString();
                var result = await _authService.LoginAsync(request, deviceInfo);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败: {Account}", request.Account);
                return BadRequest(new { message = "登录失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册成功后返回登录令牌和用户信息</returns>
        [HttpPost("register")]
        [ApiGroup(ApiGroupNames.USER)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败: {UserName}", request.UserName);
                return BadRequest(new { message = "注册失败，请稍后重试" });
            }
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <param name="request">刷新令牌请求</param>
        /// <returns>新的访问令牌</returns>
        [HttpPost("refresh-token")]

        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新令牌失败: {UserId}", request.UserId);
                return BadRequest(new { message = "刷新令牌失败，请重新登录" });
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="request">刷新令牌（可选）</param>
        /// <returns></returns>
        [HttpPost("logout")]
        [Authorize]

        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? request = null)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var refreshToken = request?.RefreshToken;
                await _authService.LogoutAsync(userId, refreshToken);
                return Ok(new { message = "退出成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "退出登录失败");
                return BadRequest(new { message = "退出登录失败" });
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>当前登录用户的信息</returns>
        [HttpGet("user-info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var userInfo = await _authService.GetUserInfoAsync(userId);
                if (userInfo == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return BadRequest(new { message = "获取用户信息失败" });
            }
        }

        /// <summary>
        /// 获取指定用户信息（管理员功能）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        [HttpGet("user-info/{userId}")]
        [Authorize]
        [ApiGroup(ApiGroupNames.MANAGEMENT)]
        public async Task<IActionResult> GetUserInfoById(string userId)
        {
            try
            {
                var userInfo = await _authService.GetUserInfoAsync(userId);
                if (userInfo == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败: {UserId}", userId);
                return BadRequest(new { message = "获取用户信息失败" });
            }
        }

        /// <summary>
        /// 更新个人信息
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的用户信息</returns>
        [HttpPut("profile")]
        [Authorize]

        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var userInfo = await _authService.UpdateProfileAsync(userId, request);
                if (userInfo == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }

                return Ok(userInfo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新个人信息失败");
                return BadRequest(new { message = "更新个人信息失败" });
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request">修改密码请求</param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [Authorize]

        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                await _authService.ChangePasswordAsync(userId, request);
                return Ok(new { message = "密码修改成功，请重新登录" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修改密码失败");
                return BadRequest(new { message = "修改密码失败" });
            }
        }
    }
}
