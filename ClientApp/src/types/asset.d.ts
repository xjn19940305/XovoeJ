declare namespace Api {
  namespace Asset {
    interface WalletAccount {
      id: string
      userId: string
      userName?: string
      availableBalance: number
      frozenBalance: number
      totalIncome: number
      totalExpense: number
      lastChangedAt?: string
      createdAt: string
      updatedAt?: string
    }

    interface WalletTransaction {
      id: string
      userId: string
      userName?: string
      businessType: string
      businessNo: string
      direction: string
      changeAvailableBalance: number
      beforeAvailableBalance: number
      afterAvailableBalance: number
      remark?: string
      createdAt: string
    }

    interface PointsAccount {
      id: string
      userId: string
      userName?: string
      availablePoints: number
      frozenPoints: number
      totalEarnedPoints: number
      totalSpentPoints: number
      lastChangedAt?: string
      createdAt: string
      updatedAt?: string
    }

    interface PointsLog {
      id: string
      userId: string
      userName?: string
      businessType: string
      businessNo: string
      direction: string
      changeAvailablePoints: number
      beforeAvailablePoints: number
      afterAvailablePoints: number
      remark?: string
      createdAt: string
    }

    interface AssetListParams {
      page: number
      pageSize: number
      keyword?: string
      businessType?: string
    }

    interface AssetListResponse<T> {
      items: T[]
      total: number
      page: number
      pageSize: number
    }
  }
}
