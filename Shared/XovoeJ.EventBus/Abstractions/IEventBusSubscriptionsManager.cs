using System;
using System.Collections.Generic;
using XovoeJ.EventBus.Events;

namespace XovoeJ.EventBus.Abstractions
{
    /// <summary>
    /// 事件总线订阅管理器接口
    /// </summary>
    public interface IEventBusSubscriptionsManager
    {
        /// <summary>
        /// 是否有空订阅
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 获取所有事件类型
        /// </summary>
        IEnumerable<Type> GetEventTypes();

        /// <summary>
        /// 添加订阅
        /// </summary>
        void AddSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// 添加动态订阅
        /// </summary>
        void AddDynamicSubscription<TH>(string eventName) where TH : IDynamicIntegrationEventHandler;

        /// <summary>
        /// 移除订阅
        /// </summary>
        void RemoveSubscription<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;

        /// <summary>
        /// 是否有订阅
        /// </summary>
        bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent;

        /// <summary>
        /// 是否有订阅
        /// </summary>
        bool HasSubscriptionsForEvent(string eventName);

        /// <summary>
        /// 获取事件处理器
        /// </summary>
        IEnumerable<Type> GetHandlersForEvent<T>() where T : IntegrationEvent;

        /// <summary>
        /// 获取事件处理器
        /// </summary>
        IEnumerable<Type> GetHandlersForEvent(string eventName);

        /// <summary>
        /// 获取事件名称
        /// </summary>
        string GetEventKey<T>();

        /// <summary>
        /// 清除所有订阅
        /// </summary>
        void Clear();
    }
}
