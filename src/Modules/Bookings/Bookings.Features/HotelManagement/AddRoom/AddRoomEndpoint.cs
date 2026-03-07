using Bookings.Features.HotelManagement.AddRoom;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Presentation.HotelManagement;

public class AddRoomEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapPost("/{hotelId:guid}/rooms", AddRoomHandler)
            .WithName("AddRoom")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Hotels")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> AddRoomHandler(
        Guid hotelId,
        AddRoomRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddRoomCommand(
            hotelId,
            request.RoomNumber,
            request.RoomType,
            request.PricePerNight,
            request.Currency,
            request.MaxOccupancy,
            request.Description);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/hotels/{hotelId}/rooms/{result.Value}", new { id = result.Value })
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }
}
