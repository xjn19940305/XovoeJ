namespace XovoeJ.Contracts.Cart
{
    /// <summary>
    /// 购物车项DTO
    /// </summary>
    public class CartItemDto
    {
        /// <summary>
        /// 购物车项ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 商品图片
        /// </summary>
        public string? ProductImage { get; set; }

        /// <summary>
        /// SKU ID
        /// </summary>
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// SKU规格
        /// </summary>
        public Dictionary<string, string>? SkuSpecs { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 库存
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// 小计
        /// </summary>
        public decimal Subtotal => Price * Quantity;

        /// <summary>
        /// 是否有效（库存是否充足）
        /// </summary>
        public bool IsValid => Stock >= Quantity;
    }
}
