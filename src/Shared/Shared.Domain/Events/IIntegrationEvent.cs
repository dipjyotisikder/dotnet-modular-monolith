namespace Shared.Domain.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    Guid CorrelationId { get; }
    DateTime OccurredOn { get; }
    int Version { get; }
    string IdempotencyKey { get; }
}
