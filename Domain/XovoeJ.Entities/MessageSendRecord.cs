using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("message_send_records")]
    public class MessageSendRecord
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [MaxLength(64)]
        [Column("template_id")]
        public string? TemplateId { get; set; }

        [MaxLength(64)]
        [Column("task_id")]
        public string? TaskId { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty;

        [MaxLength(128)]
        [Column("recipient")]
        public string? Recipient { get; set; }

        [MaxLength(64)]
        [Column("business_type")]
        public string? BusinessType { get; set; }

        [MaxLength(128)]
        [Column("trace_id")]
        public string? TraceId { get; set; }

        [MaxLength(1000)]
        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("sent_at")]
        public DateTime? SentAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(TemplateId))]
        public MessageTemplate? Template { get; set; }

        [ForeignKey(nameof(TaskId))]
        public MessageTask? Task { get; set; }
    }
}
