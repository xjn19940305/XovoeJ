using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Net.Sockets;
using RabbitMQ.Client.Exceptions;

namespace XovoeJ.Messaging.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 默认持久连接实现
    /// </summary>
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
        private readonly EventBusRabbitMQOptions _options;
        private IConnection? _connection;
        private readonly object _syncRoot = new();
        private bool _disposed;

        public DefaultRabbitMQPersistentConnection(
            IConnectionFactory connectionFactory,
            ILogger<DefaultRabbitMQPersistentConnection> logger,
            IOptions<EventBusRabbitMQOptions> options)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _options = options.Value;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public bool TryConnect()
        {
            lock (_syncRoot)
            {
                if (IsConnected)
                {
                    return true;
                }

                var policy = Policy
                    .Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_options.RetryCount, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                    _connection.ConnectionUnblocked += OnConnectionUnblocked;

                    _logger.LogInformation(
                        "RabbitMQ persistent connection acquired a connection to '{HostName}' and is subscribed to failure events",
                        _connection.Endpoint.HostName);

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("RabbitMQ connection is not connected");
            }

            return _connection!.CreateModel();
        }

        private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ connection is blocked. Reason: {Reason}", e.Reason);
        }

        private void OnConnectionUnblocked(object? sender, EventArgs e)
        {
            if (_disposed) return;

            _logger.LogInformation("RabbitMQ connection is unblocked");
        }

        private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ callback exception: {Exception}", e.Exception.Message);
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("RabbitMQ connection is on shutdown: {Reason}", reason.ReplyText);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                if (_connection != null)
                {
                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.CallbackException -= OnCallbackException;
                    _connection.ConnectionBlocked -= OnConnectionBlocked;
                    _connection.ConnectionUnblocked -= OnConnectionUnblocked;
                    _connection.Dispose();
                }
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex, "RabbitMQ connection dispose failed");
            }
        }
    }
}
