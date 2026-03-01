using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application.Configuration;

public static class CqrsConfiguration
{
    public static IServiceCollection RegisterCqrsHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CqrsConfiguration).Assembly);

            if (assemblies.Length > 0)
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
            }
        });

        return services;
    }
}
