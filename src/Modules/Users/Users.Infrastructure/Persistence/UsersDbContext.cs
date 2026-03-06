using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence.Interceptors;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence;

public class UsersDbContext(
    DbContextOptions<UsersDbContext> options,
    AuditableEntityInterceptor auditableEntityInterceptor,
    DomainEventOutboxInterceptor domainEventOutboxInterceptor) : DbContext(options)
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor = auditableEntityInterceptor;
    private readonly DomainEventOutboxInterceptor _domainEventOutboxInterceptor = domainEventOutboxInterceptor;

    public DbSet<User> Users => Set<User>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor, _domainEventOutboxInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Users");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);
    }
}
