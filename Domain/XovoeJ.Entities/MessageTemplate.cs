using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("message_templates")]
    public class MessageTemplate
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

        [Required]
        [MaxLength(32)]
        [Column("channel")]
        public string Channel { get; set; } = string.Empty;

        [MaxLength(64)]
        [Column("business_type")]
        public string? BusinessType { get; set; }

        [MaxLength(256)]
        [Column("subject")]
        public string? Subject { get; set; }

        [Column("content_preview")]
        public string? ContentPreview { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MessageTask> Tasks { get; set; } = new List<MessageTask>();

        public ICollection<MessageSendRecord> Records { get; set; } = new List<MessageSendRecord>();
    }
}
