using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Authorization;
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

    public static IServiceCollection RegisterAuthorizationHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IAuthorizationRequirementHandler<>);
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Implementation = t,
                Interfaces = t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                    .ToList()
            })
            .Where(x => x.Interfaces.Any())
            .ToList();

        foreach (var handler in handlerTypes)
        {
            foreach (var @interface in handler.Interfaces)
            {
                services.AddScoped(@interface, handler.Implementation);
            }
        }

        return services;
    }
}
