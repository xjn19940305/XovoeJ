using System.ComponentModel.DataAnnotations;

namespace XovoeJ.Contracts.Mall
{
    public class CheckoutPreviewRequestDto
    {
        public List<string>? CartItemIds { get; set; }

        public List<CheckoutDirectItemDto>? DirectItems { get; set; }

        public string? AddressId { get; set; }

        public string? CouponId { get; set; }

        public string? UserCouponId { get; set; }
    }

    public class CheckoutDirectItemDto
    {
        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public string SkuId { get; set; } = string.Empty;

        [Range(1, 9999)]
        public int Quantity { get; set; } = 1;
    }

    public class CheckoutPreviewResponseDto
    {
        public List<CheckoutPreviewItemDto> Items { get; set; } = new();

        public CheckoutAddressDto? Address { get; set; }

        public CheckoutCouponDto? Coupon { get; set; }

        public decimal ProductAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FreightAmount { get; set; }

        public decimal PayAmount { get; set; }

        public bool CanSubmit { get; set; }

        public string? UnavailableReason { get; set; }
    }

    public class CheckoutPreviewItemDto
    {
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string? ProductImage { get; set; }

        public string SkuId { get; set; } = string.Empty;

        public string? SkuCode { get; set; }

        public Dictionary<string, string>? SkuSpecs { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int Stock { get; set; }

        public decimal Subtotal { get; set; }

        public bool IsValid { get; set; }
    }

    public class CheckoutAddressDto
    {
        public string Id { get; set; } = string.Empty;

        public string ConsigneeName { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;

        public string? Label { get; set; }

        public bool IsDefault { get; set; }
    }

    public class CheckoutCouponDto
    {
        public string Id { get; set; } = string.Empty;

        public string UserCouponId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int CouponType { get; set; }

        public int DiscountType { get; set; }

        public decimal DiscountValue { get; set; }

        public decimal MinOrderAmount { get; set; }

        public decimal AppliedDiscountAmount { get; set; }
    }
}
