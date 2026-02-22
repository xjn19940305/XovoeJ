namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 商品列表查询参数
    /// </summary>
    public class ProductListQueryDto
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 分类ID
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 是否只看热门
        /// </summary>
        public bool? IsHot { get; set; }

        /// <summary>
        /// 是否只看新品
        /// </summary>
        public bool? IsNew { get; set; }

        /// <summary>
        /// 是否只看推荐
        /// </summary>
        public bool? IsRecommend { get; set; }

        /// <summary>
        /// 价格排序（asc-升序, desc-降序）
        /// </summary>
        public string? PriceSort { get; set; }

        /// <summary>
        /// 销量排序（asc-升序, desc-降序）
        /// </summary>
        public string? SalesSort { get; set; }
    }

    /// <summary>
    /// 商品列表响应DTO
    /// </summary>
    public class ProductListResponseDto
    {
        /// <summary>
        /// 商品列表
        /// </summary>
        public List<ProductDto> Items { get; set; } = new();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }
}
