namespace Shared.Infrastructure.Persistence.Locks;

public interface IDistributedLock : IAsyncDisposable
{
    Task<bool> TryAcquireAsync(TimeSpan wait, CancellationToken cancellationToken = default);
    Task ReleaseAsync(CancellationToken cancellationToken = default);
}
