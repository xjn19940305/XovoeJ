import api, { buildAdminApiPath } from '../index'

export default {
  getWallets: (params: Api.Asset.AssetListParams) =>
    api.get<Api.Asset.AssetListResponse<Api.Asset.WalletAccount>>(buildAdminApiPath('assets/wallets'), { params }),

  getWalletTransactions: (params: Api.Asset.AssetListParams) =>
    api.get<Api.Asset.AssetListResponse<Api.Asset.WalletTransaction>>(buildAdminApiPath('assets/wallet-transactions'), { params }),

  getPointsAccounts: (params: Api.Asset.AssetListParams) =>
    api.get<Api.Asset.AssetListResponse<Api.Asset.PointsAccount>>(buildAdminApiPath('assets/points-accounts'), { params }),

  getPointsLogs: (params: Api.Asset.AssetListParams) =>
    api.get<Api.Asset.AssetListResponse<Api.Asset.PointsLog>>(buildAdminApiPath('assets/points-logs'), { params }),
}
