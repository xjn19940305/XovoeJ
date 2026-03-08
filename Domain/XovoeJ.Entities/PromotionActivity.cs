using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("promotion_activities")]
    public class PromotionActivity
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("type")]
        public int Type { get; set; }

        [MaxLength(256)]
        [Column("scope_text")]
        public string? ScopeText { get; set; }

        [Column("priority")]
        public int Priority { get; set; }

        [Column("stackable")]
        public bool Stackable { get; set; }

        [Column("order_count")]
        public int OrderCount { get; set; }

        [Column("participant_count")]
        public int ParticipantCount { get; set; }

        [Column("status")]
        public int Status { get; set; }

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
