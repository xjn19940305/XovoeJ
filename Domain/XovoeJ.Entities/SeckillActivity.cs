using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("seckill_activities")]
    public class SeckillActivity
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(128)]
        [Column("product_name")]
        public string? ProductName { get; set; }

        [MaxLength(128)]
        [Column("sku_name")]
        public string? SkuName { get; set; }

        [MaxLength(256)]
        [Column("cover_image")]
        public string? CoverImage { get; set; }

        [Column("original_price", TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Column("seckill_price", TypeName = "decimal(18,2)")]
        public decimal SeckillPrice { get; set; }

        [Column("total_stock")]
        public int TotalStock { get; set; }

        [Column("locked_stock")]
        public int LockedStock { get; set; }

        [Column("sold_stock")]
        public int SoldStock { get; set; }

        [Column("purchase_limit")]
        public int PurchaseLimit { get; set; }

        [Column("participant_count")]
        public int ParticipantCount { get; set; }

        [Column("order_count")]
        public int OrderCount { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("warmup_start_time")]
        public DateTime? WarmupStartTime { get; set; }

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
