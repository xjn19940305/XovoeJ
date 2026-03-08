using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("invite_relations")]
    public class InviteRelation
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [MaxLength(64)]
        [Column("inviter_id")]
        public string? InviterId { get; set; }

        [MaxLength(64)]
        [Column("invitee_id")]
        public string? InviteeId { get; set; }

        [MaxLength(64)]
        [Column("inviter_name")]
        public string? InviterName { get; set; }

        [MaxLength(64)]
        [Column("invitee_name")]
        public string? InviteeName { get; set; }

        [MaxLength(64)]
        [Column("referral_code")]
        public string? ReferralCode { get; set; }

        [MaxLength(32)]
        [Column("channel")]
        public string? Channel { get; set; }

        [MaxLength(64)]
        [Column("attribution_source")]
        public string? AttributionSource { get; set; }

        [Column("total_orders")]
        public int TotalOrders { get; set; }

        [Column("total_reward_amount", TypeName = "decimal(18,2)")]
        public decimal TotalRewardAmount { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("invited_at")]
        public DateTime? InvitedAt { get; set; }

        [Column("first_order_at")]
        public DateTime? FirstOrderAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
