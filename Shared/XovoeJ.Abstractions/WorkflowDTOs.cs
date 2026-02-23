using XovoeJ.Entities;

namespace XovoeJ.Abstractions
{
    /// <summary>
    /// 工作流定义DTO
    /// </summary>
    public class WorkflowDefinitionDto
    {
        /// <summary>
        /// 工作流编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工作流描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 工作流类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 步骤配置
        /// </summary>
        public List<WorkflowStepDefinition>? Steps { get; set; }

        /// <summary>
        /// 表单配置
        /// </summary>
        public object? FormConfig { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 使用次数统计
        /// </summary>
        public int UsageCount { get; set; }
    }

    /// <summary>
    /// 创建/更新工作流定义请求
    /// </summary>
    public class CreateWorkflowDefinitionRequest
    {
        /// <summary>
        /// 工作流编码（创建时自动生成，更新时忽略）
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工作流描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 工作流类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 步骤配置
        /// </summary>
        public List<WorkflowStepDefinition> Steps { get; set; } = new();

        /// <summary>
        /// 表单配置
        /// </summary>
        public object? FormConfig { get; set; }
    }

    /// <summary>
    /// 工作流实例DTO
    /// </summary>
    public class WorkflowInstanceDto
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 工作流Code
        /// </summary>
        public string WorkflowCode { get; set; } = string.Empty;

        /// <summary>
        /// 工作流名称
        /// </summary>
        public string WorkflowName { get; set; } = string.Empty;

        /// <summary>
        /// 发起人ID
        /// </summary>
        public string InitiatorId { get; set; } = string.Empty;

        /// <summary>
        /// 发起人姓名
        /// </summary>
        public string? InitiatorName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 业务数据ID
        /// </summary>
        public string? BusinessKey { get; set; }

        /// <summary>
        /// 业务数据类型
        /// </summary>
        public string? BusinessType { get; set; }

        /// <summary>
        /// 当前步骤ID
        /// </summary>
        public string? CurrentStepId { get; set; }

        /// <summary>
        /// 当前步骤名称
        /// </summary>
        public string? CurrentStepName { get; set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public WorkflowStatus Status { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public object? FormData { get; set; }

        /// <summary>
        /// 流程变量
        /// </summary>
        public Dictionary<string, object>? Variables { get; set; }

        /// <summary>
        /// 当前步骤的待办人数
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 已完成的步骤列表
        /// </summary>
        public List<CompletedStepDto> CompletedSteps { get; set; } = new();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// 已完成步骤DTO
    /// </summary>
    public class CompletedStepDto
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 审批记录
        /// </summary>
        public List<ApprovalRecordDto> Approvals { get; set; } = new();

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime CompletedAt { get; set; }
    }

    /// <summary>
    /// 审批记录DTO
    /// </summary>
    public class ApprovalRecordDto
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 步骤ID
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 审批人ID
        /// </summary>
        public string ApproverId { get; set; } = string.Empty;

        /// <summary>
        /// 审批人姓名
        /// </summary>
        public string? ApproverName { get; set; }

        /// <summary>
        /// 审批动作
        /// </summary>
        public ApprovalAction Action { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<string>? Attachments { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public DateTime ActionTime { get; set; }
    }

    /// <summary>
    /// 发起工作流请求
    /// </summary>
    public class StartWorkflowRequest
    {
        /// <summary>
        /// 工作流Code
        /// </summary>
        public string WorkflowCode { get; set; } = string.Empty;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 业务数据ID
        /// </summary>
        public string? BusinessKey { get; set; }

        /// <summary>
        /// 业务数据类型
        /// </summary>
        public string? BusinessType { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public object? FormData { get; set; }

        /// <summary>
        /// 流程变量
        /// </summary>
        public Dictionary<string, object>? Variables { get; set; }
    }

    /// <summary>
    /// 审批请求
    /// </summary>
    public class ApprovalRequest
    {
        /// <summary>
        /// 审批动作
        /// </summary>
        public ApprovalAction Action { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// 附件列表
        /// </summary>
        public List<string>? Attachments { get; set; }

        /// <summary>
        /// 转交人ID（Forward动作时使用）
        /// </summary>
        public string? ForwardToUserId { get; set; }
    }

    /// <summary>
    /// 待办列表查询请求
    /// </summary>
    public class PendingTasksQuery
    {
        /// <summary>
        /// 工作流类型（可选）
        /// </summary>
        public string? WorkflowType { get; set; }

        /// <summary>
        /// 状态（可选）
        /// </summary>
        public WorkflowStatus? Status { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 待办列表响应
    /// </summary>
    public class PendingTasksResponse
    {
        /// <summary>
        /// 待办列表
        /// </summary>
        public List<WorkflowInstanceDto> Items { get; set; } = new();

        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 待办数量统计
        /// </summary>
        public Dictionary<string, int> StatusCount { get; set; } = new();

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
    }
}
