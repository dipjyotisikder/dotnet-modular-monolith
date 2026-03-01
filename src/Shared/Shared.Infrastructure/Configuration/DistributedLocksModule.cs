namespace Shared.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence.DistributedLocks;
using Shared.Infrastructure.Persistence.Locks;

public static class DistributedLocksModule
{
    public static IServiceCollection AddDistributedLocksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DistributedLocksDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("LocksDb")));

        services.AddScoped<IDistributedLockFactory, DatabaseDistributedLockFactory>();

        return services;
    }
}
