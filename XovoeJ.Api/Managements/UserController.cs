using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.User;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 用户管理控制器
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="keyword">搜索关键字</param>
        /// <param name="roleName">角色名称筛选</param>
        /// <returns>用户列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? keyword = null,
            [FromQuery] string? roleName = null)
        {
            try
            {
                var query = new UserListQueryDto
                {
                    Page = page,
                    PageSize = pageSize,
                    Keyword = keyword,
                    RoleName = roleName
                };
                var result = await _userService.GetUsersAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户列表失败");
                return BadRequest(new { message = "获取用户列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败: {UserId}", userId);
                return BadRequest(new { message = "获取用户信息失败" });
            }
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的用户信息</returns>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户失败: {UserName}", request.UserName);
                return BadRequest(new { message = "创建用户失败" });
            }
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的用户信息</returns>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequestDto request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(userId, request);
                if (user == null)
                {
                    return NotFound(new { message = "用户不存在" });
                }
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户失败: {UserId}", userId);
                return BadRequest(new { message = "更新用户失败" });
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                if (!result)
                {
                    return NotFound(new { message = "用户不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败: {UserId}", userId);
                return BadRequest(new { message = "删除用户失败" });
            }
        }

        /// <summary>
        /// 给用户分配角色
        /// </summary>
        /// <param name="request">分配角色请求</param>
        /// <returns></returns>
        [HttpPost("assign-roles")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequestDto request)
        {
            try
            {
                await _userService.AssignRolesAsync(request);
                return Ok(new { message = "角色分配成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分配角色失败: {UserId}", request.UserId);
                return BadRequest(new { message = "分配角色失败" });
            }
        }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="newPassword">新密码</param>
        /// <returns></returns>
        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    return BadRequest(new { message = "密码长度不能少于6位" });
                }

                await _userService.ResetPasswordAsync(userId, newPassword);
                return Ok(new { message = "密码重置成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码失败: {UserId}", userId);
                return BadRequest(new { message = "重置密码失败" });
            }
        }

        /// <summary>
        /// 锁定用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpPost("{userId}/lock")]
        public async Task<IActionResult> LockUser(string userId)
        {
            try
            {
                // 锁定30天
                await _userService.SetLockoutAsync(userId, DateTime.UtcNow.AddDays(30));
                return Ok(new { message = "用户已锁定" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "锁定用户失败: {UserId}", userId);
                return BadRequest(new { message = "锁定用户失败" });
            }
        }

        /// <summary>
        /// 解锁用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpPost("{userId}/unlock")]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            try
            {
                await _userService.SetLockoutAsync(userId, null);
                return Ok(new { message = "用户已解锁" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解锁用户失败: {UserId}", userId);
                return BadRequest(new { message = "解锁用户失败" });
            }
        }
    }
}
