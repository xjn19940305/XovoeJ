declare namespace Api {
  namespace Mall {
    interface UserAddress {
      id: string
      consigneeName: string
      mobile: string
      province?: string
      city?: string
      area?: string
      regionCode?: string
      detailAddress: string
      postalCode?: string
      label?: string
      isDefault: boolean
      sort: number
      fullAddress: string
      createdAt?: string
      updatedAt?: string
    }

    interface AccountSummary {
      userId: string
      nickName?: string
      realName?: string
      phoneNumber?: string
      picture?: string
      memberLevel: string
      totalSpent: number
      totalOrders: number
      pendingOrders: number
      shippedOrders: number
      completedOrders: number
      availableCouponCount: number
      defaultAddress?: UserAddress
    }

    interface CouponCenterItem {
      id: string
      name: string
      code: string
      couponType: number
      discountType: number
      discountValue: number
      minOrderAmount: number
      receiveLimit?: number
      description?: string
      startTime?: string
      endTime?: string
      claimedCount: number
    }

    interface UserCoupon {
      id: string
      couponTemplateId: string
      name: string
      couponType: number
      discountType: number
      discountValue: number
      minOrderAmount: number
      status: number
      claimedAt?: string
      usedAt?: string
      expiredAt?: string
    }

    interface PageParams {
      page: number
      pageSize: number
    }

    interface CouponCenterParams extends PageParams {
      keyword?: string
      couponType?: number
    }

    interface UserCouponParams extends PageParams {
      status?: number
    }

    interface PageResponse<T> {
      items: T[]
      total: number
      page: number
      pageSize: number
    }
  }
}
