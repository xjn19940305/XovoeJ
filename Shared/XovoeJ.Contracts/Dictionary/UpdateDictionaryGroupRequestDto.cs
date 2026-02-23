namespace XovoeJ.Contracts.Dictionary
{
    /// <summary>
    /// 更新字典分组请求DTO
    /// </summary>
    public class UpdateDictionaryGroupRequestDto
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool? IsEnabled { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
