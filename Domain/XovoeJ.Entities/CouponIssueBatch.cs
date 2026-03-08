using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 一次后台发券任务的批次记录。
    /// </summary>
    [Table("coupon_issue_batches")]
    public class CouponIssueBatch
    {
        /// <summary>
        /// 发券批次主键。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 发券目标类型，例如 all-users、specified-users、member-levels。
        /// </summary>
        [Required]
        [MaxLength(32)]
        [Column("target_type")]
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// 本次批次涉及的优惠券模板 ID 集合，JSON 序列化存储。
        /// </summary>
        [Required]
        [Column("coupon_template_ids_json")]
        public string CouponTemplateIdsJson { get; set; } = "[]";

        /// <summary>
        /// 指定用户发券时的目标用户 ID 集合，JSON 序列化存储。
        /// </summary>
        [Column("target_user_ids_json")]
        public string? TargetUserIdsJson { get; set; }

        /// <summary>
        /// 按会员等级发券时的目标等级编码集合，JSON 序列化存储。
        /// </summary>
        [Column("target_member_levels_json")]
        public string? TargetMemberLevelsJson { get; set; }

        /// <summary>
        /// 本批次理论应发放的记录数。
        /// </summary>
        [Column("requested_count")]
        public int RequestedCount { get; set; }

        /// <summary>
        /// 本批次成功发放的记录数。
        /// </summary>
        [Column("succeeded_count")]
        public int SucceededCount { get; set; }

        /// <summary>
        /// 本批次失败的记录数。
        /// </summary>
        [Column("failed_count")]
        public int FailedCount { get; set; }

        /// <summary>
        /// 批次状态，例如 pending、completed。
        /// </summary>
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "completed";

        /// <summary>
        /// 创建该批次的后台用户 ID。
        /// </summary>
        [MaxLength(64)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
