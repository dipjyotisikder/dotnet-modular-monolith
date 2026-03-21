using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.BookingManagement.CompleteBooking;

public class CompleteBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapPost("/{bookingId:guid}/complete", CompleteBookingHandler)
            .WithName("CompleteBooking")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Bookings")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CompleteBookingHandler(
        Guid bookingId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteBookingCommand(bookingId), cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
