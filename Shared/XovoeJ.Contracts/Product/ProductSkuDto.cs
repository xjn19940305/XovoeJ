namespace XovoeJ.Contracts.Product
{
    /// <summary>
    /// 商品SKU DTO
    /// </summary>
    public class ProductSkuDto
    {
        /// <summary>
        /// SKU ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// SKU编码
        /// </summary>
        public string SkuCode { get; set; } = string.Empty;

        /// <summary>
        /// 规格属性
        /// </summary>
        public Dictionary<string, string>? Specs { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        public decimal? OriginalPrice { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 销量
        /// </summary>
        public int SalesCount { get; set; }

        /// <summary>
        /// SKU图片
        /// </summary>
        public string? Image { get; set; }
    }
}
