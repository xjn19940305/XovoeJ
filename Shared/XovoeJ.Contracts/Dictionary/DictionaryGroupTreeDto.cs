using XovoeJ.Enum;

namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 字典分组树节点DTO
    /// </summary>
    public class DictionaryGroupTreeDto
    {
        /// <summary>
        /// 分组ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 分组编码
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 字典类型
        /// </summary>
        public DictionaryType Type { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 子分组
        /// </summary>
        public List<DictionaryGroupTreeDto> Children { get; set; } = new();

        /// <summary>
        /// 字典项
        /// </summary>
        public List<DictionaryItemDto> Items { get; set; } = new();
    }
}
