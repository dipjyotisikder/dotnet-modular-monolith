namespace Shared.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Repositories;
using System.Reflection;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoriesFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        var repositoryInterfaceType = typeof(IRepository<>);

        var repositoryImplementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == repositoryInterfaceType))
            .ToList();

        foreach (var implementation in repositoryImplementations)
        {
            var allInterfaces = implementation.GetInterfaces();

            var genericInterfaces = allInterfaces
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == repositoryInterfaceType)
                .ToList();

            foreach (var @interface in genericInterfaces)
            {
                services.AddScoped(@interface, implementation);
            }

            var customRepositoryInterfaces = allInterfaces
                .Where(i => !i.IsGenericType &&
                           i.IsInterface &&
                           i.GetInterfaces().Any(iface =>
                               iface.IsGenericType &&
                               iface.GetGenericTypeDefinition() == repositoryInterfaceType))
                .ToList();

            foreach (var customInterface in customRepositoryInterfaces)
            {
                services.AddScoped(customInterface, implementation);
            }
        }

        return services;
    }
}
