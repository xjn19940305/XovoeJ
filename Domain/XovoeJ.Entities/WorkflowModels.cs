using System.Text.Json.Serialization;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流步骤定义
    /// </summary>
    public class WorkflowStepDefinition
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 步骤描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 步骤类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WorkflowStepType Type { get; set; } = WorkflowStepType.Approval;

        /// <summary>
        /// 审批人类型
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApproverType ApproverType { get; set; } = ApproverType.SpecificUser;

        /// <summary>
        /// 审批人ID列表（用户ID、角色ID等）
        /// </summary>
        public List<string> ApproverIds { get; set; } = new();

        /// <summary>
        /// 审批规则（多人审批时的处理方式）
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ApprovalRule ApprovalRule { get; set; } = ApprovalRule.AllApprove;

        /// <summary>
        /// 超时时间（小时）
        /// </summary>
        public int? TimeoutHours { get; set; }

        /// <summary>
        /// 超时处理
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TimeoutAction? TimeoutAction { get; set; }

        /// <summary>
        /// 步骤顺序
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// 是否允许撤回
        /// </summary>
        public bool AllowWithdraw { get; set; } = true;

        /// <summary>
        /// 表单字段权限控制
        /// </summary>
        public Dictionary<string, FieldPermission>? FieldPermissions { get; set; }
    }

    /// <summary>
    /// 步骤类型
    /// </summary>
    public enum WorkflowStepType
    {
        /// <summary>
        /// 审批节点
        /// </summary>
        Approval = 1,

        /// <summary>
        /// 抄送节点
        /// </summary>
        Copy = 2,

        /// <summary>
        /// 条件分支
        /// </summary>
        Condition = 3,

        /// <summary>
        /// 并行网关
        /// </summary>
        Parallel = 4,

        /// <summary>
        /// 汇聚网关
        /// </summary>
        Merge = 5
    }

    /// <summary>
    /// 审批人类型
    /// </summary>
    public enum ApproverType
    {
        /// <summary>
        /// 指定用户
        /// </summary>
        SpecificUser = 1,

        /// <summary>
        /// 指定角色
        /// </summary>
        Role = 2,

        /// <summary>
        /// 部门主管
        /// </summary>
        DepartmentManager = 3,

        /// <summary>
        /// 发起人的直属领导
        /// </summary>
        DirectLeader = 4,

        /// <summary>
        /// 动态选择（表单中选择）
        /// </summary>
        Dynamic = 5,

        /// <summary>
        /// 自选审批人
        /// </summary>
        SelfSelect = 6
    }

    /// <summary>
    /// 多人审批规则
    /// </summary>
    public enum ApprovalRule
    {
        /// <summary>
        /// 需所有人同意
        /// </summary>
        AllApprove = 1,

        /// <summary>
        /// 任一人同意即可
        /// </summary>
        AnyApprove = 2,

        /// <summary>
        /// 按顺序依次审批
        /// </summary>
        Sequential = 3,

        /// <summary>
        /// 按比例同意（如50%以上）
        /// </summary>
        Percentage = 4
    }

    /// <summary>
    /// 超时处理动作
    /// </summary>
    public enum TimeoutAction
    {
        /// <summary>
        /// 自动通过
        /// </summary>
        AutoApprove = 1,

        /// <summary>
        /// 自动拒绝
        /// </summary>
        AutoReject = 2,

        /// <summary>
        /// 提醒审批人
        /// </summary>
        Remind = 3,

        /// <summary>
        /// 转交上级
        /// </summary>
        Escalate = 4
    }

    /// <summary>
    /// 审批动作类型
    /// </summary>
    public enum ApprovalAction
    {
        /// <summary>
        /// 同意/通过
        /// </summary>
        Approve = 1,

        /// <summary>
        /// 拒绝/驳回
        /// </summary>
        Reject = 2,

        /// <summary>
        /// 撤回
        /// </summary>
        Withdraw = 3,

        /// <summary>
        /// 转交/加签
        /// </summary>
        Forward = 4,

        /// <summary>
        /// 评论
        /// </summary>
        Comment = 5
    }

    /// <summary>
    /// 表单字段权限
    /// </summary>
    public class FieldPermission
    {
        /// <summary>
        /// 是否可读
        /// </summary>
        public bool Readable { get; set; } = true;

        /// <summary>
        /// 是否可写
        /// </summary>
        public bool Writable { get; set; } = false;
    }
}
