using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 积分流水。
    /// </summary>
    [Table("points_logs")]
    public class PointsLog
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 积分账户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("points_account_id")]
        public string PointsAccountId { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 业务类型，例如 order_reward、order_refund。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("business_type")]
        public string BusinessType { get; set; } = string.Empty;

        /// <summary>
        /// 业务单号，例如订单号、支付单号。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("business_no")]
        public string BusinessNo { get; set; } = string.Empty;

        /// <summary>
        /// 幂等键，用于防止重复记账。
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("idempotency_key")]
        public string IdempotencyKey { get; set; } = string.Empty;

        /// <summary>
        /// 流水方向，例如 earn、spend、refund。
        /// </summary>
        [Required]
        [MaxLength(32)]
        [Column("direction")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// 变动前可用积分。
        /// </summary>
        [Column("before_available_points")]
        public int BeforeAvailablePoints { get; set; }

        /// <summary>
        /// 变动前冻结积分。
        /// </summary>
        [Column("before_frozen_points")]
        public int BeforeFrozenPoints { get; set; }

        /// <summary>
        /// 本次可用积分变动值，获得为正，消耗为负。
        /// </summary>
        [Column("change_available_points")]
        public int ChangeAvailablePoints { get; set; }

        /// <summary>
        /// 本次冻结积分变动值。
        /// </summary>
        [Column("change_frozen_points")]
        public int ChangeFrozenPoints { get; set; }

        /// <summary>
        /// 变动后可用积分。
        /// </summary>
        [Column("after_available_points")]
        public int AfterAvailablePoints { get; set; }

        /// <summary>
        /// 变动后冻结积分。
        /// </summary>
        [Column("after_frozen_points")]
        public int AfterFrozenPoints { get; set; }

        /// <summary>
        /// 备注。
        /// </summary>
        [MaxLength(500)]
        [Column("remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
