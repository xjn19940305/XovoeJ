using System.Threading.Tasks;
using XovoeJ.EventBus.Events;

namespace XovoeJ.EventBus.Abstractions
{
    /// <summary>
    /// 集成事件处理器接口
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    public interface IIntegrationEventHandler<in T> where T : IntegrationEvent
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="event">事件数据</param>
        Task Handle(T @event);
    }
}
