using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Locks;

namespace Shared.Infrastructure.Persistence;

public class DistributedLocksDbContext : DbContext
{
    public DistributedLocksDbContext(DbContextOptions<DistributedLocksDbContext> options) : base(options)
    {
    }

    public DbSet<DistributedLock> Locks => Set<DistributedLock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Shared");

        modelBuilder.Entity<DistributedLock>(entity =>
        {
            entity.ToTable("DistributedLocks");
            entity.HasKey(e => e.LockKey);
            entity.Property(e => e.LockKey).HasMaxLength(200);
            entity.Property(e => e.InstanceId).HasMaxLength(100);
            entity.Property(e => e.AcquiredAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
