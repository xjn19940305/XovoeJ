import api from '../index'

export default {
  // 获取统计数据
  getStats: () => api.get<Api.Dashboard.StatsResponse>('api/dashboard/stats'),

  // 获取最近订单
  getRecentOrders: () => api.get<Api.Order.Order[]>('api/dashboard/recent-orders'),
}
