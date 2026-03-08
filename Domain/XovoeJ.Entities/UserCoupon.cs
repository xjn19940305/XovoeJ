using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 用户实际持有的优惠券。
    /// </summary>
    [Table("user_coupons")]
    public class UserCoupon
    {
        /// <summary>
        /// 用户券主键。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 券归属的用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 来源优惠券模板 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("coupon_template_id")]
        public string CouponTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 当前用户券状态，例如可用、已使用、已过期。
        /// </summary>
        [Column("status")]
        public CouponStatus Status { get; set; } = CouponStatus.Unused;

        /// <summary>
        /// 发券来源类型，例如 claim、all-users、member_upgrade。
        /// </summary>
        [MaxLength(32)]
        [Column("source_type")]
        public string SourceType { get; set; } = "manual";

        /// <summary>
        /// 发券来源关联值，例如批次 ID 或会员等级编码。
        /// </summary>
        [MaxLength(128)]
        [Column("source_reference")]
        public string? SourceReference { get; set; }

        /// <summary>
        /// 发券时快照下来的优惠券名称，避免模板改名影响历史数据。
        /// </summary>
        [MaxLength(128)]
        [Column("snapshot_name")]
        public string SnapshotName { get; set; } = string.Empty;

        /// <summary>
        /// 发券时快照下来的优惠券类型。
        /// </summary>
        [Column("snapshot_coupon_type")]
        public int SnapshotCouponType { get; set; }

        /// <summary>
        /// 发券时快照下来的折扣类型。
        /// </summary>
        [Column("snapshot_discount_type")]
        public int SnapshotDiscountType { get; set; }

        /// <summary>
        /// 发券时快照下来的优惠值。
        /// </summary>
        [Column("snapshot_discount_value", TypeName = "decimal(18,2)")]
        public decimal SnapshotDiscountValue { get; set; }

        /// <summary>
        /// 发券时快照下来的最低使用门槛。
        /// </summary>
        [Column("snapshot_min_order_amount", TypeName = "decimal(18,2)")]
        public decimal SnapshotMinOrderAmount { get; set; }

        /// <summary>
        /// 发放时间。
        /// </summary>
        [Column("issued_at")]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 领取时间；后台直接发券时通常与发放时间相同。
        /// </summary>
        [Column("claimed_at")]
        public DateTime? ClaimedAt { get; set; }

        /// <summary>
        /// 实际核销使用时间。
        /// </summary>
        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// 到期时间。
        /// </summary>
        [Column("expired_at")]
        public DateTime? ExpiredAt { get; set; }

        /// <summary>
        /// 使用该券的订单 ID。
        /// </summary>
        [MaxLength(64)]
        [Column("order_id")]
        public string? OrderId { get; set; }

        /// <summary>
        /// 使用该券的订单号。
        /// </summary>
        [MaxLength(64)]
        [Column("order_no")]
        public string? OrderNo { get; set; }

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
