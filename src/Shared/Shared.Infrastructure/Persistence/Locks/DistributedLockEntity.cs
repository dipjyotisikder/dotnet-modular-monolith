namespace Shared.Infrastructure.Persistence.Locks;

public class DistributedLockEntity
{
    public string LockKey { get; set; } = null!;
    public string? InstanceId { get; set; }
    public DateTime AcquiredAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
