import api, { buildAdminApiPath } from '../index'

export default {
  // ========== 工作流定义管理 ==========

  // 获取工作流定义列表
  getDefinitions: (type?: string) => api.get<Api.Workflow.WorkflowDefinition[]>(buildAdminApiPath('workflow/definitions'), { params: { type } }),

  // 获取工作流定义详情
  getDefinition: (code: string) => api.get<Api.Workflow.WorkflowDefinition>(buildAdminApiPath(`workflow/definitions/${code}`)),

  // 创建工作流定义
  createDefinition: (data: Api.Workflow.CreateWorkflowDefinitionRequest) => api.post<Api.Workflow.WorkflowDefinition>(buildAdminApiPath('workflow/definitions'), data),

  // 更新工作流定义
  updateDefinition: (code: string, data: Api.Workflow.CreateWorkflowDefinitionRequest) => api.put<Api.Workflow.WorkflowDefinition>(buildAdminApiPath(`workflow/definitions/${code}`), data),

  // 删除工作流定义
  deleteDefinition: (code: string) => api.delete(buildAdminApiPath(`workflow/definitions/${code}`)),

  // ========== 工作流实例管理 ==========

  // 发起工作流
  startWorkflow: (data: Api.Workflow.StartWorkflowRequest) => api.post<Api.Workflow.WorkflowInstance>(buildAdminApiPath('workflow/start'), data),

  // 获取工作流实例详情
  getInstance: (instanceId: string) => api.get<Api.Workflow.WorkflowInstance>(buildAdminApiPath(`workflow/instances/${instanceId}`)),

  // 获取我发起的工作流列表
  getMyInstances: (params?: {
    workflowType?: string
    status?: Api.Workflow.WorkflowStatus
    page?: number
    pageSize?: number
  }) => api.get<{
    data: Api.Workflow.WorkflowInstance[]
    total: number
    page: number
    pageSize: number
  }>(buildAdminApiPath('workflow/instances/my'), { params }),

  // 获取待办任务列表
  getPendingTasks: (params?: {
    workflowType?: string
    status?: Api.Workflow.WorkflowStatus
    page?: number
    pageSize?: number
  }) => api.get<Api.Workflow.PendingTasksResponse>(buildAdminApiPath('workflow/pending'), { params }),

  // 获取待办数量统计
  getPendingCount: () => api.get<Record<string, number>>(buildAdminApiPath('workflow/pending/count')),

  // 处理审批
  processApproval: (instanceId: string, data: Api.Workflow.ApprovalRequest) => api.post(buildAdminApiPath(`workflow/instances/${instanceId}/approve`), data),

  // 撤回工作流
  withdrawWorkflow: (instanceId: string) => api.post(buildAdminApiPath(`workflow/instances/${instanceId}/withdraw`)),

  // 终止工作流
  terminateWorkflow: (instanceId: string, reason?: string) => api.post(buildAdminApiPath(`workflow/instances/${instanceId}/terminate`), { reason }),
}
