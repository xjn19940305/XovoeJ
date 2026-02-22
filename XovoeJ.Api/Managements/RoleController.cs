using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XovoeJ.Abstractions.Services;
using XovoeJ.Api.Swaggers;
using XovoeJ.Contracts.Role;

namespace XovoeJ.Api.Managements
{
    /// <summary>
    /// 角色管理控制器
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Produces("application/json")]
    [Authorize]
    [ApiGroup(ApiGroupNames.MANAGEMENT)]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        /// <returns>角色列表</returns>
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _roleService.GetRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色列表失败");
                return BadRequest(new { message = "获取角色列表失败" });
            }
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>角色信息</returns>
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRoleById(string roleId)
        {
            try
            {
                var role = await _roleService.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    return NotFound(new { message = "角色不存在" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色信息失败: {RoleId}", roleId);
                return BadRequest(new { message = "获取角色信息失败" });
            }
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的角色信息</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(request);
                return Ok(role);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建角色失败: {RoleName}", request.Name);
                return BadRequest(new { message = "创建角色失败" });
            }
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的角色信息</returns>
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleRequestDto request)
        {
            try
            {
                var role = await _roleService.UpdateRoleAsync(roleId, request);
                if (role == null)
                {
                    return NotFound(new { message = "角色不存在" });
                }
                return Ok(role);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新角色失败: {RoleId}", roleId);
                return BadRequest(new { message = "更新角色失败" });
            }
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns></returns>
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            try
            {
                var result = await _roleService.DeleteRoleAsync(roleId);
                if (!result)
                {
                    return NotFound(new { message = "角色不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除角色失败: {RoleId}", roleId);
                return BadRequest(new { message = "删除角色失败" });
            }
        }

        /// <summary>
        /// 获取角色的权限代码列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>权限代码列表</returns>
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            try
            {
                var permissions = await _roleService.GetRolePermissionsAsync(roleId);
                return Ok(permissions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色权限失败: {RoleId}", roleId);
                return BadRequest(new { message = "获取角色权限失败" });
            }
        }

        /// <summary>
        /// 给角色分配权限
        /// </summary>
        /// <param name="request">分配权限请求</param>
        /// <returns></returns>
        [HttpPost("assign-permissions")]
        public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsRequestDto request)
        {
            try
            {
                await _roleService.AssignPermissionsAsync(request);
                return Ok(new { message = "权限分配成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分配权限失败: {RoleId}", request.RoleId);
                return BadRequest(new { message = "分配权限失败" });
            }
        }

        /// <summary>
        /// 获取当前登录用户的所有权限代码
        /// </summary>
        /// <returns>权限代码列表</returns>
        [HttpGet("permissions/my")]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "无效的用户信息" });
                }

                var permissions = await _roleService.GetUserPermissionCodesAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户权限失败");
                return BadRequest(new { message = "获取用户权限失败" });
            }
        }
    }
}
