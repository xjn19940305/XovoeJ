using XovoeJ.Contracts.Role;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 角色服务接口
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// 获取所有角色列表
        /// </summary>
        /// <returns>角色列表</returns>
        Task<List<RoleDto>> GetRolesAsync();

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>角色信息</returns>
        Task<RoleDto?> GetRoleByIdAsync(string roleId);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的角色信息</returns>
        Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的角色信息</returns>
        Task<RoleDto?> UpdateRoleAsync(string roleId, UpdateRoleRequestDto request);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns></returns>
        Task<bool> DeleteRoleAsync(string roleId);

        /// <summary>
        /// 获取角色的权限代码列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>权限代码列表</returns>
        Task<List<string>> GetRolePermissionsAsync(string roleId);

        /// <summary>
        /// 给角色分配权限
        /// </summary>
        /// <param name="request">分配权限请求</param>
        /// <returns></returns>
        Task<bool> AssignPermissionsAsync(AssignPermissionsRequestDto request);

        /// <summary>
        /// 获取用户的所有权限代码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>权限代码列表</returns>
        Task<List<string>> GetUserPermissionCodesAsync(string userId);
    }
}
