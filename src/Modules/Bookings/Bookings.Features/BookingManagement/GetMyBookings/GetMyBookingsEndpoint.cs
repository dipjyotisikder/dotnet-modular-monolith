using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Features.BookingManagement.GetMyBookings;

public class GetMyBookingsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapGet("/my", GetMyBookingsHandler)
            .WithName("GetMyBookings")
            .RequireAuthorization()
            .WithTags("Bookings")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetMyBookingsHandler(
        IUserContext userContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsAuthenticated)
            return Results.Unauthorized();

        var result = await sender.Send(new GetMyBookingsQuery(userContext.UserId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
