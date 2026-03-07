using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Seeding;

namespace Shared.Infrastructure.Seeding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSeeding(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        var seederType = typeof(ISeeder);

        foreach (var assembly in assemblies)
        {
            var seederTypes = assembly
                .GetTypes()
                .Where(p => seederType.IsAssignableFrom(p)
                        && p.IsClass
                        && !p.IsAbstract
                        && p != seederType);

            foreach (var type in seederTypes)
            {
                services.AddScoped(seederType, type);
            }
        }

        return services;
    }
}
