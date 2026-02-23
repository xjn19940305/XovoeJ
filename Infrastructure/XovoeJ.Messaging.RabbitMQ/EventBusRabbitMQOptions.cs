namespace XovoeJ.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 事件总线配置选项
    /// </summary>
    public class EventBusRabbitMQOptions
    {
        /// <summary>
        /// RabbitMQ 连接地址
        /// </summary>
        public string HostName { get; set; } = "localhost";

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 5672;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = "guest";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "guest";

        /// <summary>
        /// 虚拟主机
        /// </summary>
        public string VirtualHost { get; set; } = "/";

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// 订阅客户端名称前缀
        /// </summary>
        public string SubscriptionClientName { get; set; } = "XovoeJ";

        /// <summary>
        /// 交换机名称
        /// </summary>
        public string BrokerName { get; set; } = "xovoej_event_bus";

        /// <summary>
        /// 是否自动删除队列
        /// </summary>
        public bool AutoDelete { get; set; } = false;

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int Timeout { get; set; } = 30;
    }
}
