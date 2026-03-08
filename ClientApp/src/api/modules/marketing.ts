import api, { buildAdminApiPath } from '../index'

export default {
  getCouponList: (params: Api.Marketing.CouponListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.CouponTemplate>>(buildAdminApiPath('marketing/coupon-templates'), { params }),

  getCouponDetail: (id: string) => api.get<Api.Marketing.CouponTemplate>(buildAdminApiPath(`marketing/coupon-templates/${id}`)),

  updateCouponStatus: (id: string, data: Api.Marketing.UpdateStatusRequest) => api.post(buildAdminApiPath(`marketing/coupon-templates/${id}/status`), data),

  getCouponIssueBatchList: (params: Api.Marketing.PageParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.CouponIssueBatch>>(buildAdminApiPath('marketing/coupon-issue-batches'), { params }),

  createCouponIssueBatch: (data: Api.Marketing.CreateCouponIssueBatchRequest) => api.post<Api.Marketing.CouponIssueBatch>(buildAdminApiPath('marketing/coupon-issue-batches'), data),

  getMemberLevelRewardRules: () => api.get<Api.Marketing.MemberLevelRewardRule[]>(buildAdminApiPath('marketing/member-level-reward-rules')),

  saveMemberLevelRewardRule: (data: Api.Marketing.SaveMemberLevelRewardRuleRequest) => api.post<Api.Marketing.MemberLevelRewardRule>(buildAdminApiPath('marketing/member-level-reward-rules'), data),

  getPromotionList: (params: Api.Marketing.PromotionListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.PromotionActivity>>(buildAdminApiPath('marketing/promotions'), { params }),

  getPromotionDetail: (id: string) => api.get<Api.Marketing.PromotionActivity>(buildAdminApiPath(`marketing/promotions/${id}`)),

  updatePromotionStatus: (id: string, data: Api.Marketing.UpdateStatusRequest) => api.post(buildAdminApiPath(`marketing/promotions/${id}/status`), data),

  getSeckillList: (params: Api.Marketing.SeckillListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.SeckillActivity>>(buildAdminApiPath('marketing/seckills'), { params }),

  getSeckillDetail: (id: string) => api.get<Api.Marketing.SeckillActivity>(buildAdminApiPath(`marketing/seckills/${id}`)),

  updateSeckillStatus: (id: string, data: Api.Marketing.UpdateStatusRequest) => api.post(buildAdminApiPath(`marketing/seckills/${id}/status`), data),

  getGroupBuyList: (params: Api.Marketing.GroupBuyListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.GroupBuyActivity>>(buildAdminApiPath('marketing/group-buys'), { params }),

  getGroupBuyDetail: (id: string) => api.get<Api.Marketing.GroupBuyActivity>(buildAdminApiPath(`marketing/group-buys/${id}`)),

  updateGroupBuyStatus: (id: string, data: Api.Marketing.UpdateStatusRequest) => api.post(buildAdminApiPath(`marketing/group-buys/${id}/status`), data),

  getBargainList: (params: Api.Marketing.BargainListParams) => api.get<Api.Marketing.PageResponse<Api.Marketing.BargainActivity>>(buildAdminApiPath('marketing/bargains'), { params }),

  getBargainDetail: (id: string) => api.get<Api.Marketing.BargainActivity>(buildAdminApiPath(`marketing/bargains/${id}`)),

  updateBargainStatus: (id: string, data: Api.Marketing.UpdateStatusRequest) => api.post(buildAdminApiPath(`marketing/bargains/${id}/status`), data),
}
