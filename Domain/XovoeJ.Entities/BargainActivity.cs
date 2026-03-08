using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("bargain_activities")]
    public class BargainActivity
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

        [Column("original_price", TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Column("floor_price", TypeName = "decimal(18,2)")]
        public decimal FloorPrice { get; set; }

        [Column("current_lowest_price", TypeName = "decimal(18,2)")]
        public decimal CurrentLowestPrice { get; set; }

        [Column("purchase_limit")]
        public int PurchaseLimit { get; set; }

        [Column("participant_count")]
        public int ParticipantCount { get; set; }

        [Column("helper_count")]
        public int HelperCount { get; set; }

        [Column("success_count")]
        public int SuccessCount { get; set; }

        [Column("status")]
        public int Status { get; set; }

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
