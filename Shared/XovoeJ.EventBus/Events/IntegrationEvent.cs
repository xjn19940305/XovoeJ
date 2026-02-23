namespace XovoeJ.EventBus.Events
{
    /// <summary>
    /// 集成事件基类
    /// </summary>
    public abstract class IntegrationEvent
    {
        /// <summary>
        /// 事件ID
        /// </summary>
        public string Id { get; } = Guid.CreateVersion7().ToString();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationDate { get; } = DateTime.UtcNow;

        /// <summary>
        /// 事件类型（自动设置为类名）
        /// </summary>
        public string EventType { get; } = typeof(IntegrationEvent).AssemblyQualifiedName ?? string.Empty;
    }
}
