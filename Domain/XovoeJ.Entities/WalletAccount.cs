using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 用户钱包账户快照。
    /// </summary>
    [Table("wallet_accounts")]
    public class WalletAccount
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 当前可用余额。
        /// </summary>
        [Column("available_balance", TypeName = "decimal(18,2)")]
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// 当前冻结余额。
        /// </summary>
        [Column("frozen_balance", TypeName = "decimal(18,2)")]
        public decimal FrozenBalance { get; set; }

        /// <summary>
        /// 累计收入金额。
        /// </summary>
        [Column("total_income", TypeName = "decimal(18,2)")]
        public decimal TotalIncome { get; set; }

        /// <summary>
        /// 累计支出金额。
        /// </summary>
        [Column("total_expense", TypeName = "decimal(18,2)")]
        public decimal TotalExpense { get; set; }

        /// <summary>
        /// 最近一次账户变更时间。
        /// </summary>
        [Column("last_changed_at")]
        public DateTime? LastChangedAt { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间。
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
