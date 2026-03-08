declare namespace Api {
  namespace Workflow {
    // 工作流实例状态
    enum WorkflowStatus {
      Pending = 0,
      InProgress = 1,
      Completed = 2,
      Rejected = 3,
      Withdrawn = 4,
      Terminated = 5,
    }

    // 步骤类型
    enum WorkflowStepType {
      Approval = 1,
      Copy = 2,
      Condition = 3,
      Parallel = 4,
      Merge = 5,
    }

    // 审批人类型
    enum ApproverType {
      SpecificUser = 1,
      Role = 2,
      DepartmentManager = 3,
      DirectLeader = 4,
      Dynamic = 5,
      SelfSelect = 6,
    }

    // 多人审批规则
    enum ApprovalRule {
      AllApprove = 1,
      AnyApprove = 2,
      Sequential = 3,
      Percentage = 4,
    }

    // 超时处理动作
    enum TimeoutAction {
      AutoApprove = 1,
      AutoReject = 2,
      Remind = 3,
      Escalate = 4,
    }

    // 审批动作
    enum ApprovalAction {
      Approve = 1,
      Reject = 2,
      Withdraw = 3,
      Forward = 4,
      Comment = 5,
    }

    // 表单字段权限
    interface FieldPermission {
      readable: boolean
      writable: boolean
    }

    // 工作流步骤定义
    interface WorkflowStepDefinition {
      id: string
      name: string
      description?: string
      type: WorkflowStepType
      approverType: ApproverType
      approverIds: string[]
      approvalRule: ApprovalRule
      timeoutHours?: number
      timeoutAction?: TimeoutAction
      order: number
      allowWithdraw: boolean
      fieldPermissions?: Record<string, FieldPermission>
    }

    // 工作流定义
    interface WorkflowDefinition {
      code: string
      name: string
      description?: string
      type: string
      version: number
      isEnabled: boolean
      steps?: WorkflowStepDefinition[]
      formConfig?: any
      createdAt: string
      updatedAt?: string
      usageCount: number
    }

    // 创建/更新工作流请求
    interface CreateWorkflowDefinitionRequest {
      code?: string
      name: string
      description?: string
      type: string
      steps: WorkflowStepDefinition[]
      formConfig?: any
    }

    interface UpdateWorkflowDefinitionStatusRequest {
      isEnabled: boolean
    }

    // 工作流实例
    interface WorkflowInstance {
      id: string
      workflowCode: string
      workflowName: string
      initiatorId: string
      initiatorName?: string
      title: string
      businessKey?: string
      businessType?: string
      currentStepId?: string
      currentStepName?: string
      status: WorkflowStatus
      formData?: any
      variables?: Record<string, any>
      pendingCount: number
      completedSteps: CompletedStep[]
      createdAt: string
      updatedAt?: string
      completedAt?: string
    }

    interface PendingTasksResponse {
      items: WorkflowInstance[]
      total: number
      page: number
      pageSize: number
      statusCount?: Record<string, number>
    }

    // 已完成步骤
    interface CompletedStep {
      stepId: string
      stepName: string
      approvals: ApprovalRecord[]
      completedAt: string
    }

    // 审批记录
    interface ApprovalRecord {
      id: string
      stepId: string
      stepName: string
      approverId: string
      approverName?: string
      action: ApprovalAction
      comment?: string
      attachments?: string[]
      actionTime: string
    }

    // 发起工作流请求
    interface StartWorkflowRequest {
      workflowCode: string
      title: string
      businessKey?: string
      businessType?: string
      formData?: any
      variables?: Record<string, any>
    }

    // 审批请求
    interface ApprovalRequest {
      action: ApprovalAction
      comment?: string
      attachments?: string[]
      forwardToUserId?: string
    }
  }
}
