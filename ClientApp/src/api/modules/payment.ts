import api, { buildAdminApiPath } from '../index'

export default {
  getList: (params: Api.Payment.PaymentOrderListParams) =>
    api.get<Api.Payment.PaymentOrderListResponse>(buildAdminApiPath('payments/orders'), { params }),

  getDetail: (paymentOrderNo: string) =>
    api.get<Api.Payment.PaymentOrder>(buildAdminApiPath(`payments/orders/${paymentOrderNo}`)),

  close: (paymentOrderNo: string) =>
    api.post(buildAdminApiPath(`payments/orders/${paymentOrderNo}/close`)),
}
