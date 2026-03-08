declare namespace Api {
  namespace AfterSale {
    interface Item {
      id: string
      afterSaleNo: string
      orderId: string
      orderNo: string
      userId: string
      userName?: string
      type: number
      status: number
      reason: string
      refundAmount: number
      contactName?: string
      contactMobile?: string
      appliedAt: string
      auditedAt?: string
      refundedAt?: string
      updatedAt?: string
    }

    interface OrderItem {
      productId: string
      productName: string
      productImage?: string
      price: number
      quantity: number
    }

    interface Detail extends Item {
      description?: string
      adminRemark?: string
      exchangedAt?: string
      exchangeShippingCompany?: string
      exchangeTrackingNo?: string
      exchangeRemark?: string
      evidenceImages: string[]
      items: OrderItem[]
    }

    interface ListParams {
      page: number
      pageSize: number
      keyword?: string
      status?: number
      type?: number
    }

    interface ListResponse {
      items: Item[]
      total: number
      page: number
      pageSize: number
    }

    interface AuditRequest {
      adminRemark?: string
    }

    interface RefundRequest {
      refundAmount?: number
      adminRemark?: string
    }

    interface ExchangeRequest {
      shippingCompany: string
      trackingNo: string
      exchangeRemark?: string
      adminRemark?: string
    }
  }
}
