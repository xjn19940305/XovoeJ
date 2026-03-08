declare namespace Api {
  namespace Order {
    interface Order {
      id: string
      orderNo: string
      userId?: string
      userName?: string
      totalAmount: number
      status: number
      statusText?: string
      receiverName?: string
      receiverPhone?: string
      receiverAddress?: string
      remark?: string
      needInvoice?: boolean
      invoiceType?: number
      invoiceTitle?: string
      invoiceTaxNo?: string
      invoiceEmail?: string
      createdAt?: string
      updatedAt?: string
      paidAt?: string
      shippedAt?: string
      shippingCompany?: string
      trackingNo?: string
      shippingRemark?: string
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

    interface ShipOrderRequest {
      shippingCompany: string
      trackingNo: string
      shippingRemark?: string
    }
  }
}
