using System.Threading.Tasks;

namespace XovoeJ.EventBus.Abstractions
{
    /// <summary>
    /// 动态集成事件处理器接口（用于处理JSON格式的事件）
    /// </summary>
    public interface IDynamicIntegrationEventHandler
    {
        /// <summary>
        /// 处理动态事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="message">消息内容（JSON字符串）</param>
        Task HandleDynamic(string eventName, string message);
    }
}
