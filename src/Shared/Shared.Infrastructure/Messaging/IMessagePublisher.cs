namespace Shared.Infrastructure.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(string eventType, string payload, string idempotencyKey);
    Task PublishToDeadLetterQueueAsync(string eventType, string payload, string reason);
}
