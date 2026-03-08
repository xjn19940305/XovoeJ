using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 发券批次中的单条明细记录。
    /// </summary>
    [Table("coupon_issue_records")]
    public class CouponIssueRecord
    {
        /// <summary>
        /// 发券明细主键。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 所属发券批次 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("batch_id")]
        public string BatchId { get; set; } = string.Empty;

        /// <summary>
        /// 发券目标用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 实际发放的优惠券模板 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("coupon_template_id")]
        public string CouponTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 成功发券后生成的用户券 ID。
        /// </summary>
        [MaxLength(64)]
        [Column("user_coupon_id")]
        public string? UserCouponId { get; set; }

        /// <summary>
        /// 当前明细状态，例如 success、failed。
        /// </summary>
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "success";

        /// <summary>
        /// 发券失败时的错误信息。
        /// </summary>
        [MaxLength(512)]
        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
