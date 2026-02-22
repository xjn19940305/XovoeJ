using XovoeJ.Enum;

namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 订单项DTO
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// 订单项ID
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
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 小计
        /// </summary>
        public decimal Subtotal { get; set; }
    }
}
