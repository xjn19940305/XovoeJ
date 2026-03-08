using XovoeJ.Entities;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 资产流水服务。
    /// </summary>
    public interface IAssetLedgerService
    {
        /// <summary>
        /// 扣减钱包余额并写流水。
        /// </summary>
        Task<WalletTransaction> DeductWalletAsync(string userId, decimal amount, string businessType, string businessNo, string idempotencyKey, string? remark = null);

        /// <summary>
        /// 回退钱包余额并写流水。
        /// </summary>
        Task<WalletTransaction> RefundWalletAsync(string userId, decimal amount, string businessType, string businessNo, string idempotencyKey, string? remark = null);

        /// <summary>
        /// 增加积分并写流水。
        /// </summary>
        Task<PointsLog> AddPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null);

        /// <summary>
        /// 扣减积分并写流水。
        /// </summary>
        Task<PointsLog> DeductPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null);

        /// <summary>
        /// 回退积分并写流水。
        /// </summary>
        Task<PointsLog> RefundPointsAsync(string userId, int points, string businessType, string businessNo, string idempotencyKey, string? remark = null);
    }
}
