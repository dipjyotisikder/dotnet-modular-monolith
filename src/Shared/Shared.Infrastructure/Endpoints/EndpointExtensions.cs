using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Shared.Infrastructure.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpointsFromAssembly(
        this IEndpointRouteBuilder app,
        Assembly assembly)
    {
        var endpointType = typeof(IEndpoint);

        var endpointImplementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        endpointType.IsAssignableFrom(t))
            .ToList();

        foreach (var implementation in endpointImplementations)
        {
            var instance = Activator.CreateInstance(implementation);
            if (instance is IEndpoint endpoint)
                endpoint.MapEndpoint(app);
        }

        return app;
    }
}
