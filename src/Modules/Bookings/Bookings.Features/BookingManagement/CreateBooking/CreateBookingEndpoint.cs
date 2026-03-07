using Bookings.Features.BookingManagement.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Presentation.BookingManagement;

public class CreateBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapPost("/", CreateBookingHandler)
            .WithName("CreateBooking")
            .RequireAuthorization()
            .WithTags("Bookings")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> CreateBookingHandler(
        CreateBookingRequest request,
        IUserContext userContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsAuthenticated)
            return Results.Unauthorized();

        var command = new CreateBookingCommand(
            userContext.UserId,
            request.HotelId,
            request.RoomId,
            request.CheckIn,
            request.CheckOut);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/bookings/{result.Value}", new { id = result.Value })
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }
}
