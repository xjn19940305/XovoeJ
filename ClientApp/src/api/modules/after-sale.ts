import api, { buildAdminApiPath } from '../index'

export default {
  getList: (params: Api.AfterSale.ListParams) => api.get<Api.AfterSale.ListResponse>(buildAdminApiPath('after-sales'), { params }),
  getDetail: (id: string) => api.get<Api.AfterSale.Detail>(buildAdminApiPath(`after-sales/${id}`)),
  approve: (id: string, data?: Api.AfterSale.AuditRequest) => api.post(buildAdminApiPath(`after-sales/${id}/approve`), data),
  reject: (id: string, data?: Api.AfterSale.AuditRequest) => api.post(buildAdminApiPath(`after-sales/${id}/reject`), data),
  refund: (id: string, data?: Api.AfterSale.RefundRequest) => api.post(buildAdminApiPath(`after-sales/${id}/refund`), data),
  exchange: (id: string, data: Api.AfterSale.ExchangeRequest) => api.post(buildAdminApiPath(`after-sales/${id}/exchange`), data),
}
