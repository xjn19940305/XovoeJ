using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    [Table("user_addresses")]
    public class UserAddress
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        [Column("consignee_name")]
        public string ConsigneeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [MaxLength(64)]
        [Column("province")]
        public string? Province { get; set; }

        [MaxLength(64)]
        [Column("city")]
        public string? City { get; set; }

        [MaxLength(64)]
        [Column("area")]
        public string? Area { get; set; }

        [MaxLength(32)]
        [Column("region_code")]
        public string? RegionCode { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("detail_address")]
        public string DetailAddress { get; set; } = string.Empty;

        [MaxLength(16)]
        [Column("postal_code")]
        public string? PostalCode { get; set; }

        [MaxLength(32)]
        [Column("label")]
        public string? Label { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; }

        [Column("sort")]
        public int Sort { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
