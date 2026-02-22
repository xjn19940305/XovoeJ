import api from '../index'

export default {
  // 获取订单列表
  getList: (params: Api.Order.OrderListParams) => api.get<Api.Order.OrderListResponse>('api/orders', { params }),

  // 获取订单详情
  getDetail: (id: string) => api.get<Api.Order.OrderDetail>(`api/orders/${id}`),

  // 更新订单
  update: (id: string, data: Api.Order.UpdateOrderRequest) => api.put(`api/orders/${id}`, data),

  // 删除订单
  delete: (id: string) => api.delete(`api/orders/${id}`),

  // 发货
  ship: (id: string) => api.post(`api/orders/${id}/ship`),
}
