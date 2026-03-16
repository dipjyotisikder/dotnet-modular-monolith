using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using System.Security.Claims;

namespace Users.Features.DeviceManagement.GetUserDevices;

public class GetUserDevicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapGet("/devices", GetUserDevicesHandler)
            .WithName("GetUserDevices")
            .RequireAuthorization()
            .WithTags("Devices")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetUserDevicesHandler(
        ClaimsPrincipal claims,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            return Results.Unauthorized();

        var query = new GetUserDevicesQuery(userIdGuid);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
