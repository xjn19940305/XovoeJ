using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 钱包流水。
    /// </summary>
    [Table("wallet_transactions")]
    public class WalletTransaction
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 钱包账户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("wallet_account_id")]
        public string WalletAccountId { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 业务类型，例如 order_pay、order_refund。
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
        /// 流水方向，例如 income、expense、refund。
        /// </summary>
        [Required]
        [MaxLength(32)]
        [Column("direction")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// 变动前可用余额。
        /// </summary>
        [Column("before_available_balance", TypeName = "decimal(18,2)")]
        public decimal BeforeAvailableBalance { get; set; }

        /// <summary>
        /// 变动前冻结余额。
        /// </summary>
        [Column("before_frozen_balance", TypeName = "decimal(18,2)")]
        public decimal BeforeFrozenBalance { get; set; }

        /// <summary>
        /// 本次可用余额变动值，收入为正，支出为负。
        /// </summary>
        [Column("change_available_balance", TypeName = "decimal(18,2)")]
        public decimal ChangeAvailableBalance { get; set; }

        /// <summary>
        /// 本次冻结余额变动值。
        /// </summary>
        [Column("change_frozen_balance", TypeName = "decimal(18,2)")]
        public decimal ChangeFrozenBalance { get; set; }

        /// <summary>
        /// 变动后可用余额。
        /// </summary>
        [Column("after_available_balance", TypeName = "decimal(18,2)")]
        public decimal AfterAvailableBalance { get; set; }

        /// <summary>
        /// 变动后冻结余额。
        /// </summary>
        [Column("after_frozen_balance", TypeName = "decimal(18,2)")]
        public decimal AfterFrozenBalance { get; set; }

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
