using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;

namespace XovoeJ.EventBus
{
    /// <summary>
    /// 事件总线订阅管理器实现
    /// </summary>
    public class EventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly Dictionary<string, Type> _eventTypesMap;

        public event EventHandler<string>? OnEventRemoved;

        public EventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _eventTypesMap = new Dictionary<string, Type>();
        }

        public bool IsEmpty => _handlers.Count == 0;

        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            AddSubscription(typeof(TH), eventName);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
                _eventTypesMap[eventName] = typeof(T);
            }
        }

        public void AddDynamicSubscription<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            AddSubscription(typeof(TH), eventName, isDynamic: true);
        }

        private void AddSubscription(Type handlerType, string eventName, bool isDynamic = false)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers[eventName] = new List<Type>();
            }

            if (_handlers[eventName].Any(s => s == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'");
            }

            _handlers[eventName].Add(handlerType);
        }

        public void RemoveSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveSubscription(eventName, handlerToRemove);
        }

        private Type? FindSubscriptionToRemove<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private Type? DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s == handlerType);
        }

        private void DoRemoveSubscription(string eventName, Type? handlerToRemove)
        {
            if (handlerToRemove != null)
            {
                _handlers[eventName].Remove(handlerToRemove);

                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }

                    OnEventRemoved?.Invoke(this, eventName);
                }
            }
        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName)
        {
            return _handlers.ContainsKey(eventName);
        }

        public IEnumerable<Type> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var eventName = GetEventKey<T>();
            return GetHandlersForEvent(eventName);
        }

        public IEnumerable<Type> GetHandlersForEvent(string eventName)
        {
            return _handlers.GetValueOrDefault(eventName, new List<Type>());
        }

        public IEnumerable<Type> GetEventTypes() => _eventTypes;

        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }

        public void Clear()
        {
            _handlers.Clear();
            _eventTypes.Clear();
            _eventTypesMap.Clear();
        }
    }
}
