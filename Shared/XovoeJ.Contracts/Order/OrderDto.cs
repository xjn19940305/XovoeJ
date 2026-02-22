using XovoeJ.Enum;

namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 订单DTO
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 优惠金额
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightAmount { get; set; }

        /// <summary>
        /// 实付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 订单状态描述
        /// </summary>
        public string? StatusDescription => Status.ToString();

        /// <summary>
        /// 支付状态
        /// </summary>
        public int PayStatus { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PayTime { get; set; }

        /// <summary>
        /// 发货状态
        /// </summary>
        public int ShipStatus { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShipTime { get; set; }

        /// <summary>
        /// 收货时间
        /// </summary>
        public DateTime? ReceiveTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string? ConsigneeName { get; set; }

        /// <summary>
        /// 收货人电话
        /// </summary>
        public string? ConsigneeMobile { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string? ConsigneeAddress { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 订单项列表
        /// </summary>
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }
}
