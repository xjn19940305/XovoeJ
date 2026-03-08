declare namespace Api {
  namespace Banner {
    interface BannerItem {
      id: string
      title: string
      subtitle?: string
      imageUrl: string
      linkUrl?: string
      sortOrder: number
      isEnabled: boolean
      startTime?: string
      endTime?: string
      remark?: string
      createdAt: string
      updatedAt?: string
    }

    interface BannerListResponse {
      items: BannerItem[]
      total: number
      page: number
      pageSize: number
    }

    interface SaveBannerRequest {
      title: string
      subtitle?: string
      imageUrl: string
      linkUrl?: string
      sortOrder: number
      isEnabled: boolean
      startTime?: string
      endTime?: string
      remark?: string
    }

    interface UpdateBannerStatusRequest {
      isEnabled: boolean
    }
  }
}
