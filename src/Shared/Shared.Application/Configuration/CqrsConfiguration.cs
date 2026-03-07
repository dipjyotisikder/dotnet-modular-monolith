using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application.Configuration;

public static class CqrsConfiguration
{
    public static IServiceCollection RegisterCqrsHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CqrsConfiguration).Assembly);
            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }
}
