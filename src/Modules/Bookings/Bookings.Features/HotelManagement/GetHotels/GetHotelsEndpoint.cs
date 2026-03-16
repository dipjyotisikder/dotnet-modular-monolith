using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Features.HotelManagement.GetHotels;

public class GetHotelsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapGet("/", GetHotelsHandler)
            .WithName("GetHotels")
            .AllowAnonymous()
            .WithTags("Hotels")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetHotelsHandler(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHotelsQuery(), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
