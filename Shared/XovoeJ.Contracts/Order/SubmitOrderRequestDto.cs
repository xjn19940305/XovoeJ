using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Order
{
    public class SubmitOrderRequestDto
    {
        public List<string>? CartItemIds { get; set; }

        public List<DirectOrderItemDto>? DirectItems { get; set; }

        [Required(ErrorMessage = "收货人姓名不能为空")]
        [MaxLength(64)]
        public string ConsigneeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "收货人电话不能为空")]
        [MaxLength(32)]
        public string ConsigneeMobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "收货地址不能为空")]
        [MaxLength(512)]
        public string ConsigneeAddress { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Remark { get; set; }

        public string? CouponId { get; set; }

        public string? UserCouponId { get; set; }

        public bool NeedInvoice { get; set; }

        public int? InvoiceType { get; set; }

        [MaxLength(128)]
        public string? InvoiceTitle { get; set; }

        [MaxLength(64)]
        public string? InvoiceTaxNo { get; set; }

        [MaxLength(128)]
        public string? InvoiceEmail { get; set; }
    }

    public class DirectOrderItemDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string SkuId { get; set; } = string.Empty;

        [Required]
        [Range(1, 9999)]
        public int Quantity { get; set; } = 1;
    }
}
