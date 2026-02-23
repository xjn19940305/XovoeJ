using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流待办项
    /// </summary>
    [Table("workflow_pending_items")]
    public class WorkflowPendingItem
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
        /// 是否已处理
        /// </summary>
        [Column("is_processed")]
        public bool IsProcessed { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
