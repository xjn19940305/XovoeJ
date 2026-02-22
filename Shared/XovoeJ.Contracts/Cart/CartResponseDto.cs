namespace XovoeJ.Contracts.Cart
{
    /// <summary>
    /// 购物车响应DTO
    /// </summary>
    public class CartResponseDto
    {
        /// <summary>
        /// 购物车项列表
        /// </summary>
        public List<CartItemDto> Items { get; set; } = new();

        /// <summary>
        /// 商品总数量
        /// </summary>
        public int TotalCount => Items.Sum(i => i.Quantity);

        /// <summary>
        /// 商品总金额
        /// </summary>
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);

        /// <summary>
        /// 无效商品数量
        /// </summary>
        public int InvalidCount => Items.Count(i => !i.IsValid);
    }
}
