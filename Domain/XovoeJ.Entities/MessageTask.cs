using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("message_tasks")]
    public class MessageTask
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [MaxLength(64)]
        [Column("template_id")]
        public string? TemplateId { get; set; }

        [Required]
        [MaxLength(128)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty;

        [MaxLength(64)]
        [Column("trigger_type")]
        public string? TriggerType { get; set; }

        [Column("recipient_count")]
        public int RecipientCount { get; set; }

        [Column("success_count")]
        public int SuccessCount { get; set; }

        [Column("failed_count")]
        public int FailedCount { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("scheduled_at")]
        public DateTime? ScheduledAt { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public MessageTemplate? Template { get; set; }

        public ICollection<MessageSendRecord> Records { get; set; } = new List<MessageSendRecord>();
    }
}
