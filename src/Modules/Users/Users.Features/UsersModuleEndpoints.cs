using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Users.Features;

public static class UsersModuleEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapEndpointsFromAssembly(typeof(UsersModuleEndpoints).Assembly);
    }
}
