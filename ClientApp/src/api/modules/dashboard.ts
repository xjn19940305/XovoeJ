import api, { buildAdminApiPath } from '../index'

export default {
  // 获取统计数据
  getStats: () => api.get<Api.Dashboard.StatsResponse>(buildAdminApiPath('dashboard/stats')),

  // 获取最近订单
  getRecentOrders: () => api.get<Api.Order.Order[]>(buildAdminApiPath('dashboard/recent-orders')),
}
