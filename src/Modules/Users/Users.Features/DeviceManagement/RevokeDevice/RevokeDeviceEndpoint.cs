using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Users.Features.DeviceManagement.RevokeDevice;

namespace Users.Presentation.DeviceManagement;

public class RevokeDeviceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapPost("/devices/{deviceId}/revoke", RevokeDeviceHandler)
            .WithName("RevokeDevice")
            .RequireAuthorization()
            .WithTags("Devices");
    }

    private static async Task<IResult> RevokeDeviceHandler(
        string deviceId,
        RevokeDeviceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RevokeDeviceCommand(deviceId, request.Reason ?? "User initiated");
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(result.Error);
    }
}

public record RevokeDeviceRequest(string? Reason);
