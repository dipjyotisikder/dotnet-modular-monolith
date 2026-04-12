using Microsoft.AspNetCore.Routing;
using System.Reflection;

namespace Shared.Infrastructure.Endpoints;

public static class EndpointExtensions
{
    public static void MapEndpointsFromAssembly(this IEndpointRouteBuilder app,
        Assembly assembly)
    {
        var endpointType = typeof(IEndpoint);

        var endpointImplementations = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        endpointType.IsAssignableFrom(t))
            .ToList();

        foreach (var instance in endpointImplementations.Select(Activator.CreateInstance))
        {
            if (instance is IEndpoint endpoint)
                endpoint.MapEndpoint(app);
        }
    }
}
