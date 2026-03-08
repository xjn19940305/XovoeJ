declare namespace Api {
  namespace Marketing {
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

    interface CouponTemplate {
      id: string
      name: string
      code: string
      couponType: number
      discountType: number
      discountValue: number
      minOrderAmount: number
      totalQuantity: number
      issuedQuantity: number
      usedQuantity: number
      status: number
      receiveLimit?: number
      description?: string
      startTime?: string
      endTime?: string
      createdAt?: string
      updatedAt?: string
    }

    interface CouponListParams extends PageParams {
      couponType?: number
    }

    type CouponIssueTargetType = 'all-users' | 'specified-users' | 'member-levels'

    interface CouponIssueBatch {
      id: string
      targetType: CouponIssueTargetType | string
      couponTemplateIds: string[]
      targetUserIds: string[]
      targetMemberLevelCodes: string[]
      requestedCount: number
      succeededCount: number
      failedCount: number
      status: string
      createdBy?: string
      createdAt: string
      updatedAt?: string
    }

    interface CreateCouponIssueBatchRequest {
      targetType: CouponIssueTargetType
      couponTemplateIds: string[]
      targetUserIds?: string[]
      targetMemberLevelCodes?: string[]
    }

    interface MemberLevelRewardRule {
      id: string
      levelCode: string
      levelName: string
      couponTemplateIds: string[]
      status: number
      sort: number
      description?: string
      createdAt: string
      updatedAt?: string
    }

    interface SaveMemberLevelRewardRuleRequest {
      levelCode: string
      levelName: string
      couponTemplateIds: string[]
      status: number
      sort: number
      description?: string
    }

    interface PromotionActivity {
      id: string
      name: string
      type: number
      scopeText?: string
      priority: number
      stackable: boolean
      orderCount: number
      participantCount: number
      status: number
      description?: string
      startTime?: string
      endTime?: string
      createdAt?: string
      updatedAt?: string
    }

    interface PromotionListParams extends PageParams {
      type?: number
    }

    interface SeckillActivity {
      id: string
      name: string
      code: string
      productName?: string
      skuName?: string
      coverImage?: string
      originalPrice: number
      seckillPrice: number
      totalStock: number
      lockedStock: number
      soldStock: number
      purchaseLimit: number
      participantCount: number
      orderCount: number
      status: number
      warmupStartTime?: string
      startTime?: string
      endTime?: string
      description?: string
      createdAt?: string
      updatedAt?: string
    }

    interface SeckillListParams extends PageParams {}

    interface GroupBuyActivity {
      id: string
      name: string
      code: string
      productName?: string
      skuName?: string
      originalPrice: number
      groupPrice: number
      groupSize: number
      purchaseLimit: number
      virtualGroupCount: number
      participantCount: number
      successGroupCount: number
      failedGroupCount: number
      groupLeaderReward: number
      status: number
      startTime?: string
      endTime?: string
      description?: string
      createdAt?: string
      updatedAt?: string
    }

    interface GroupBuyListParams extends PageParams {}

    interface BargainActivity {
      id: string
      name: string
      code: string
      productName?: string
      skuName?: string
      originalPrice: number
      floorPrice: number
      currentLowestPrice: number
      purchaseLimit: number
      participantCount: number
      helperCount: number
      successCount: number
      status: number
      startTime?: string
      endTime?: string
      description?: string
      createdAt?: string
      updatedAt?: string
    }

    interface BargainListParams extends PageParams {}

    interface UpdateStatusRequest {
      status: number
    }
  }
}
