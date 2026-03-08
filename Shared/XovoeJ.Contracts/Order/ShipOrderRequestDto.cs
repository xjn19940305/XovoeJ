namespace XovoeJ.Contracts.Order
{
    /// <summary>
    /// 管理端发货请求
    /// </summary>
    public class ShipOrderRequestDto
    {
        /// <summary>
        /// 物流公司
        /// </summary>
        public string ShippingCompany { get; set; } = string.Empty;

        /// <summary>
        /// 运单号
        /// </summary>
        public string TrackingNo { get; set; } = string.Empty;

        /// <summary>
        /// 发货备注
        /// </summary>
        public string? ShippingRemark { get; set; }
    }
}
