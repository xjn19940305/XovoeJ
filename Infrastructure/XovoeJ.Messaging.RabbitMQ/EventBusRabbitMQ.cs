using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using XovoeJ.EventBus;
using XovoeJ.EventBus.Abstractions;
using XovoeJ.EventBus.Events;
using IModel = RabbitMQ.Client.IModel;

namespace XovoeJ.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 事件总线实现
    /// </summary>
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly EventBusRabbitMQOptions _options;
        private IModel? _consumerChannel;
        private string? _consumerTag;
        private bool _disposed;

        public EventBusRabbitMQ(
            IRabbitMQPersistentConnection persistentConnection,
            ILogger<EventBusRabbitMQ> logger,
            IServiceProvider serviceProvider,
            IEventBusSubscriptionsManager subsManager,
            IOptions<EventBusRabbitMQOptions> options)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _subsManager = subsManager ?? throw new ArgumentNullException(nameof(subsManager));
            _options = options.Value;
            _consumerChannel = CreateConsumerChannel();

            if (_subsManager is EventBusSubscriptionsManager manager)
            {
                manager.OnEventRemoved += SubsManager_OnEventRemoved;
            }
        }

        private void SubsManager_OnEventRemoved(object? sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                return;
            }

            using var channel = _persistentConnection.CreateModel();
            channel.QueueUnbind(queue: _options.SubscriptionClientName,
                exchange: _options.BrokerName,
                routingKey: eventName);

            if (_subsManager.IsEmpty)
            {
                _consumerChannel?.Close();
            }
        }

        public Task PublishAsync<T>(T @event) where T : IntegrationEvent
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_options.RetryCount, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var eventName = _subsManager.GetEventKey<T>();

            return Task.Run(() =>
            {
                policy.Execute(() =>
                {
                    var channel = _persistentConnection.CreateModel();

                    channel.ExchangeDeclare(exchange: _options.BrokerName,
                        type: "direct",
                        durable: _options.Durable,
                        autoDelete: _options.AutoDelete);

                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    channel.BasicPublish(
                        exchange: _options.BrokerName,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);
                });
            });
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            DoInternalSubscription(eventName);

            _subsManager.AddSubscription<T, TH>();

            StartBasicConsume();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);
            if (!containsKey)
            {
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }

                using var channel = _persistentConnection.CreateModel();
                channel.QueueBind(queue: _options.SubscriptionClientName,
                    exchange: _options.BrokerName,
                    routingKey: eventName);
            }
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        private void StartBasicConsume()
        {
            _logger.LogDebug("Starting RabbitMQ basic consume");

            if (_consumerChannel == null)
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
                return;
            }

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
                consumer.Received += Consumer_Received;

                _consumerTag = _consumerChannel.BasicConsume(
                    queue: _options.SubscriptionClientName,
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async Task Consumer_Received(object? sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);
            }

            // Even on exception, we ack the message to remove it from the queue
            _consumerChannel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using var scope = _serviceProvider.CreateScope();

                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = scope.ServiceProvider.GetService(subscription);
                    if (handler == null) continue;

                    if (handler is IDynamicIntegrationEventHandler dynamicHandler)
                    {
                        await dynamicHandler.HandleDynamic(eventName, message);
                    }
                    else
                    {
                        var eventType = _subsManager.GetEventTypes()
                            .FirstOrDefault(t => t.Name == eventName);

                        if (eventType != null)
                        {
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            if (integrationEvent != null)
                            {
                                var handleMethod = subscription.GetMethod("Handle");
                                if (handleMethod != null)
                                {
                                    var task = (Task?)handleMethod.Invoke(handler, new[] { integrationEvent });
                                    if (task != null)
                                    {
                                        await task;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _options.BrokerName,
                type: "direct",
                durable: _options.Durable,
                autoDelete: _options.AutoDelete);

            channel.QueueDeclare(queue: _options.SubscriptionClientName,
                durable: _options.Durable,
                exclusive: false,
                autoDelete: _options.AutoDelete,
                arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning("RabbitMQ callback exception: {Exception}", ea.Exception.Message);
                _consumerChannel?.Dispose();
                _consumerChannel = CreateConsumerChannel();
                StartBasicConsume();
            };

            return channel;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            if (_subsManager is EventBusSubscriptionsManager manager)
            {
                manager.OnEventRemoved -= SubsManager_OnEventRemoved;
            }

            _consumerChannel?.Dispose();
            _subsManager.Clear();
        }
    }
}
