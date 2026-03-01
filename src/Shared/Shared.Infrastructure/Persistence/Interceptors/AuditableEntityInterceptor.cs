using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain;
using Shared.Domain.Services;

namespace Shared.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ISystemClock _clock;
    private readonly IUserContext _userContext;

    public AuditableEntityInterceptor(ISystemClock clock, IUserContext userContext)
    {
        _clock = clock;
        _userContext = userContext;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var entries = context.ChangeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var userId = _userContext.IsAuthenticated ? _userContext.UserId : Guid.Empty;
        var now = _clock.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedBy)).CurrentValue = userId;
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = now;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.ModifiedBy)).CurrentValue = userId;
                entry.Property(nameof(IAuditableEntity.ModifiedAt)).CurrentValue = now;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
