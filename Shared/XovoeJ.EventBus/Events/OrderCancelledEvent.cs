namespace XovoeJ.EventBus.Events
{
    /// <summary>
    /// 订单取消事件
    /// </summary>
    public class OrderCancelledEvent : IntegrationEvent
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
        /// 取消原因
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 取消时间
        /// </summary>
        public DateTime CancelledTime { get; set; } = DateTime.UtcNow;
    }
}
