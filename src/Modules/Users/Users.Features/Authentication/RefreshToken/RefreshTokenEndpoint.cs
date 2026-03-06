using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Users.Features.Authentication.RefreshToken;

namespace Users.Presentation.Authentication;

public class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/auth")
            .MapPost("/refresh", RefreshHandler)
            .WithName("RefreshToken")
            .AllowAnonymous()
            .WithTags("Authentication");
    }

    private static async Task<IResult> RefreshHandler(
        RefreshTokenRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken, request.DeviceId);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }
}
