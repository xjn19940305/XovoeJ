using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using XovoeJ.EventBus;
using XovoeJ.EventBus.Abstractions;

namespace XovoeJ.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 事件总线服务集合扩展
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加 RabbitMQ 事件总线
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="subscriptionClientName">订阅客户端名称</param>
        public static IServiceCollection AddRabbitMQEventBus(
            this IServiceCollection services,
            IConfiguration configuration,
            string? subscriptionClientName = null)
        {
            // 配置选项
            services.Configure<EventBusRabbitMQOptions>(options =>
            {
                configuration.GetSection("EventBusRabbitMQ").Bind(options);
            });

            return services.AddRabbitMQEventBus(subscriptionClientName);
        }

        /// <summary>
        /// 添加 RabbitMQ 事件总线（使用自定义配置）
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="setupAction">配置操作</param>
        /// <param name="subscriptionClientName">订阅客户端名称</param>
        public static IServiceCollection AddRabbitMQEventBus(
            this IServiceCollection services,
            Action<EventBusRabbitMQOptions> setupAction,
            string? subscriptionClientName = null)
        {
            // 配置选项
            services.Configure(setupAction);

            return services.AddRabbitMQEventBus(subscriptionClientName);
        }

        private static IServiceCollection AddRabbitMQEventBus(
            this IServiceCollection services,
            string? subscriptionClientName)
        {
            // 添加持久连接
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var options = sp.GetRequiredService<IOptions<EventBusRabbitMQOptions>>().Value;

                if (!string.IsNullOrEmpty(subscriptionClientName))
                {
                    options.SubscriptionClientName = subscriptionClientName;
                }

                var factory = new ConnectionFactory
                {
                    HostName = options.HostName,
                    Port = options.Port,
                    UserName = options.UserName,
                    Password = options.Password,
                    VirtualHost = options.VirtualHost,
                    DispatchConsumersAsync = true
                };

                return new DefaultRabbitMQPersistentConnection(factory, logger,
                    Microsoft.Extensions.Options.Options.Create(options));
            });

            // 添加订阅管理器
            services.AddSingleton<IEventBusSubscriptionsManager, EventBusSubscriptionsManager>();

            // 添加事件总线
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();

            return services;
        }
    }
}
