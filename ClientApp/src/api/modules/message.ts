import api, { buildAdminApiPath } from '../index'

export default {
  getTemplateList: (params: Api.Message.TemplateListParams) => api.get<Api.Message.PageResponse<Api.Message.MessageTemplate>>(buildAdminApiPath('message/templates'), { params }),

  getTemplateDetail: (id: string) => api.get<Api.Message.MessageTemplate>(buildAdminApiPath(`message/templates/${id}`)),

  updateTemplateStatus: (id: string, data: Api.Message.UpdateStatusRequest) => api.post(buildAdminApiPath(`message/templates/${id}/status`), data),

  getTaskList: (params: Api.Message.TaskListParams) => api.get<Api.Message.PageResponse<Api.Message.MessageTask>>(buildAdminApiPath('message/tasks'), { params }),

  getTaskDetail: (id: string) => api.get<Api.Message.MessageTask>(buildAdminApiPath(`message/tasks/${id}`)),

  sendTask: (id: string) => api.post(buildAdminApiPath(`message/tasks/${id}/send`)),

  getRecordList: (params: Api.Message.RecordListParams) => api.get<Api.Message.PageResponse<Api.Message.MessageRecord>>(buildAdminApiPath('message/records'), { params }),

  getRecordDetail: (id: string) => api.get<Api.Message.MessageRecord>(buildAdminApiPath(`message/records/${id}`)),

  retryRecord: (id: string) => api.post(buildAdminApiPath(`message/records/${id}/retry`)),
}
