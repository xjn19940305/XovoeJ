namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 商品DTO
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 分类ID
        /// </summary>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 副标题
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string? MainImage { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> Images { get; set; } = new();

        /// <summary>
        /// 详情内容
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        public int SalesCount { get; set; }

        /// <summary>
        /// 浏览量
        /// </summary>
        public int BrowseCount { get; set; }

        /// <summary>
        /// 是否上架
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否热门
        /// </summary>
        public bool IsHot { get; set; }

        /// <summary>
        /// 是否新品
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// 是否推荐
        /// </summary>
        public bool IsRecommend { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// SKU列表
        /// </summary>
        public List<ProductSkuDto> Skus { get; set; } = new();

        /// <summary>
        /// 最低价格
        /// </summary>
        public decimal? MinPrice => Skus.Any() ? Skus.Min(s => s.Price) : null;

        /// <summary>
        /// 最高价格
        /// </summary>
        public decimal? MaxPrice => Skus.Any() ? Skus.Max(s => s.Price) : null;

        /// <summary>
        /// 总库存
        /// </summary>
        public int TotalStock => Skus.Sum(s => s.Stock);
    }
}
