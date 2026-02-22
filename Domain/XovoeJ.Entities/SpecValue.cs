using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 规格值
    /// </summary>
    [Table("spec_values")]
    public class SpecValue
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 规格组ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("spec_group_id")]
        public string SpecGroupId { get; set; } = string.Empty;

        /// <summary>
        /// 规格值 (如: 红色、XL)
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

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
        /// 规格组导航属性
        /// </summary>
        [ForeignKey(nameof(SpecGroupId))]
        public SpecGroup? SpecGroup { get; set; }
    }
}
