using Bookings.Features.HotelManagement.SearchAvailableRooms;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Presentation.HotelManagement;

public class SearchAvailableRoomsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapGet("/{hotelId:guid}/rooms/available", SearchAvailableRoomsHandler)
            .WithName("SearchAvailableRooms")
            .AllowAnonymous()
            .WithTags("Hotels")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> SearchAvailableRoomsHandler(
        Guid hotelId,
        DateTime checkIn,
        DateTime checkOut,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SearchAvailableRoomsQuery(hotelId, checkIn, checkOut),
            cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }
}
