using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Cart
{
    /// <summary>
    /// 添加购物车请求DTO
    /// </summary>
    public class AddToCartRequestDto
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        [Required(ErrorMessage = "商品ID不能为空")]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// SKU ID
        /// </summary>
        [Required(ErrorMessage = "SKU ID不能为空")]
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        [Required(ErrorMessage = "数量不能为空")]
        [Range(1, 9999, ErrorMessage = "数量必须在1-9999之间")]
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 更新购物车数量请求DTO
    /// </summary>
    public class UpdateCartQuantityRequestDto
    {
        /// <summary>
        /// 数量
        /// </summary>
        [Required(ErrorMessage = "数量不能为空")]
        [Range(1, 9999, ErrorMessage = "数量必须在1-9999之间")]
        public int Quantity { get; set; } = 1;
    }
}
