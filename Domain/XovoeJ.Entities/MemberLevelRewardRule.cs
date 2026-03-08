using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 会员升级奖励规则。
    /// </summary>
    [Table("member_level_reward_rules")]
    public class MemberLevelRewardRule
    {
        /// <summary>
        /// 规则主键。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 会员等级编码，例如 silver、gold。
        /// </summary>
        [Required]
        [MaxLength(32)]
        [Column("level_code")]
        public string LevelCode { get; set; } = string.Empty;

        /// <summary>
        /// 会员等级显示名称。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("level_name")]
        public string LevelName { get; set; } = string.Empty;

        /// <summary>
        /// 达到该等级时要发放的优惠券模板 ID 集合，JSON 序列化存储。
        /// </summary>
        [Required]
        [Column("coupon_template_ids_json")]
        public string CouponTemplateIdsJson { get; set; } = "[]";

        /// <summary>
        /// 规则状态，1 表示启用。
        /// </summary>
        [Column("status")]
        public int Status { get; set; } = 1;

        /// <summary>
        /// 排序值，越小越靠前。
        /// </summary>
        [Column("sort")]
        public int Sort { get; set; } = 100;

        /// <summary>
        /// 规则说明。
        /// </summary>
        [MaxLength(256)]
        [Column("description")]
        public string? Description { get; set; }

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
