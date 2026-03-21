using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.BookingManagement.CancelBooking;

public class CancelBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapDelete("/{bookingId:guid}", CancelBookingHandler)
            .WithName("CancelBooking")
            .RequireAuthorization()
            .WithTags("Bookings")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CancelBookingHandler(
        Guid bookingId,
        IUserContext userContext,
        ISender sender,
        CancellationToken cancellationToken,
        string reason = "Cancelled by guest")
    {
        if (!userContext.IsAuthenticated)
            return Results.Unauthorized();

        var command = new CancelBookingCommand(bookingId, userContext.UserId, reason);
        var result = await sender.Send(command, cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
