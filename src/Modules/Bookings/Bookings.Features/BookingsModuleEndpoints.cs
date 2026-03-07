using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Features;

public static class BookingsModuleEndpoints
{
    public static void MapBookingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapEndpointsFromAssembly(typeof(BookingsModuleEndpoints).Assembly);
    }
}
