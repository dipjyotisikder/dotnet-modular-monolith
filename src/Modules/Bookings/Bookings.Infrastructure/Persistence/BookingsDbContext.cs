using Bookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.Persistence;

public class BookingsDbContext(
    DbContextOptions<BookingsDbContext> options,
    AuditableEntityInterceptor auditableEntityInterceptor,
    DomainEventOutboxInterceptor domainEventOutboxInterceptor) : DbContext(options)
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor = auditableEntityInterceptor;
    private readonly DomainEventOutboxInterceptor _domainEventOutboxInterceptor = domainEventOutboxInterceptor;

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor, _domainEventOutboxInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Bookings");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
    }
}
