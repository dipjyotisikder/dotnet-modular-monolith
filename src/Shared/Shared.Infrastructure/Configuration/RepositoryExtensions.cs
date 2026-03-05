namespace Shared.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Repositories;
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
            var interfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == repositoryInterfaceType)
                .ToList();

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, implementation);
            }
        }

        return services;
    }
}
