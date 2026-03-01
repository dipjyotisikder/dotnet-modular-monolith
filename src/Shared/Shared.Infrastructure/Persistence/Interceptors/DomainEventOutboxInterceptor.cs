using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain;
using Shared.Domain.Entities;
using Shared.Domain.Services;
using System.Text.Json;

namespace Shared.Infrastructure.Persistence.Interceptors;

public class DomainEventOutboxInterceptor(ISystemClock clock) : SaveChangesInterceptor
{
    private readonly ISystemClock _clock = clock;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entitiesWithDomainEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();

        foreach (var entry in entitiesWithDomainEvents)
        {
            var domainEvents = entry.Entity.DomainEvents.ToList();

            foreach (var domainEvent in domainEvents)
            {
                var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
                var payload = JsonSerializer.Serialize(domainEvent);
                var idempotencyKey = $"{entry.Entity.Id}-{domainEvent.EventId}";

                var outboxMessage = OutboxMessage.Create(
                    eventType: eventType,
                    payload: payload,
                    eventVersion: 1,
                    idempotencyKey: idempotencyKey,
                    correlationId: domainEvent.EventId,
                    clock: _clock);

                context.Set<OutboxMessage>().Add(outboxMessage);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
