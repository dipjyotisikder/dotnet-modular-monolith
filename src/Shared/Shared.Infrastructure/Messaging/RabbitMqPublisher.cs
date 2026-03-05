using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Infrastructure.Configuration.Options;
using System.Text;

namespace Shared.Infrastructure.Messaging;

public class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private bool _disposed;
    private readonly bool _isConnected;

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _logger = logger;
        _rabbitMqOptions = rabbitMqOptions.Value;

        if (!_rabbitMqOptions.Enabled)
        {
            _logger.LogInformation("RabbitMQ is disabled");
            _isConnected = false;
            return;
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.Host,
                Port = _rabbitMqOptions.Port,
                UserName = _rabbitMqOptions.Username,
                Password = _rabbitMqOptions.Password,
                VirtualHost = _rabbitMqOptions.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _rabbitMqOptions.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _isConnected = true;
            _logger.LogInformation("RabbitMQ initialized with exchange: {Exchange}", _rabbitMqOptions.Exchange);
        }
        catch (Exception ex)
        {
            _isConnected = false;
            _logger.LogError(ex, "RabbitMQ connection failed");
        }
    }

    public Task PublishAsync(string eventType, string payload, string idempotencyKey)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ not connected: {EventType}", eventType);
            return Task.CompletedTask;
        }

        try
        {
            var body = Encoding.UTF8.GetBytes(payload);
            var properties = _channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventType;
            properties.MessageId = idempotencyKey;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers ??= new Dictionary<string, object?>();
            properties.Headers["x-idempotency-key"] = idempotencyKey;

            _channel.BasicPublish(
                exchange: _rabbitMqOptions.Exchange,
                routingKey: eventType,
                basicProperties: properties,
                body: body);

            _logger.LogDebug("Published message {IdempotencyKey} of type {EventType}", idempotencyKey, eventType);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType}", eventType);
            throw;
        }
    }

    public Task PublishToDeadLetterQueueAsync(string eventType, string payload, string reason)
    {
        if (!_isConnected || _channel == null)
        {
            _logger.LogWarning("RabbitMQ not connected for DLQ: {EventType}", eventType);
            return Task.CompletedTask;
        }

        try
        {
            const string dlqExchange = "dlq-exchange";
            _channel.ExchangeDeclare(
                exchange: dlqExchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(payload);
            var properties = _channel.CreateBasicProperties();

            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventType;
            properties.Headers ??= new Dictionary<string, object?>();
            properties.Headers["x-dlq-reason"] = reason;
            properties.Headers["x-dlq-timestamp"] = DateTimeOffset.UtcNow.ToString("O");

            _channel.BasicPublish(
                exchange: dlqExchange,
                routingKey: $"{eventType}.dlq",
                basicProperties: properties,
                body: body);

            _logger.LogWarning("Message {EventType} sent to DLQ. Reason: {Reason}", eventType, reason);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish to DLQ: {EventType}", eventType);
            throw;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RabbitMqPublisher()
    {
        Dispose(false);
    }
}

