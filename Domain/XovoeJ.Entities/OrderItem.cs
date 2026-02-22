using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 订单项
    /// </summary>
    [Table("order_items")]
    public class OrderItem
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称
        /// </summary>
        [MaxLength(256)]
        [Column("product_name")]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 商品图片
        /// </summary>
        [MaxLength(512)]
        [Column("product_image")]
        public string? ProductImage { get; set; }

        /// <summary>
        /// SKU ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("sku_id")]
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// SKU规格
        /// </summary>
        [Column("sku_specs", TypeName = "jsonb")]
        public string? SkuSpecs { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Column("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// 小计
        /// </summary>
        [Column("subtotal", TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 订单导航属性
        /// </summary>
        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }
}
