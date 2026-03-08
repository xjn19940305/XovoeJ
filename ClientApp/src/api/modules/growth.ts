import api, { buildAdminApiPath } from '../index'

export default {
  getDistributionList: (params: Api.Growth.DistributionListParams) => api.get<Api.Growth.PageResponse<Api.Growth.ReferralRelation>>(buildAdminApiPath('growth/distributions'), { params }),

  getDistributionDetail: (id: string) => api.get<Api.Growth.ReferralRelation>(buildAdminApiPath(`growth/distributions/${id}`)),

  getReferralLinkList: (params: Api.Growth.ReferralLinkListParams) => api.get<Api.Growth.PageResponse<Api.Growth.ReferralLink>>(buildAdminApiPath('growth/referral-links'), { params }),

  getReferralLinkDetail: (id: string) => api.get<Api.Growth.ReferralLink>(buildAdminApiPath(`growth/referral-links/${id}`)),

  getCommissionList: (params: Api.Growth.CommissionListParams) => api.get<Api.Growth.PageResponse<Api.Growth.CommissionRecord>>(buildAdminApiPath('growth/commissions'), { params }),

  getCommissionDetail: (id: string) => api.get<Api.Growth.CommissionRecord>(buildAdminApiPath(`growth/commissions/${id}`)),
}
