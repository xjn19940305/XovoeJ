import api, { buildAdminApiPath } from '../index'

export default {
  getList: (params?: {
    page?: number
    pageSize?: number
    keyword?: string
    isEnabled?: boolean
  }) => api.get<Api.Banner.BannerListResponse>(buildAdminApiPath('content/banners'), { params }),

  getById: (bannerId: string) => api.get<Api.Banner.BannerItem>(buildAdminApiPath(`content/banners/${bannerId}`)),

  create: (data: Api.Banner.SaveBannerRequest) => api.post<Api.Banner.BannerItem>(buildAdminApiPath('content/banners'), data),

  update: (bannerId: string, data: Api.Banner.SaveBannerRequest) => api.put<Api.Banner.BannerItem>(buildAdminApiPath(`content/banners/${bannerId}`), data),

  updateStatus: (bannerId: string, data: Api.Banner.UpdateBannerStatusRequest) => api.post(buildAdminApiPath(`content/banners/${bannerId}/status`), data),

  remove: (bannerId: string) => api.delete(buildAdminApiPath(`content/banners/${bannerId}`)),
}
