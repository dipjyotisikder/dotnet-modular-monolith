using Bookings.Features.HotelManagement.CreateHotel;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Bookings.Presentation.HotelManagement;

public class CreateHotelEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapPost("/", CreateHotelHandler)
            .WithName("CreateHotel")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Hotels")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> CreateHotelHandler(
        CreateHotelRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateHotelCommand(
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.Country,
            request.ZipCode);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/hotels/{result.Value}", new { id = result.Value })
            : Results.BadRequest(result.Error);
    }
}
