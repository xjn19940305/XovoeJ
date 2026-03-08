using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("coupon_templates")]
    public class CouponTemplate
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

        [Column("coupon_type")]
        public int CouponType { get; set; }

        [Column("discount_type")]
        public int DiscountType { get; set; }

        [Column("discount_value", TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column("min_order_amount", TypeName = "decimal(18,2)")]
        public decimal MinOrderAmount { get; set; }

        [Column("total_quantity")]
        public int TotalQuantity { get; set; }

        [Column("issued_quantity")]
        public int IssuedQuantity { get; set; }

        [Column("used_quantity")]
        public int UsedQuantity { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("receive_limit")]
        public int? ReceiveLimit { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
