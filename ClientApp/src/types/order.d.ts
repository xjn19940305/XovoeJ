declare namespace Api {
  namespace Order {
    interface Order {
      id: string
      orderNo: string
      userId?: string
      userName?: string
      totalAmount: number
      status: number // 0-待付款 1-待发货 2-待收货 3-已完成 4-已取消
      statusText?: string
      receiverName?: string
      receiverPhone?: string
      receiverAddress?: string
      remark?: string
      createdAt?: string
      updatedAt?: string
      paidAt?: string
      shippedAt?: string
      completedAt?: string
    }

    interface OrderItem {
      id: string
      orderId: string
      productId: string
      productName: string
      productImage?: string
      price: number
      quantity: number
    }

    interface OrderDetail extends Order {
      items: OrderItem[]
    }

    interface OrderListParams {
      page: number
      pageSize: number
      keyword?: string
      status?: number
      startDate?: string
      endDate?: string
    }

    interface OrderListResponse {
      items: Order[]
      total: number
      page: number
      pageSize: number
    }

    interface UpdateOrderRequest {
      status?: number
      remark?: string
    }
  }
}
