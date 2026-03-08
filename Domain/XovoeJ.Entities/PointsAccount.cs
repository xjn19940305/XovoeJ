using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 用户积分账户快照。
    /// </summary>
    [Table("points_accounts")]
    public class PointsAccount
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
        /// 当前可用积分。
        /// </summary>
        [Column("available_points")]
        public int AvailablePoints { get; set; }

        /// <summary>
        /// 当前冻结积分。
        /// </summary>
        [Column("frozen_points")]
        public int FrozenPoints { get; set; }

        /// <summary>
        /// 累计获得积分。
        /// </summary>
        [Column("total_earned_points")]
        public int TotalEarnedPoints { get; set; }

        /// <summary>
        /// 累计消耗积分。
        /// </summary>
        [Column("total_spent_points")]
        public int TotalSpentPoints { get; set; }

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
