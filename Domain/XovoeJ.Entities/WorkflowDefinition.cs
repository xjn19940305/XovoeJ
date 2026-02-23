using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流定义实体
    /// </summary>
    [Table("workflow_definitions")]
    public class WorkflowDefinition
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 工作流编码
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 工作流名称
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工作流描述
        /// </summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 工作流类型（采购、报销等）
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 版本号
        /// </summary>
        [Column("version")]
        public int Version { get; set; } = 1;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 步骤配置（JSON）
        /// </summary>
        [Column("steps_config", TypeName = "jsonb")]
        public string? StepsConfig { get; set; }

        /// <summary>
        /// 表单配置（JSON）
        /// </summary>
        [Column("form_config", TypeName = "jsonb")]
        public string? FormConfig { get; set; }

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
    }
}
