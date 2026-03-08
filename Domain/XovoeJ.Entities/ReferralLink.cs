using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("referral_links")]
    public class ReferralLink
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

        [MaxLength(64)]
        [Column("owner_id")]
        public string? OwnerId { get; set; }

        [MaxLength(64)]
        [Column("owner_name")]
        public string? OwnerName { get; set; }

        [MaxLength(32)]
        [Column("channel")]
        public string? Channel { get; set; }

        [MaxLength(256)]
        [Column("landing_path")]
        public string? LandingPath { get; set; }

        [MaxLength(512)]
        [Column("full_url")]
        public string? FullUrl { get; set; }

        [MaxLength(128)]
        [Column("attribution_source")]
        public string? AttributionSource { get; set; }

        [MaxLength(128)]
        [Column("campaign_name")]
        public string? CampaignName { get; set; }

        [Column("click_count")]
        public int ClickCount { get; set; }

        [Column("signup_count")]
        public int SignupCount { get; set; }

        [Column("first_order_count")]
        public int FirstOrderCount { get; set; }

        [Column("reward_amount", TypeName = "decimal(18,2)")]
        public decimal RewardAmount { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("expire_at")]
        public DateTime? ExpireAt { get; set; }

        [Column("last_visit_at")]
        public DateTime? LastVisitAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
