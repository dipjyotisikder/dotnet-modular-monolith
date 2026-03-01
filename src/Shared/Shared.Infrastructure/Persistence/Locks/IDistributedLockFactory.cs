namespace Shared.Infrastructure.Persistence.Locks;

public interface IDistributedLockFactory
{
    IDistributedLock CreateLock(string key, TimeSpan? expiryTime = null);
}
