using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.HotelManagement.AddRoom;

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

        return ResultToHttpResponseMapper.MapToHttpResponse(result,
            id => Results.Created($"/api/hotels/{hotelId}/rooms/{id}", new { id }));
    }
}
