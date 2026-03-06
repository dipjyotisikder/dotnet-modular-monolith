using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain;
using Shared.Domain.Services;
using Shared.Infrastructure.Entities;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Persistence.Interceptors;

public class DomainEventOutboxInterceptor(ISystemClock clock, IServiceScopeFactory scopeFactory)
    : SaveChangesInterceptor
{
    private List<OutboxMessage> _pendingOutboxMessages = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _pendingOutboxMessages = [];

        var context = eventData.Context;
        if (context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        foreach (var entry in context.ChangeTracker.Entries<Entity>()
                     .Where(e => e.Entity.DomainEvents.Count > 0))
        {
            foreach (var domainEvent in entry.Entity.DomainEvents)
            {
                var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
                var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
                var idempotencyKey = $"{entry.Entity.Id}-{domainEvent.EventId}";

                _pendingOutboxMessages.Add(OutboxMessage.Create(
                    eventType: eventType,
                    payload: payload,
                    eventVersion: 1,
                    idempotencyKey: idempotencyKey,
                    correlationId: domainEvent.EventId,
                    clock: clock));
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<Entity>())
                entry.Entity.ClearDomainEvents();
        }

        if (_pendingOutboxMessages.Count > 0)
        {
            using var scope = scopeFactory.CreateScope();
            var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

            outboxDb.OutboxMessages.AddRange(_pendingOutboxMessages);
            await outboxDb.SaveChangesAsync(cancellationToken);

            _pendingOutboxMessages = [];
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pendingOutboxMessages = [];
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }
}
