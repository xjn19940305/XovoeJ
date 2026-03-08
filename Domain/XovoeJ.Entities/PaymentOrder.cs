using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 订单支付单。
    /// </summary>
    [Table("payment_orders")]
    public class PaymentOrder
    {
        /// <summary>
        /// 主键 ID。
        /// </summary>
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 支付单号。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("payment_order_no")]
        public string PaymentOrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 关联订单 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 关联订单号。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("order_no")]
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户 ID。
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 支付方式，例如 wallet。
        /// </summary>
        [MaxLength(32)]
        [Column("payment_method")]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// 应付金额。
        /// </summary>
        [Column("payable_amount", TypeName = "decimal(18,2)")]
        public decimal PayableAmount { get; set; }

        /// <summary>
        /// 实付金额。
        /// </summary>
        [Column("paid_amount", TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// 已退款金额。
        /// </summary>
        [Column("refunded_amount", TypeName = "decimal(18,2)")]
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// 支付单状态。
        /// </summary>
        [Column("status")]
        public PaymentOrderStatus Status { get; set; } = PaymentOrderStatus.Pending;

        /// <summary>
        /// 过期时间。
        /// </summary>
        [Column("expire_at")]
        public DateTime? ExpireAt { get; set; }

        /// <summary>
        /// 支付成功时间。
        /// </summary>
        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// 关闭时间。
        /// </summary>
        [Column("closed_at")]
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// 最近失败原因。
        /// </summary>
        [MaxLength(500)]
        [Column("failure_reason")]
        public string? FailureReason { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间。
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
