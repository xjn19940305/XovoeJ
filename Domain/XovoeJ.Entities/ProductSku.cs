using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 商品SKU
    /// </summary>
    [Table("product_skus")]
    public class ProductSku
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// SKU编码
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("sku_code")]
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// 规格属性(JSON: {"颜色": "红色", "尺寸": "XL"})
        /// </summary>
        [Column("specs", TypeName = "jsonb")]
        public string? Specs { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        [Column("original_price", TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        [Column("cost_price", TypeName = "decimal(18,2)")]
        public decimal? CostPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        [Column("stock")]
        public int Stock { get; set; }

        /// <summary>
        /// 低库存预警
        /// </summary>
        [Column("low_stock")]
        public int LowStock { get; set; } = 10;

        /// <summary>
        /// 销量
        /// </summary>
        [Column("sales_count")]
        public int SalesCount { get; set; }

        /// <summary>
        /// SKU图片
        /// </summary>
        [MaxLength(512)]
        [Column("image")]
        public string? Image { get; set; }

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
        /// 商品导航属性
        /// </summary>
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }
    }
}
