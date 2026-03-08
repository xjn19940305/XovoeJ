using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 提交订单请求DTO
    /// </summary>
    public class SubmitOrderRequestDto
    {
        /// <summary>
        /// 购物车项ID列表（为空则从购物车下单）
        /// </summary>
        public List<string>? CartItemIds { get; set; }

        /// <summary>
        /// 直接购买的商品项
        /// </summary>
        public List<DirectOrderItemDto>? DirectItems { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [Required(ErrorMessage = "收货人姓名不能为空")]
        [MaxLength(64)]
        public string ConsigneeName { get; set; } = string.Empty;

        /// <summary>
        /// 收货人电话
        /// </summary>
        [Required(ErrorMessage = "收货人电话不能为空")]
        [MaxLength(32)]
        public string ConsigneeMobile { get; set; } = string.Empty;

        /// <summary>
        /// 收货地址
        /// </summary>
        [Required(ErrorMessage = "收货地址不能为空")]
        [MaxLength(512)]
        public string ConsigneeAddress { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(1000)]
        public string? Remark { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>
        public string? CouponId { get; set; }

        public bool NeedInvoice { get; set; }

        public int? InvoiceType { get; set; }

        [MaxLength(128)]
        public string? InvoiceTitle { get; set; }

        [MaxLength(64)]
        public string? InvoiceTaxNo { get; set; }

        [MaxLength(128)]
        public string? InvoiceEmail { get; set; }
    }

    /// <summary>
    /// 直接购买商品项
    /// </summary>
    public class DirectOrderItemDto
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// SKU ID
        /// </summary>
        [Required]
        public string SkuId { get; set; } = string.Empty;

        /// <summary>
        /// 数量
        /// </summary>
        [Required]
        [Range(1, 9999)]
        public int Quantity { get; set; } = 1;
    }
}
