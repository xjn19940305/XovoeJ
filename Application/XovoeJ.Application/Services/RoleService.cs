using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Role;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 角色服务实现
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RoleService> _logger;

        public RoleService(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            ILogger<RoleService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        public async Task<List<RoleDto>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Sort ?? 0)
                .ThenBy(r => r.Name)
                .ToListAsync();

            return roles.Select(MapToRoleDto).ToList();
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        public async Task<RoleDto?> GetRoleByIdAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            return role != null ? MapToRoleDto(role) : null;
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request)
        {
            var role = new Role
            {
                Name = request.Name,
                Description = request.Description,
                Sort = request.Sort,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"创建角色失败: {errors}");
            }

            return MapToRoleDto(role);
        }

        /// <summary>
        /// 更新角色
        /// </summary>
        public async Task<RoleDto?> UpdateRoleAsync(string roleId, UpdateRoleRequestDto request)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return null;
            }

            role.Name = request.Name;
            role.Description = request.Description;
            role.Sort = request.Sort;
            role.UpdateAt = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"更新角色失败: {errors}");
            }

            return MapToRoleDto(role);
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return false;
            }

            // 检查是否有用户使用该角色
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? string.Empty);
            if (usersInRole.Any())
            {
                throw new ArgumentException("该角色下还有用户，无法删除");
            }

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }

        /// <summary>
        /// 获取角色的权限代码列表
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                throw new ArgumentException("角色不存在");
            }

            // 获取角色的所有权限声明
            var claims = await _roleManager.GetClaimsAsync(role);
            return claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value ?? string.Empty)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList();
        }

        /// <summary>
        /// 给角色分配权限
        /// </summary>
        public async Task<bool> AssignPermissionsAsync(AssignPermissionsRequestDto request)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new ArgumentException("角色不存在");
            }

            // 获取现有的权限声明
            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingPermissionClaims = existingClaims
                .Where(c => c.Type == "permission")
                .ToList();

            // 移除所有现有的权限声明
            foreach (var claim in existingPermissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            // 添加新的权限声明
            foreach (var permissionCode in request.PermissionCodes)
            {
                if (!string.IsNullOrEmpty(permissionCode))
                {
                    await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permissionCode));
                }
            }

            return true;
        }

        /// <summary>
        /// 获取用户的所有权限代码
        /// </summary>
        public async Task<List<string>> GetUserPermissionCodesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissionCodes = new List<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var rolePermissions = claims
                        .Where(c => c.Type == "permission")
                        .Select(c => c.Value ?? string.Empty)
                        .Where(v => !string.IsNullOrEmpty(v));
                    permissionCodes.AddRange(rolePermissions);
                }
            }

            return permissionCodes.Distinct().ToList();
        }

        #region 私有方法

        /// <summary>
        /// 映射角色实体到DTO
        /// </summary>
        private static RoleDto MapToRoleDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id ?? string.Empty,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                Sort = role.Sort,
                CreatedAt = role.CreatedAt,
                UpdateAt = role.UpdateAt
            };
        }

        #endregion
    }
}
