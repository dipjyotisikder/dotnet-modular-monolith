using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Authorization;
using Shared.Domain.Services;
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
            .RequireAuthorization()
            .WithTags("Hotels")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> AddRoomHandler(
        Guid hotelId,
        AddRoomRequest request,
        IPermissionService permissionService,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!permissionService.HasPermission(Permission.RoomCreate))
            return Results.Forbid();
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
