using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 轮播图实体。
    /// </summary>
    [Table("banners")]
    public class Banner
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 轮播图标题。
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 轮播图副标题。
        /// </summary>
        [MaxLength(256)]
        [Column("subtitle")]
        public string? Subtitle { get; set; }

        /// <summary>
        /// 轮播图图片地址。
        /// </summary>
        [Required]
        [MaxLength(512)]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// 点击跳转链接。
        /// </summary>
        [MaxLength(512)]
        [Column("link_url")]
        public string? LinkUrl { get; set; }

        /// <summary>
        /// 排序值，越小越靠前。
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用展示。
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 生效开始时间。
        /// </summary>
        [Column("start_time")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 生效结束时间。
        /// </summary>
        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 备注信息。
        /// </summary>
        [MaxLength(500)]
        [Column("remark")]
        public string? Remark { get; set; }

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
