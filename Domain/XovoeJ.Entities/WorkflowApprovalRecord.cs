using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流审批记录
    /// </summary>
    [Table("workflow_approval_records")]
    public class WorkflowApprovalRecord
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 工作流实例ID
        /// </summary>
        [Required]
        [Column("instance_id")]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("step_id")]
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 步骤名称
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("step_name")]
        public string StepName { get; set; } = string.Empty;

        /// <summary>
        /// 审批人ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("approver_id")]
        public string ApproverId { get; set; } = string.Empty;

        /// <summary>
        /// 审批人姓名
        /// </summary>
        [MaxLength(64)]
        [Column("approver_name")]
        public string? ApproverName { get; set; }

        /// <summary>
        /// 审批动作
        /// </summary>
        [Column("action")]
        public ApprovalAction Action { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>
        [Column("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// 附件列表（JSON数组）
        /// </summary>
        [Column("attachments", TypeName = "jsonb")]
        public string? Attachments { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        [Column("action_time")]
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;
    }
}
