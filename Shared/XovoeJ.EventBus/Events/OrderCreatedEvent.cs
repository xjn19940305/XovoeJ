namespace XovoeJ.EventBus.Events
{
    /// <summary>
    /// 订单创建事件
    /// </summary>
    public class OrderCreatedEvent : IntegrationEvent
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 订单号
        /// </summary>
        public string OrderNo { get; set; } = string.Empty;

        /// <summary>
        /// 订单金额
        /// </summary>
        public decimal TotalAmount { get; set; }
    }
}
