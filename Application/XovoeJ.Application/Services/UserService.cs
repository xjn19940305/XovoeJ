using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.User;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 用户服务实现
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly XovoeJDbContext _dbContext;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<User> userManager,
            XovoeJDbContext dbContext,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户列表（分页）
        /// </summary>
        public async Task<UserListResponseDto> GetUsersAsync(UserListQueryDto query)
        {
            var queryable = _userManager.Users.AsQueryable();

            // 关键字搜索
            if (!string.IsNullOrEmpty(query.Keyword))
            {
                queryable = queryable.Where(u =>
                    (u.UserName != null && u.UserName.Contains(query.Keyword)) ||
                    (u.NickName != null && u.NickName.Contains(query.Keyword)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(query.Keyword)) ||
                    (u.Email != null && u.Email.Contains(query.Keyword)));
            }

            // 总数
            var total = await queryable.CountAsync();

            // 分页
            var users = await queryable
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var items = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                items.Add(await MapToUserDto(user, roles.ToList()));
            }

            // 如果按角色筛选
            if (!string.IsNullOrEmpty(query.RoleName))
            {
                items = items.Where(u => u.Roles.Contains(query.RoleName)).ToList();
                total = items.Count;
            }

            return new UserListResponseDto
            {
                Items = items,
                Total = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await MapToUserDto(user, roles.ToList());
        }

        /// <summary>
        /// 创建用户
        /// </summary>
        public async Task<UserDto> CreateUserAsync(CreateUserRequestDto request)
        {
            // 检查用户名是否已存在
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                throw new ArgumentException("用户名已被使用");
            }

            // 检查邮箱是否已存在
            if (!string.IsNullOrEmpty(request.Email))
            {
                existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    throw new ArgumentException("邮箱已被使用");
                }
            }

            // 检查手机号是否已存在
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);
                if (existingUser != null)
                {
                    throw new ArgumentException("手机号已被使用");
                }
            }

            // 创建用户
            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                NickName = request.NickName ?? request.UserName,
                Gender = request.Gender,
                RealName = request.RealName,
                CreatedAt = DateTime.UtcNow,
                LastSignDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"创建用户失败: {errors}");
            }

            // 分配角色
            if (request.Roles != null && request.Roles.Any())
            {
                foreach (var roleName in request.Roles)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await MapToUserDto(user, roles.ToList());
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(string userId, UpdateUserRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // 更新字段
            if (request.Email != null)
                user.Email = request.Email;
            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;
            if (request.NickName != null)
                user.NickName = request.NickName;
            if (request.Gender.HasValue)
                user.Gender = request.Gender.Value;
            if (request.RealName != null)
                user.RealName = request.RealName;
            if (request.Picture != null)
                user.Picture = request.Picture;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"更新用户失败: {errors}");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return await MapToUserDto(user, roles.ToList());
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        /// <summary>
        /// 给用户分配角色
        /// </summary>
        public async Task<bool> AssignRolesAsync(AssignRolesRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            // 获取当前角色
            var currentRoles = await _userManager.GetRolesAsync(user);

            // 移除所有现有角色
            foreach (var role in currentRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            // 添加新角色
            foreach (var roleName in request.RoleNames)
            {
                if (!string.IsNullOrEmpty(roleName))
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }

            return true;
        }

        /// <summary>
        /// 重置用户密码
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            // 移除当前密码
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"重置密码失败: {errors}");
            }

            return true;
        }

        /// <summary>
        /// 锁定/解锁用户
        /// </summary>
        public async Task<bool> SetLockoutAsync(string userId, DateTime? lockoutEnd)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            return result.Succeeded;
        }

        #region 私有方法

        /// <summary>
        /// 映射用户实体到DTO
        /// </summary>
        private async Task<UserDto> MapToUserDto(User user, List<string>? roles = null)
        {
            roles ??= (await _userManager.GetRolesAsync(user)).ToList();

            return new UserDto
            {
                Id = user.Id ?? string.Empty,
                UserName = user.UserName,
                NickName = user.NickName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                RealName = user.RealName,
                Picture = user.Picture,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd?.DateTime,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                LastSignDate = user.LastSignDate
            };
        }

        #endregion
    }
}
