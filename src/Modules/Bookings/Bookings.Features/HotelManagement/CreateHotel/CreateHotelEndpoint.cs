using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Domain.Authorization;
using Shared.Domain.Services;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Bookings.Features.HotelManagement.CreateHotel;

public class CreateHotelEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/hotels")
            .MapPost("/", CreateHotelHandler)
            .WithName("CreateHotel")
            .RequireAuthorization()
            .WithTags("Hotels")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateHotelHandler(
        CreateHotelRequest request,
        IPermissionService permissionService,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!permissionService.HasPermission(Permission.HotelCreate))
            return Results.Forbid();
        var command = new CreateHotelCommand(
            request.Name,
            request.Description,
            request.StarRating,
            request.Street,
            request.City,
            request.Country,
            request.ZipCode);

        var result = await sender.Send(command, cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result,
            id => Results.Created($"/api/hotels/{id}", new { id }));
    }
}
