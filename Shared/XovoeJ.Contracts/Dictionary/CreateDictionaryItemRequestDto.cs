using XovoeJ.Enum;

namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 创建字典项请求DTO
    /// </summary>
    public class CreateDictionaryItemRequestDto
    {
        /// <summary>
        /// 分组ID - 必填
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 配置键 - 必填，唯一
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称 - 必填
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 配置值
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public DictionaryDataType DataType { get; set; } = DictionaryDataType.String;

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
        /// 可选值 - JSON数组
        /// </summary>
        public string? Options { get; set; }

        /// <summary>
        /// 校验规则 - JSON对象
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
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
