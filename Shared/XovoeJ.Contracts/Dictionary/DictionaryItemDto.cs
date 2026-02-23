using XovoeJ.Enum;

namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 字典项DTO
    /// </summary>
    public class DictionaryItemDto
    {
        /// <summary>
        /// 字典项ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 分组ID
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 分组编码
        /// </summary>
        public string? GroupCode { get; set; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// 配置键
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 配置值
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DictionaryDataType DataType { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 占位提示
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// 配置项说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 可选值
        /// </summary>
        public string? Options { get; set; }

        /// <summary>
        /// 校验规则
        /// </summary>
        public string? Validation { get; set; }

        /// <summary>
        /// 是否系统内置
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
