using XovoeJ.Abstractions.Services;
using XovoeJ.Api.EventHandlers;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;

namespace XovoeJ.Api
{
    /// <summary>
    /// 事件总线配置扩展
    /// </summary>
    public static class EventBusConfigurationExtensions
    {
        /// <summary>
        /// 配置事件总线订阅
        /// </summary>
        /// <param name="app">Web应用</param>
        public static void ConfigureEventBus(this WebApplication app)
        {
            var eventBus = app.Services.GetRequiredService<IEventBus>();

            // 订阅订单相关事件
            eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
            eventBus.Subscribe<OrderPaidEvent, OrderPaidEventHandler>();
            eventBus.Subscribe<OrderCancelledEvent, OrderCancelledEventHandler>();

            // 订阅用户相关事件
            // eventBus.Subscribe<UserRegisteredEvent, UserRegisteredEventHandler>();

            // 订阅商品相关事件
            // eventBus.Subscribe<ProductCreatedEvent, ProductCreatedEventHandler>();
            // eventBus.Subscribe<ProductUpdatedEvent, ProductUpdatedEventHandler>();

            // 可以继续添加更多订阅...
        }
    }
}
