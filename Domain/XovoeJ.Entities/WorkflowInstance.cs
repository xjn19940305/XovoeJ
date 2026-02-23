using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流实例实体
    /// </summary>
    [Table("workflow_instances")]
    public class WorkflowInstance
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 工作流定义Code
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("workflow_code")]
        public string WorkflowCode { get; set; } = string.Empty;

        /// <summary>
        /// 工作流名称（冗余字段，方便查询）
        /// </summary>
        [MaxLength(128)]
        [Column("workflow_name")]
        public string WorkflowName { get; set; } = string.Empty;

        /// <summary>
        /// 发起人ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("initiator_id")]
        public string InitiatorId { get; set; } = string.Empty;

        /// <summary>
        /// 发起人姓名
        /// </summary>
        [MaxLength(64)]
        [Column("initiator_name")]
        public string? InitiatorName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 业务数据ID
        /// </summary>
        [MaxLength(64)]
        [Column("business_key")]
        public string? BusinessKey { get; set; }

        /// <summary>
        /// 业务数据类型
        /// </summary>
        [MaxLength(64)]
        [Column("business_type")]
        public string? BusinessType { get; set; }

        /// <summary>
        /// 当前步骤ID
        /// </summary>
        [MaxLength(64)]
        [Column("current_step_id")]
        public string? CurrentStepId { get; set; }

        /// <summary>
        /// 当前步骤名称
        /// </summary>
        [MaxLength(128)]
        [Column("current_step_name")]
        public string? CurrentStepName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column("status")]
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;

        /// <summary>
        /// 表单数据（JSON）
        /// </summary>
        [Column("form_data", TypeName = "jsonb")]
        public string? FormData { get; set; }

        /// <summary>
        /// 流程变量（JSON）
        /// </summary>
        [Column("variables", TypeName = "jsonb")]
        public Dictionary<string, object>? Variables { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }
}
