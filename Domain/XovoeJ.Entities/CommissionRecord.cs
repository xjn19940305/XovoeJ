using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("commission_records")]
    public class CommissionRecord
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [MaxLength(64)]
        [Column("promoter_id")]
        public string? PromoterId { get; set; }

        [MaxLength(64)]
        [Column("promoter_name")]
        public string? PromoterName { get; set; }

        [MaxLength(64)]
        [Column("order_no")]
        public string? OrderNo { get; set; }

        [MaxLength(128)]
        [Column("rule_name")]
        public string? RuleName { get; set; }

        [MaxLength(64)]
        [Column("source_type")]
        public string? SourceType { get; set; }

        [Column("commission_rate", TypeName = "decimal(10,2)")]
        public decimal CommissionRate { get; set; }

        [Column("estimated_amount", TypeName = "decimal(18,2)")]
        public decimal EstimatedAmount { get; set; }

        [Column("settled_amount", TypeName = "decimal(18,2)")]
        public decimal SettledAmount { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("settled_at")]
        public DateTime? SettledAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
