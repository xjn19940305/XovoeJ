using XovoeJ.Contracts.User;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 用户服务接口
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>用户列表</returns>
        Task<UserListResponseDto> GetUsersAsync(UserListQueryDto query);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<UserDto?> GetUserByIdAsync(string userId);

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建的用户信息</returns>
        Task<UserDto> CreateUserAsync(CreateUserRequestDto request);

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新后的用户信息</returns>
        Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequestDto request);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// 给用户分配角色
        /// </summary>
        /// <param name="request">分配角色请求</param>
        /// <returns></returns>
        Task<bool> AssignRolesAsync(AssignRolesRequestDto request);

        /// <summary>
        /// 重置用户密码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="newPassword">新密码</param>
        /// <returns></returns>
        Task<bool> ResetPasswordAsync(string userId, string newPassword);

        /// <summary>
        /// 锁定/解锁用户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="lockoutEnd">锁定结束时间（null表示解锁）</param>
        /// <returns></returns>
        Task<bool> SetLockoutAsync(string userId, DateTime? lockoutEnd);
    }
}
