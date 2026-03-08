import api, { buildAdminApiPath } from '../index'

export default {
  // 获取订单列表
  getList: (params: Api.Order.OrderListParams) => api.get<Api.Order.OrderListResponse>(buildAdminApiPath('orders'), { params }),

  // 获取订单详情
  getDetail: (id: string) => api.get<Api.Order.OrderDetail>(buildAdminApiPath(`orders/${id}`)),

  // 更新订单
  update: (id: string, data: Api.Order.UpdateOrderRequest) => api.put(buildAdminApiPath(`orders/${id}`), data),

  // 删除订单
  delete: (id: string) => api.delete(buildAdminApiPath(`orders/${id}`)),

  // 发货
  ship: (id: string, data: Api.Order.ShipOrderRequest) => api.post(buildAdminApiPath(`orders/${id}/ship`), data),
}
