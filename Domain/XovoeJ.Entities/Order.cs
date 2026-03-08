using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 订单
    /// </summary>
    [Table("orders")]
    public class Order
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 订单号
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("order_no")]
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 订单总金额
        /// </summary>
        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        [Column("discount_amount", TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [MaxLength(64)]
        [Column("user_coupon_id")]
        public string? UserCouponId { get; set; }

        [MaxLength(64)]
        [Column("coupon_template_id")]
        public string? CouponTemplateId { get; set; }

        [MaxLength(128)]
        [Column("coupon_name")]
        public string? CouponName { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        [Column("freight_amount", TypeName = "decimal(18,2)")]
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 实付金额
        /// </summary>
        [Column("pay_amount", TypeName = "decimal(18,2)")]
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 关联支付单 ID。
        /// </summary>
        [MaxLength(64)]
        [Column("payment_order_id")]
        public string? PaymentOrderId { get; set; }

        /// <summary>
        /// 关联支付单号。
        /// </summary>
        [MaxLength(64)]
        [Column("payment_order_no")]
        public string? PaymentOrderNo { get; set; }

        /// <summary>
        /// 实际支付方式快照，例如 wallet。
        /// </summary>
        [MaxLength(32)]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// 钱包支付金额快照。
        /// </summary>
        [Column("wallet_pay_amount", TypeName = "decimal(18,2)")]
        public decimal WalletPayAmount { get; set; }

        /// <summary>
        /// 订单消耗积分数量快照。
        /// </summary>
        [Column("points_used")]
        public int PointsUsed { get; set; }

        /// <summary>
        /// 积分抵扣金额快照。
        /// </summary>
        [Column("points_deduction_amount", TypeName = "decimal(18,2)")]
        public decimal PointsDeductionAmount { get; set; }

        /// <summary>
        /// 订单奖励积分数量快照。
        /// </summary>
        [Column("reward_points")]
        public int RewardPoints { get; set; }

        /// <summary>
        /// 已退款金额快照。
        /// </summary>
        [Column("refunded_amount", TypeName = "decimal(18,2)")]
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// 支付状态（0-未支付，1-已支付）
        /// </summary>
        [Column("pay_status")]
        public int PayStatus { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        [Column("pay_time")]
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 发货状态（0-未发货，1-已发货）
        /// </summary>
        [Column("ship_status")]
        public int ShipStatus { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        [Column("ship_time")]
        public DateTime? ShipTime { get; set; }

        [MaxLength(64)]
        [Column("shipping_company")]
        public string? ShippingCompany { get; set; }

        [MaxLength(64)]
        [Column("tracking_no")]
        public string? TrackingNo { get; set; }

        [MaxLength(500)]
        [Column("shipping_remark")]
        public string? ShippingRemark { get; set; }

        /// <summary>
        /// 收货时间
        /// </summary>
        [Column("receive_time")]
        public DateTime? ReceiveTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [Column("finish_time")]
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// 取消时间
        /// </summary>
        [Column("cancel_time")]
        public DateTime? CancelTime { get; set; }

        /// <summary>
        /// 是否已删除（软删除）
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [MaxLength(64)]
        [Column("consignee_name")]
        public string? ConsigneeName { get; set; }

        /// <summary>
        /// 收货人电话
        /// </summary>
        [MaxLength(32)]
        [Column("consignee_mobile")]
        public string? ConsigneeMobile { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        [MaxLength(512)]
        [Column("consignee_address")]
        public string? ConsigneeAddress { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(1000)]
        [Column("remark")]
        public string? Remark { get; set; }

        [Column("need_invoice")]
        public bool NeedInvoice { get; set; }

        [Column("invoice_type")]
        public int? InvoiceType { get; set; }

        [MaxLength(128)]
        [Column("invoice_title")]
        public string? InvoiceTitle { get; set; }

        [MaxLength(64)]
        [Column("invoice_tax_no")]
        public string? InvoiceTaxNo { get; set; }

        [MaxLength(128)]
        [Column("invoice_email")]
        public string? InvoiceEmail { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 订单项集合
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
