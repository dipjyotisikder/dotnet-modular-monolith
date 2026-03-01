namespace Shared.Infrastructure.Persistence.Locks;

using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.DistributedLocks;

public class DatabaseDistributedLockFactory : IDistributedLockFactory
{
    private readonly DistributedLocksDbContext _db;
    private readonly ISystemClock _clock;

    public DatabaseDistributedLockFactory(DistributedLocksDbContext db, ISystemClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public IDistributedLock CreateLock(string key, TimeSpan? expiryTime = null)
    {
        return new DatabaseDistributedLock(_db, _clock, key, expiryTime);
    }
}
