namespace Shared.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Locks;

public static class DistributedLocksModuleExtensions
{
    public static void AddDistributedLocksModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DistributedLocksDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDistributedLockFactory, DatabaseDistributedLockFactory>();
    }
}
