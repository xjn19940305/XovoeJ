using Microsoft.EntityFrameworkCore;
using XovoeJ.Abstractions.Services;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 资产流水服务实现。
    /// </summary>
    public class AssetLedgerService : IAssetLedgerService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly IAssetAccountService _assetAccountService;

        public AssetLedgerService(XovoeJDbContext dbContext, IAssetAccountService assetAccountService)
        {
            _dbContext = dbContext;
            _assetAccountService = assetAccountService;
        }

        public Task<WalletTransaction> DeductWalletAsync(string userId, decimal amount, string businessType, string businessNo, string idempotencyKey, string? remark = null)
        {
            return ChangeWalletAsync(userId, -Math.Abs(amount), "expense", businessType, businessNo, idempotencyKey, remark);
        }

        public Task<WalletTransaction> RefundWalletAsync(string userId, decimal amount, string businessType, string businessNo, string idempotencyKey, string? remark = null)
        {
            return ChangeWalletAsync(userId, Math.Abs(amount), "refund", businessType, businessNo, idempotencyKey, remark);
        }

        public Task<PointsLog> AddPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null)
        {
            return ChangePointsAsync(userId, Math.Abs(points), "earn", businessType, businessNo, idempotencyKey, remark);
        }

        public Task<PointsLog> DeductPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null)
        {
            return ChangePointsAsync(userId, -Math.Abs(points), "spend", businessType, businessNo, idempotencyKey, remark);
        }

        public Task<PointsLog> RefundPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null)
        {
            return ChangePointsAsync(userId, Math.Abs(points), "refund", businessType, businessNo, idempotencyKey, remark);
        }

        private async Task<WalletTransaction> ChangeWalletAsync(string userId, decimal changeAmount, string direction, string businessType, string businessNo, string idempotencyKey, string? remark)
        {
            var existing = await _dbContext.WalletTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdempotencyKey == idempotencyKey);

            if (existing != null)
            {
                return existing;
            }

            await _assetAccountService.EnsureWalletAccountAsync(userId);

            var account = await _dbContext.WalletAccounts
                .AsTracking()
                .FirstAsync(item => item.UserId == userId);

            var afterAvailable = account.AvailableBalance + changeAmount;
            if (afterAvailable < 0)
            {
                throw new ArgumentException("钱包余额不足");
            }

            var transaction = new WalletTransaction
            {
                WalletAccountId = account.Id,
                UserId = userId,
                BusinessType = businessType,
                BusinessNo = businessNo,
                IdempotencyKey = idempotencyKey,
                Direction = direction,
                BeforeAvailableBalance = account.AvailableBalance,
                BeforeFrozenBalance = account.FrozenBalance,
                ChangeAvailableBalance = changeAmount,
                ChangeFrozenBalance = 0m,
                AfterAvailableBalance = afterAvailable,
                AfterFrozenBalance = account.FrozenBalance,
                Remark = remark,
                CreatedAt = DateTime.UtcNow,
            };

            account.AvailableBalance = afterAvailable;
            account.LastChangedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            if (changeAmount >= 0)
            {
                account.TotalIncome += changeAmount;
            }
            else
            {
                account.TotalExpense += Math.Abs(changeAmount);
            }

            _dbContext.WalletTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        private async Task<PointsLog> ChangePointsAsync(string userId, int changePoints, string direction, string businessType, string businessNo, string idempotencyKey, string? remark)
        {
            var existing = await _dbContext.PointsLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.IdempotencyKey == idempotencyKey);

            if (existing != null)
            {
                return existing;
            }

            await _assetAccountService.EnsurePointsAccountAsync(userId);

            var account = await _dbContext.PointsAccounts
                .AsTracking()
                .FirstAsync(item => item.UserId == userId);

            var afterAvailable = account.AvailablePoints + changePoints;
            if (afterAvailable < 0)
            {
                throw new ArgumentException("积分不足");
            }

            var log = new PointsLog
            {
                PointsAccountId = account.Id,
                UserId = userId,
                BusinessType = businessType,
                BusinessNo = businessNo,
                IdempotencyKey = idempotencyKey,
                Direction = direction,
                BeforeAvailablePoints = account.AvailablePoints,
                BeforeFrozenPoints = account.FrozenPoints,
                ChangeAvailablePoints = changePoints,
                ChangeFrozenPoints = 0,
                AfterAvailablePoints = afterAvailable,
                AfterFrozenPoints = account.FrozenPoints,
                Remark = remark,
                CreatedAt = DateTime.UtcNow,
            };

            account.AvailablePoints = afterAvailable;
            account.LastChangedAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            if (changePoints >= 0)
            {
                account.TotalEarnedPoints += changePoints;
            }
            else
            {
                account.TotalSpentPoints += Math.Abs(changePoints);
            }

            _dbContext.PointsLogs.Add(log);
            await _dbContext.SaveChangesAsync();
            return log;
        }
    }
}
