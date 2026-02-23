namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 批量创建字典项请求DTO
    /// </summary>
    public class BatchCreateDictionaryItemRequestDto
    {
        /// <summary>
        /// 分组ID - 必填
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 字典项列表
        /// </summary>
        public List<BatchCreateDictionaryItemData> Items { get; set; } = new();
    }

    /// <summary>
    /// 批量创建字典项数据
    /// </summary>
    public class BatchCreateDictionaryItemData
    {
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
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
