using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using System.Security.Claims;
using Users.Features.DeviceManagement.RevokeAllDevices;

namespace Users.Presentation.DeviceManagement;

public class RevokeAllDevicesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapPost("/devices/revoke-all", RevokeAllDevicesHandler)
            .WithName("RevokeAllDevices")
            .RequireAuthorization()
            .WithTags("Devices")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> RevokeAllDevicesHandler(
        RevokeAllDevicesRequest request,
        ClaimsPrincipal claims,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
            return Results.Unauthorized();

        var command = new RevokeAllDevicesCommand(userIdGuid, request.Reason ?? "User initiated");
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(result.Error);
    }
}

public record RevokeAllDevicesRequest(string? Reason);
