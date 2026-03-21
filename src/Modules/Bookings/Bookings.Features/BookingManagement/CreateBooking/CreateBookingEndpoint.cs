using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.BookingManagement.CreateBooking;

public class CreateBookingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/bookings")
            .MapPost("/", CreateBookingHandler)
            .WithName("CreateBooking")
            .RequireAuthorization()
            .WithTags("Bookings")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
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

        return ResultToHttpResponseMapper.MapToHttpResponse(
            result,
            id => Results.Created($"/api/bookings/{id}", new { id }));
    }
}
