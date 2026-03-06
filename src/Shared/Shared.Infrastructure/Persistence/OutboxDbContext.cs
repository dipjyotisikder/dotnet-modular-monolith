using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Entities;

namespace Shared.Infrastructure.Persistence;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Shared");

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("Outbox");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CorrelationId).IsRequired();
            entity.Property(e => e.EventVersion).IsRequired();

            entity.HasIndex(e => new { e.IsDeadLettered, e.Sent, e.RetryCount });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => e.CorrelationId);
            entity.HasIndex(e => e.DeadLetteredAt);
        });
    }
}
