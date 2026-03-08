import api, { buildAdminApiPath } from '../index'

export default {
  getCouponList: (params: Api.Marketing.CouponListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.CouponTemplate>>(buildAdminApiPath('marketing/coupon-templates'), { params }),

  getCouponDetail: (id: string) => api.get<Api.Marketing.CouponTemplate>(buildAdminApiPath(`marketing/coupon-templates/${id}`)),

  getPromotionList: (params: Api.Marketing.PromotionListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.PromotionActivity>>(buildAdminApiPath('marketing/promotions'), { params }),

  getPromotionDetail: (id: string) => api.get<Api.Marketing.PromotionActivity>(buildAdminApiPath(`marketing/promotions/${id}`)),

  getSeckillList: (params: Api.Marketing.SeckillListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.SeckillActivity>>(buildAdminApiPath('marketing/seckills'), { params }),

  getSeckillDetail: (id: string) => api.get<Api.Marketing.SeckillActivity>(buildAdminApiPath(`marketing/seckills/${id}`)),

  getGroupBuyList: (params: Api.Marketing.GroupBuyListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.GroupBuyActivity>>(buildAdminApiPath('marketing/group-buys'), { params }),

  getGroupBuyDetail: (id: string) => api.get<Api.Marketing.GroupBuyActivity>(buildAdminApiPath(`marketing/group-buys/${id}`)),

  getBargainList: (params: Api.Marketing.BargainListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.BargainActivity>>(buildAdminApiPath('marketing/bargains'), { params }),

  getBargainDetail: (id: string) => api.get<Api.Marketing.BargainActivity>(buildAdminApiPath(`marketing/bargains/${id}`)),
}
