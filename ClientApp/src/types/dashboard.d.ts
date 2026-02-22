declare namespace Api {
  namespace Dashboard {
    interface StatsResponse {
      // 今日统计
      todayOrders: number
      todayUsers: number
      todaySalesAmount: number
      // 本月统计
      monthSalesAmount: number
      // 订单统计
      totalOrders: number
      pendingPaymentOrders: number
      pendingShipmentOrders: number
      shippedOrders: number
      completedOrders: number
      cancelledOrders: number
      // 商品统计
      totalProducts: number
      onSaleProducts: number
      outOfStockProducts: number
      // 用户统计
      totalUsers: number
      todayUsers: number
      thisWeekUsers: number
      thisMonthUsers: number
    }
  }
}
