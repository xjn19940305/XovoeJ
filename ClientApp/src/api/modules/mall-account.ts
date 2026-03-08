import api, { buildMallApiPath } from '../index'

export default {
  getSummary: () => api.get<Api.Mall.AccountSummary>(buildMallApiPath('account/summary')),
  getAddresses: () => api.get<Api.Mall.UserAddress[]>(buildMallApiPath('addresses')),
}
