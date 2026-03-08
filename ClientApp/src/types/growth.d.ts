declare namespace Api {
  namespace Growth {
    interface PageParams {
      page: number
      pageSize: number
      keyword?: string
      status?: number
    }

    interface PageResponse<T> {
      items: T[]
      total: number
      page: number
      pageSize: number
    }

    interface ReferralRelation {
      id: string
      inviterName?: string
      inviteeName?: string
      referralCode?: string
      channel?: string
      attributionSource?: string
      totalOrders: number
      totalRewardAmount: number
      status: number
      invitedAt?: string
      firstOrderAt?: string
      createdAt?: string
      updatedAt?: string
    }

    interface ReferralLink {
      id: string
      name: string
      code: string
      ownerName?: string
      channel?: string
      landingPath?: string
      fullUrl?: string
      attributionSource?: string
      campaignName?: string
      clickCount: number
      signupCount: number
      firstOrderCount: number
      rewardAmount: number
      status: number
      expireAt?: string
      lastVisitAt?: string
      createdAt?: string
      updatedAt?: string
    }

    interface CommissionRecord {
      id: string
      promoterName?: string
      orderNo?: string
      ruleName?: string
      sourceType?: string
      commissionRate: number
      estimatedAmount: number
      settledAmount: number
      status: number
      createdAt?: string
      settledAt?: string
      updatedAt?: string
    }

    interface DistributionListParams extends PageParams {}

    interface ReferralLinkListParams extends PageParams {
      channel?: string
    }

    interface CommissionListParams extends PageParams {}
  }
}
