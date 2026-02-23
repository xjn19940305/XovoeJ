using System;
using RabbitMQ.Client;

namespace XovoeJ.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 持久连接接口
    /// </summary>
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 尝试连接
        /// </summary>
        bool TryConnect();

        /// <summary>
        /// 创建模型
        /// </summary>
        IModel CreateModel();
    }
}
