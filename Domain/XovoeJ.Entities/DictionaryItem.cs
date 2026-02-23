using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 字典项 - 具体的配置项
    /// </summary>
    [Table("dictionary_items")]
    public class DictionaryItem
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 分组ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("group_id")]
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 配置键/编码 - 唯一标识，如 "site_title", "alipay_enabled" 等
        /// </summary>
        [Required]
        [MaxLength(128)]
        [Column("key")]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称/标签
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 配置值 - 根据DataType存储不同类型的值
        /// </summary>
        [MaxLength(4000)]
        [Column("value")]
        public string? Value { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [Column("data_type")]
        public DictionaryDataType DataType { get; set; } = DictionaryDataType.String;

        /// <summary>
        /// 默认值
        /// </summary>
        [MaxLength(4000)]
        [Column("default_value")]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 占位提示
        /// </summary>
        [MaxLength(256)]
        [Column("placeholder")]
        public string? Placeholder { get; set; }

        /// <summary>
        /// 配置项说明
        /// </summary>
        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 可选值 - JSON格式，用于下拉选择，如 ["enabled","disabled"] 或 [{"v":"1","t":"开启"},{"v":"0","t":"关闭"}]
        /// </summary>
        [Column("options", TypeName = "jsonb")]
        public string? Options { get; set; }

        /// <summary>
        /// 校验规则 - JSON格式，如 {"required":true,"pattern":"^[a-z]+$","min":1,"max":100}
        /// </summary>
        [Column("validation", TypeName = "jsonb")]
        public string? Validation { get; set; }

        /// <summary>
        /// 是否系统内置 - 内置项不可删除
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        [Column("is_required")]
        public bool IsRequired { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 排序 - 数字越小越靠前
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        [Column("remark")]
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 所属分组导航属性
        /// </summary>
        [ForeignKey(nameof(GroupId))]
        public DictionaryGroup? Group { get; set; }
    }
}
