using XovoeJ.Enum;

namespace XovoeJ.Contracts.Order
{
    public class OrderDto
    {
        public string Id { get; set; } = string.Empty;

        public string OrderNo { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public string? UserCouponId { get; set; }

        public string? CouponTemplateId { get; set; }

        public string? CouponName { get; set; }

        public decimal FreightAmount { get; set; }

        public decimal PayAmount { get; set; }

        public string? PaymentOrderId { get; set; }

        public string? PaymentOrderNo { get; set; }

        public string? PaymentMethod { get; set; }

        public decimal WalletPayAmount { get; set; }

        public int PointsUsed { get; set; }

        public decimal PointsDeductionAmount { get; set; }

        public int RewardPoints { get; set; }

        public decimal RefundedAmount { get; set; }

        public OrderStatus Status { get; set; }

        public string? StatusDescription => Status.ToString();

        public int PayStatus { get; set; }

        public DateTime? PayTime { get; set; }

        public int ShipStatus { get; set; }

        public DateTime? ShipTime { get; set; }

        public string? ShippingCompany { get; set; }

        public string? TrackingNo { get; set; }

        public string? ShippingRemark { get; set; }

        public DateTime? ReceiveTime { get; set; }

        public DateTime? FinishTime { get; set; }

        public string? ConsigneeName { get; set; }

        public string? ConsigneeMobile { get; set; }

        public string? ConsigneeAddress { get; set; }

        public string? Remark { get; set; }

        public bool NeedInvoice { get; set; }

        public int? InvoiceType { get; set; }

        public string? InvoiceTitle { get; set; }

        public string? InvoiceTaxNo { get; set; }

        public string? InvoiceEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
