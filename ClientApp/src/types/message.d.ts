declare namespace Api {
  namespace Message {
    type MessageChannel = 'inApp' | 'sms' | 'email' | 'push' | 'wechat'

    interface PageParams {
      page: number
      pageSize: number
      keyword?: string
      channel?: string
      status?: number
    }

    interface PageResponse<T> {
      items: T[]
      total: number
      page: number
      pageSize: number
    }

    interface MessageTemplate {
      id: string
      name: string
      code: string
      channel: string
      businessType?: string
      subject?: string
      contentPreview?: string
      description?: string
      status: number
      updatedAt?: string
      createdAt?: string
    }

    interface MessageTask {
      id: string
      name: string
      templateName?: string
      channel: string
      triggerType?: string
      recipientCount: number
      successCount: number
      failedCount: number
      status: number
      scheduledAt?: string
      sentAt?: string
      createdAt?: string
    }

    interface MessageRecord {
      id: string
      templateName?: string
      taskName?: string
      channel: string
      recipient?: string
      businessType?: string
      traceId?: string
      errorMessage?: string
      status: number
      sentAt?: string
      createdAt?: string
    }

    interface TemplateListParams extends PageParams {}

    interface TaskListParams extends PageParams {}

    interface RecordListParams extends PageParams {}

    interface UpdateStatusRequest {
      status: number
    }
  }
}
