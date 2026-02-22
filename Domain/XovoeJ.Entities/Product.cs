using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 商品SPU
    /// </summary>
    [Table("products")]
    public class Product
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 分类ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("category_id")]
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 副标题
        /// </summary>
        [MaxLength(256)]
        [Column("subtitle")]
        public string? Subtitle { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        [MaxLength(512)]
        [Column("main_image")]
        public string? MainImage { get; set; }

        /// <summary>
        /// 图片列表(JSON)
        /// </summary>
        [Column("images", TypeName = "jsonb")]
        public string? Images { get; set; }

        /// <summary>
        /// 详情内容(HTML)
        /// </summary>
        [Column("detail", TypeName = "text")]
        public string? Detail { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        [Column("sales_count")]
        public int SalesCount { get; set; }

        /// <summary>
        /// 浏览量
        /// </summary>
        [Column("browse_count")]
        public int BrowseCount { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 是否热门
        /// </summary>
        [Column("is_hot")]
        public bool IsHot { get; set; }

        /// <summary>
        /// 是否新品
        /// </summary>
        [Column("is_new")]
        public bool IsNew { get; set; }

        /// <summary>
        /// 是否推荐
        /// </summary>
        [Column("is_recommend")]
        public bool IsRecommend { get; set; }

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
        /// 分类导航属性
        /// </summary>
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        /// <summary>
        /// SKU集合
        /// </summary>
        public ICollection<ProductSku> Skus { get; set; } = new List<ProductSku>();
    }
}
