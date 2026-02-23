using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 字典分组 - 支持层级结构的配置分组
    /// </summary>
    [Table("dictionary_groups")]
    public class DictionaryGroup
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 分组编码 - 唯一标识，如 "system", "payment", "shipping" 等
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 字典类型
        /// </summary>
        [Column("type")]
        public DictionaryType Type { get; set; } = DictionaryType.System;

        /// <summary>
        /// 父分组ID - 支持层级结构
        /// </summary>
        [MaxLength(64)]
        [Column("parent_id")]
        public string? ParentId { get; set; }

        /// <summary>
        /// 层级 - 根节点为0，依次递增
        /// </summary>
        [Column("level")]
        public int Level { get; set; }

        /// <summary>
        /// 路径 - 如 "system/payment", 便于查询整个子树
        /// </summary>
        [MaxLength(512)]
        [Column("path")]
        public string? Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        [MaxLength(256)]
        [Column("icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序 - 数字越小越靠前
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        [Column("remark")]
        public string? Remark { get; set; }

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
        /// 父分组导航属性
        /// </summary>
        [ForeignKey(nameof(ParentId))]
        public DictionaryGroup? Parent { get; set; }

        /// <summary>
        /// 子分组集合
        /// </summary>
        public ICollection<DictionaryGroup> Children { get; set; } = new List<DictionaryGroup>();

        /// <summary>
        /// 字典项集合
        /// </summary>
        public ICollection<DictionaryItem> Items { get; set; } = new List<DictionaryItem>();
    }
}
