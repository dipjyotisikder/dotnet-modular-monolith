namespace Shared.Infrastructure.Entities;

using Shared.Domain.Services;

public class OutboxMessage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string EventType { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public int EventVersion { get; private set; } = 1;
    public string IdempotencyKey { get; private set; } = null!;
    public Guid CorrelationId { get; private set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private set; }
    public bool Sent { get; private set; } = false;
    public int RetryCount { get; private set; } = 0;
    public DateTime? ProcessedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsDeadLettered { get; private set; } = false;
    public DateTime? DeadLetteredAt { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(
        string eventType,
        string payload,
        int eventVersion,
        string idempotencyKey,
        Guid correlationId,
        ISystemClock clock)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be empty", nameof(eventType));

        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException("Payload cannot be empty", nameof(payload));

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key cannot be empty", nameof(idempotencyKey));

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Payload = payload,
            EventVersion = eventVersion,
            IdempotencyKey = idempotencyKey,
            CorrelationId = correlationId,
            CreatedAt = clock.UtcNow
        };
    }

    public void MarkAsSent(ISystemClock clock)
    {
        if (IsDeadLettered)
            throw new InvalidOperationException("Cannot mark dead-lettered message as sent");

        Sent = true;
        ProcessedAt = clock.UtcNow;
    }

    public void RecordFailure(string errorMessage)
    {
        if (IsDeadLettered)
            throw new InvalidOperationException("Cannot record failure on dead-lettered message");

        RetryCount++;
        ErrorMessage = errorMessage;
    }

    public void MoveToDeadLetterQueue(ISystemClock clock, string reason)
    {
        if (IsDeadLettered)
            throw new InvalidOperationException("Message already in dead letter queue");

        IsDeadLettered = true;
        DeadLetteredAt = clock.UtcNow;
        ErrorMessage = reason;
        ProcessedAt = clock.UtcNow;
    }

    public bool ShouldRetry(int maxRetries) => !Sent && !IsDeadLettered && RetryCount < maxRetries;
}
