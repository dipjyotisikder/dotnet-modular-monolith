using Bookings.Features.BookingManagement.CompleteBooking;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Presentation.BookingManagement;

public class CompleteBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapPost("/{bookingId:guid}/complete", CompleteBookingHandler)
            .WithName("CompleteBooking")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Bookings")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CompleteBookingHandler(
        Guid bookingId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteBookingCommand(bookingId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(new { message = "Booking completed successfully" })
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }
}
