using XovoeJ.Enum;

namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 更新字典项请求DTO
    /// </summary>
    public class UpdateDictionaryItemRequestDto
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 配置值
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// 占位提示
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// 配置项说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
