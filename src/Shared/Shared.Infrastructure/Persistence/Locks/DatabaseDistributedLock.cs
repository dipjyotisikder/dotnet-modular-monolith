namespace Shared.Infrastructure.Persistence.Locks;

using Microsoft.EntityFrameworkCore;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.DistributedLocks;

public class DatabaseDistributedLock : IDistributedLock
{
    private readonly DistributedLocksDbContext _db;
    private readonly ISystemClock _clock;
    private readonly string _key;
    private readonly string _instanceId;
    private readonly TimeSpan _expiryTime;
    private bool _acquired;

    public DatabaseDistributedLock(DistributedLocksDbContext db, ISystemClock clock, string key, TimeSpan? expiryTime = null)
    {
        _db = db;
        _clock = clock;
        _key = key;
        _instanceId = Guid.NewGuid().ToString();
        _expiryTime = expiryTime ?? TimeSpan.FromMinutes(5);
        _acquired = false;
    }

    public async Task<bool> TryAcquireAsync(TimeSpan wait, CancellationToken cancellationToken = default)
    {
        var deadline = _clock.UtcNow.Add(wait);

        while (_clock.UtcNow < deadline)
        {
            var now = _clock.UtcNow;
            var expiresAt = now.Add(_expiryTime);

            await CleanupExpiredLocksAsync(cancellationToken);

            var existingLock = await _db.Locks
                .Where(l => l.LockKey == _key)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingLock == null)
            {
                var newLock = new DistributedLockEntity
                {
                    LockKey = _key,
                    InstanceId = _instanceId,
                    AcquiredAt = now,
                    ExpiresAt = expiresAt
                };

                _db.Locks.Add(newLock);

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                    _acquired = true;
                    return true;
                }
                catch (DbUpdateException)
                {
                }
            }
            else if (existingLock.ExpiresAt < now)
            {
                existingLock.InstanceId = _instanceId;
                existingLock.AcquiredAt = now;
                existingLock.ExpiresAt = expiresAt;

                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                    _acquired = true;
                    return true;
                }
                catch (DbUpdateException)
                {
                }
            }

            await Task.Delay(100, cancellationToken);
        }

        return false;
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken = default)
    {
        if (!_acquired) return;

        var lockEntity = await _db.Locks
            .Where(l => l.LockKey == _key && l.InstanceId == _instanceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (lockEntity != null)
        {
            _db.Locks.Remove(lockEntity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        _acquired = false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_acquired)
        {
            await ReleaseAsync();
        }
    }

    private async Task CleanupExpiredLocksAsync(CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var expiredLocks = await _db.Locks
            .Where(l => l.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        if (expiredLocks.Any())
        {
            _db.Locks.RemoveRange(expiredLocks);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
