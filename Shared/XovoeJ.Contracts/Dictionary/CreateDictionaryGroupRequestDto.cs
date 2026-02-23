using XovoeJ.Enum;

namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 创建字典分组请求DTO
    /// </summary>
    public class CreateDictionaryGroupRequestDto
    {
        /// <summary>
        /// 分组编码 - 必填，唯一
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称 - 必填
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 字典类型
        /// </summary>
        public DictionaryType Type { get; set; } = DictionaryType.System;

        /// <summary>
        /// 父分组ID
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

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
