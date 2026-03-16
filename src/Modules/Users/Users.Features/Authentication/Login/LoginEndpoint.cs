using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Users.Features.Authentication.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/auth")
            .MapPost("/login", LoginHandler)
            .WithName("Login")
            .AllowAnonymous()
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> LoginHandler(
        LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.DeviceName,
            request.DeviceType,
            request.IpAddress);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }
}
