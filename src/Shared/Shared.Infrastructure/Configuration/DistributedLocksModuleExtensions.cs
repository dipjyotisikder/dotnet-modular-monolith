namespace Shared.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Locks;

public static class DistributedLocksModuleExtensions
{
    public static IServiceCollection AddDistributedLocksModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DistributedLocksDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDistributedLockFactory, DatabaseDistributedLockFactory>();

        return services;
    }
}
