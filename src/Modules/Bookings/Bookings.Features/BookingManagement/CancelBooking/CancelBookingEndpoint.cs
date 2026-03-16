using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;

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
            .Produces(StatusCodes.Status200OK);
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

        return result.IsSuccess
            ? Results.Ok(new { message = "Booking cancelled successfully" })
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : result.ErrorCode == ErrorCodes.FORBIDDEN
                    ? Results.Forbid()
                    : Results.BadRequest(result.Error);
    }
}
