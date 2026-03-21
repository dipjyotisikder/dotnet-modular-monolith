using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.HotelManagement.SearchAvailableRooms;

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

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
