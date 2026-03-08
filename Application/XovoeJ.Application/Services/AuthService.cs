using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using XovoeJ.Abstractions.Services;
using XovoeJ.Application.Options;
using XovoeJ.Contracts.Auth;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly XovoeJDbContext _dbContext;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AuthService> _logger;
        private readonly IAssetAccountService _assetAccountService;

        public AuthService(
            UserManager<User> userManager,
            XovoeJDbContext dbContext,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AuthService> logger,
            IAssetAccountService assetAccountService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _jwtOptions = jwtOptions.Value;
            _logger = logger;
            _assetAccountService = assetAccountService;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? deviceInfo = null)
        {
            var user = await FindUserByAccountAsync(request.Account);
            if (user == null)
            {
                throw new UnauthorizedAccessException("账号或密码错误");
            }

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid)
            {
                throw new UnauthorizedAccessException("账号或密码错误");
            }

            // 更新最后登录时间
            user.LastSignDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // 生成Token
            var (accessToken, refreshToken) = await GenerateTokensAsync(user);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 默认30分钟过期
                User = await MapToUserInfoDto(user)
            };
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
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
                throw new ArgumentException($"注册失败: {errors}");
            }

            // 添加默认角色（前端用户）
            await _userManager.AddToRoleAsync(user, "前端用户");
            await _assetAccountService.EnsureUserAssetsAsync(user.Id);

            // 生成Token（注册后自动登录）
            var (accessToken, refreshToken) = await GenerateTokensAsync(user);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                User = await MapToUserInfoDto(user)
            };
        }

        /// <summary>
        /// 刷新令牌
        /// </summary>
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var refreshToken = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == request.UserId);

            if (refreshToken == null)
            {
                throw new UnauthorizedAccessException("刷新令牌无效");
            }

            if (refreshToken.IsRevoked || refreshToken.IsUsed)
            {
                throw new UnauthorizedAccessException("刷新令牌已失效");
            }

            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("刷新令牌已过期");
            }

            // 获取用户信息
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("用户不存在");
            }

            // 标记旧令牌为已使用
            refreshToken.IsUsed = true;
            refreshToken.UsedAt = DateTime.UtcNow;

            // 生成新Token
            var (accessToken, newRefreshToken) = await GenerateTokensAsync(user);

            // 设置新令牌替换旧令牌
            var newRefreshTokenEntity = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == newRefreshToken);
            if (newRefreshTokenEntity != null)
            {
                newRefreshTokenEntity.ReplacedByTokenId = refreshToken.Id;
            }

            await _dbContext.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                User = await MapToUserInfoDto(user)
            };
        }

        /// <summary>
        /// 登出
        /// </summary>
        public async Task LogoutAsync(string userId, string? refreshToken = null)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // 撤销指定的刷新令牌
                var token = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
                if (token != null)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // 撤销该用户的所有刷新令牌
                var tokens = await _dbContext.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();
                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public async Task<UserInfoDto?> GetUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return await MapToUserInfoDto(user);
        }

        /// <summary>
        /// 更新个人信息
        /// </summary>
        public async Task<UserInfoDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            // 更新字段
            if (request.NickName != null)
                user.NickName = request.NickName;
            if (request.Gender.HasValue)
                user.Gender = request.Gender.Value;
            if (request.RealName != null)
                user.RealName = request.RealName;
            if (request.Picture != null)
                user.Picture = request.Picture;
            if (request.Province != null)
                user.Province = request.Province;
            if (request.City != null)
                user.City = request.City;
            if (request.Area != null)
                user.Area = request.Area;
            if (request.Address != null)
                user.Address = request.Address;
            if (request.BirthDate.HasValue)
                user.BirthDate = request.BirthDate.Value;
            if (request.Notes != null)
                user.Notes = request.Notes;

            user.UpdateAt = DateTime.UtcNow;

            // 检查是否完善信息
            user.IsProfileCompleted = !string.IsNullOrEmpty(user.NickName) &&
                                       !string.IsNullOrEmpty(user.RealName) &&
                                       !string.IsNullOrEmpty(user.PhoneNumber);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"更新失败: {errors}");
            }

            return await MapToUserInfoDto(user);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new ArgumentException($"修改密码失败: {errors}");
            }

            // 撤销该用户的所有刷新令牌（强制重新登录）
            var tokens = await _dbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();

            return true;
        }

        #region 私有方法

        /// <summary>
        /// 根据账号查找用户（支持用户名、邮箱、手机号）
        /// </summary>
        private async Task<User?> FindUserByAccountAsync(string account)
        {
            // 先尝试按用户名查找
            var user = await _userManager.FindByNameAsync(account);
            if (user != null)
                return user;

            // 按邮箱查找
            user = await _userManager.FindByEmailAsync(account);
            if (user != null)
                return user;

            // 按手机号查找
            user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == account);
            return user;
        }

        /// <summary>
        /// 生成JWT访问令牌和刷新令牌
        /// </summary>
        private async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            // 获取用户角色和权限
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim("sub", user.Id ?? string.Empty),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            // 添加角色声明
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            // 获取角色对应的权限
            foreach (var roleName in roles)
            {
                var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    var roleClaims = await _dbContext.RoleClaims
                        .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                        .ToListAsync();
                    foreach (var claim in roleClaims)
                    {
                        claims.Add(new Claim("permission", claim.ClaimValue ?? string.Empty));
                    }
                }
            }

            // 添加自定义声明
            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            if (!string.IsNullOrEmpty(user.PhoneNumber))
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtOptions.SecretKey ?? throw new InvalidOperationException("JWT密钥未配置")));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: _jwtOptions.ValidIssuer,
                audience: _jwtOptions.ValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            var refreshToken = GenerateRefreshToken();

            // 保存刷新令牌到数据库
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                JwtId = jwtToken.Id,
                UserId = user.Id ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
                DeviceInfo = null
            };

            _dbContext.RefreshTokens.Add(refreshTokenEntity);
            await _dbContext.SaveChangesAsync();

            return (accessToken, refreshToken);
        }

        /// <summary>
        /// 生成安全的刷新令牌
        /// </summary>
        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// 映射用户实体到UserInfoDto
        /// </summary>
        private async Task<UserInfoDto> MapToUserInfoDto(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            // 获取角色对应的权限
            foreach (var roleName in roles)
            {
                var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    var roleClaims = await _dbContext.RoleClaims
                        .Where(rc => rc.RoleId == role.Id && rc.ClaimType == "permission")
                        .Select(rc => rc.ClaimValue)
                        .ToListAsync();
                    permissions.AddRange(roleClaims.Where(c => !string.IsNullOrEmpty(c))!);
                }
            }

            return new UserInfoDto
            {
                Id = user.Id ?? string.Empty,
                UserName = user.UserName,
                NickName = user.NickName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                RealName = user.RealName,
                Picture = user.Picture,
                Province = user.Province,
                City = user.City,
                Area = user.Area,
                Address = user.Address,
                BirthDate = user.BirthDate,
                IsProfileCompleted = user.IsProfileCompleted,
                CreatedAt = user.CreatedAt,
                LastSignDate = user.LastSignDate,
                Roles = roles.ToList(),
                Permissions = permissions.Distinct().ToList()
            };
        }

        #endregion
    }
}
