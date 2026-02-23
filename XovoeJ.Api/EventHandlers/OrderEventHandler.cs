using Microsoft.Extensions.Logging;
using XovoeJ.EventBus;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;

namespace XovoeJ.Api.EventHandlers
{
    /// <summary>
    /// 订单创建事件处理器
    /// </summary>
    public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderCreatedEvent @event)
        {
            _logger.LogInformation("处理订单创建事件: OrderId={OrderId}, OrderNo={OrderNo}, Amount={Amount}",
                @event.OrderId, @event.OrderNo, @event.TotalAmount);

            // 这里处理订单创建后的业务逻辑：
            // 1. 发送通知（短信/邮件）
            // 2. 更新库存
            // 3. 记录日志
            // 4. 调用第三方服务
            // ...

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 订单支付完成事件处理器
    /// </summary>
    public class OrderPaidEventHandler : IIntegrationEventHandler<OrderPaidEvent>
    {
        private readonly ILogger<OrderPaidEventHandler> _logger;

        public OrderPaidEventHandler(ILogger<OrderPaidEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderPaidEvent @event)
        {
            _logger.LogInformation("处理订单支付完成事件: OrderId={OrderId}, Amount={Amount}, Method={Method}",
                @event.OrderId, @event.PaidAmount, @event.PaymentMethod);

            // 这里处理订单支付完成后的业务逻辑：
            // 1. 发送支付成功通知
            // 2. 触发发货流程
            // 3. 更新会员积分
            // ...

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 订单取消事件处理器
    /// </summary>
    public class OrderCancelledEventHandler : IIntegrationEventHandler<OrderCancelledEvent>
    {
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(OrderCancelledEvent @event)
        {
            _logger.LogInformation("处理订单取消事件: OrderId={OrderId}, Reason={Reason}",
                @event.OrderId, @event.Reason);

            // 这里处理订单取消后的业务逻辑：
            // 1. 退款处理
            // 2. 恢复库存
            // 3. 发送取消通知
            // ...

            return Task.CompletedTask;
        }
    }
}
