namespace XovoeJ.EventBus.Events
{
    /// <summary>
    /// 事件订阅信息
    /// </summary>
    public class EventSubscriptionInfo
    {
        /// <summary>
        /// 事件类型名称
        /// </summary>
        public string EventTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 处理器类型名称
        /// </summary>
        public string HandlerTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 是否动态订阅
        /// </summary>
        public bool IsDynamic { get; set; }

        public override string ToString()
        {
            return IsDynamic
                ? $"Dynamic: {EventTypeName} -> {HandlerTypeName}"
                : $"{EventTypeName} -> {HandlerTypeName}";
        }
    }
}
