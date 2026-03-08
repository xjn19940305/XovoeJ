declare namespace Api {
  namespace Payment {
    interface PaymentOrder {
      id: string
      paymentOrderNo: string
      orderId: string
      orderNo: string
      userId: string
      userName?: string
      paymentMethod?: string
      payableAmount: number
      paidAmount: number
      refundedAmount: number
      status: number
      statusText: string
      expireAt?: string
      paidAt?: string
      closedAt?: string
      failureReason?: string
      createdAt: string
    }

    interface PaymentOrderListParams {
      page: number
      pageSize: number
      keyword?: string
      status?: number
    }

    interface PaymentOrderListResponse {
      items: PaymentOrder[]
      total: number
      page: number
      pageSize: number
    }
  }
}
