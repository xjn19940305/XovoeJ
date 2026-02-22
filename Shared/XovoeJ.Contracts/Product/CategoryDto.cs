namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 商品分类DTO
    /// </summary>
    public class CategoryDto
    {
        /// <summary>
        /// 分类ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 父分类ID
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 分类图片
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 子分类
        /// </summary>
        public List<CategoryDto> Children { get; set; } = new();
    }
}
