namespace Shared.Domain.Events;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; }
    public int Version { get; init; } = 1;
    public string IdempotencyKey { get; init; } = Guid.NewGuid().ToString();
}
