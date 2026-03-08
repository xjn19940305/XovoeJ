using Microsoft.EntityFrameworkCore;
using XovoeJ.Abstractions.Services;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 资产账户初始化服务。
    /// </summary>
    public class AssetAccountService : IAssetAccountService
    {
        private readonly XovoeJDbContext _dbContext;

        public AssetAccountService(XovoeJDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WalletAccount> EnsureWalletAccountAsync(string userId)
        {
            var account = await _dbContext.WalletAccounts
                .AsTracking()
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (account != null)
            {
                return account;
            }

            account = new WalletAccount
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedAt = DateTime.UtcNow,
            };

            _dbContext.WalletAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<PointsAccount> EnsurePointsAccountAsync(string userId)
        {
            var account = await _dbContext.PointsAccounts
                .AsTracking()
                .FirstOrDefaultAsync(item => item.UserId == userId);

            if (account != null)
            {
                return account;
            }

            account = new PointsAccount
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastChangedAt = DateTime.UtcNow,
            };

            _dbContext.PointsAccounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task EnsureUserAssetsAsync(string userId)
        {
            await EnsureWalletAccountAsync(userId);
            await EnsurePointsAccountAsync(userId);
        }

        public async Task EnsureAllUserAssetsAsync()
        {
            var userIds = await _dbContext.Users
                .AsNoTracking()
                .Select(item => item.Id)
                .ToListAsync();

            foreach (var userId in userIds)
            {
                await EnsureUserAssetsAsync(userId);
            }
        }
    }
}
