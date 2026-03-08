import api, { buildAdminApiPath } from '../index'

export default {
  getDistributionList: (params: Api.Growth.DistributionListParams) => api.get<Api.Growth.PageResponse<Api.Growth.ReferralRelation>>(buildAdminApiPath('growth/distributions'), { params }),

  getDistributionDetail: (id: string) => api.get<Api.Growth.ReferralRelation>(buildAdminApiPath(`growth/distributions/${id}`)),

  updateDistributionStatus: (id: string, data: Api.Growth.UpdateStatusRequest) => api.post(buildAdminApiPath(`growth/distributions/${id}/status`), data),

  getReferralLinkList: (params: Api.Growth.ReferralLinkListParams) => api.get<Api.Growth.PageResponse<Api.Growth.ReferralLink>>(buildAdminApiPath('growth/referral-links'), { params }),

  getReferralLinkDetail: (id: string) => api.get<Api.Growth.ReferralLink>(buildAdminApiPath(`growth/referral-links/${id}`)),

  updateReferralLinkStatus: (id: string, data: Api.Growth.UpdateStatusRequest) => api.post(buildAdminApiPath(`growth/referral-links/${id}/status`), data),

  getCommissionList: (params: Api.Growth.CommissionListParams) => api.get<Api.Growth.PageResponse<Api.Growth.CommissionRecord>>(buildAdminApiPath('growth/commissions'), { params }),

  getCommissionDetail: (id: string) => api.get<Api.Growth.CommissionRecord>(buildAdminApiPath(`growth/commissions/${id}`)),

  settleCommission: (id: string) => api.post(buildAdminApiPath(`growth/commissions/${id}/settle`)),
}
