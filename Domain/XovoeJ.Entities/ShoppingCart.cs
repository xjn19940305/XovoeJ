using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 购物车
    /// </summary>
    [Table("shopping_cart")]
    public class ShoppingCart
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// SKU ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("sku_id")]
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        [Column("quantity")]
        public int Quantity { get; set; }

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
