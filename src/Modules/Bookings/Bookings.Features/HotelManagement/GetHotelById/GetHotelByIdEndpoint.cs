using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Features.HotelManagement.GetHotelById;

public class GetHotelByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapGet("/{hotelId:guid}", GetHotelByIdHandler)
            .WithName("GetHotelById")
            .AllowAnonymous()
            .WithTags("Hotels")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetHotelByIdHandler(
        Guid hotelId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHotelByIdQuery(hotelId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.ErrorCode == ErrorCodes.RESOURCE_NOT_FOUND
                ? Results.NotFound(result.Error)
                : Results.BadRequest(result.Error);
    }
}
