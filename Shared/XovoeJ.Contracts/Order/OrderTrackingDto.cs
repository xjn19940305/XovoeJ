namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 订单物流信息
    /// </summary>
    public class OrderTrackingDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 物流公司
        /// </summary>
        public string? ShippingCompany { get; set; }

        /// <summary>
        /// 运单号
        /// </summary>
        public string? TrackingNo { get; set; }

        /// <summary>
        /// 发货备注
        /// </summary>
        public string? ShippingRemark { get; set; }

        /// <summary>
        /// 发货时间
        /// </summary>
        public DateTime? ShipTime { get; set; }

        /// <summary>
        /// 当前物流状态
        /// </summary>
        public string TrackingStatus { get; set; } = string.Empty;
    }
}
