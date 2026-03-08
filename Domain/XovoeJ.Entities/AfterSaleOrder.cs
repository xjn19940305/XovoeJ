using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 售后单
    /// </summary>
    [Table("after_sale_orders")]
    public class AfterSaleOrder
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [Required]
        [MaxLength(64)]
        [Column("after_sale_no")]
        public string AfterSaleNo { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("order_no")]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("type")]
        public int Type { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [MaxLength(100)]
        [Column("reason")]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [MaxLength(64)]
        [Column("contact_name")]
        public string? ContactName { get; set; }

        [MaxLength(32)]
        [Column("contact_mobile")]
        public string? ContactMobile { get; set; }

        [Column("refund_amount", TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [Column("original_order_status")]
        public int OriginalOrderStatus { get; set; }

        [Column("evidence_images", TypeName = "text")]
        public string? EvidenceImages { get; set; }

        [MaxLength(500)]
        [Column("admin_remark")]
        public string? AdminRemark { get; set; }

        [Column("applied_at")]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [Column("audited_at")]
        public DateTime? AuditedAt { get; set; }

        [Column("refunded_at")]
        public DateTime? RefundedAt { get; set; }

        [MaxLength(64)]
        [Column("exchange_shipping_company")]
        public string? ExchangeShippingCompany { get; set; }

        [MaxLength(64)]
        [Column("exchange_tracking_no")]
        public string? ExchangeTrackingNo { get; set; }

        [MaxLength(500)]
        [Column("exchange_remark")]
        public string? ExchangeRemark { get; set; }

        [Column("exchanged_at")]
        public DateTime? ExchangedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
