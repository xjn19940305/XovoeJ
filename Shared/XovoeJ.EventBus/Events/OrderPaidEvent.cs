namespace XovoeJ.EventBus.Events
{
    /// <summary>
    /// 订单支付完成事件
    /// </summary>
    public class OrderPaidEvent : IntegrationEvent
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
        /// 支付金额
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PaymentTime { get; set; }
    }
}
