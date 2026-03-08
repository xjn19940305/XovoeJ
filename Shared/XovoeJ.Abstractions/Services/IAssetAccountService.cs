using XovoeJ.Entities;

namespace XovoeJ.Abstractions.Services
{
    /// <summary>
    /// 资产账户初始化服务。
    /// </summary>
    public interface IAssetAccountService
    {
        /// <summary>
        /// 确保用户钱包账户存在。
        /// </summary>
        Task<WalletAccount> EnsureWalletAccountAsync(string userId);

        /// <summary>
        /// 确保用户积分账户存在。
        /// </summary>
        Task<PointsAccount> EnsurePointsAccountAsync(string userId);

        /// <summary>
        /// 确保用户资产账户完整存在。
        /// </summary>
        Task EnsureUserAssetsAsync(string userId);

        /// <summary>
        /// 为所有缺失账户的用户补齐资产账户。
        /// </summary>
        Task EnsureAllUserAssetsAsync();
    }
}
