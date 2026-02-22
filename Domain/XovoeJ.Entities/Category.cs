using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 商品分类
    /// </summary>
    [Table("categories")]
    public class Category
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 分类名称
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父分类ID
        /// </summary>
        [MaxLength(64)]
        [Column("parent_id")]
        public string? ParentId { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        [Column("level")]
        public int Level { get; set; }

        /// <summary>
        /// 路径 (如: 1/2/3)
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
        /// 分类图片
        /// </summary>
        [MaxLength(512)]
        [Column("image")]
        public string? Image { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

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
        /// 父分类导航属性
        /// </summary>
        [ForeignKey(nameof(ParentId))]
        public Category? Parent { get; set; }

        /// <summary>
        /// 子分类集合
        /// </summary>
        public ICollection<Category> Children { get; set; } = new List<Category>();

        /// <summary>
        /// 商品集合
        /// </summary>
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
