using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Users.Features.Authentication.Logout;

public class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/auth")
            .MapPost("/logout", LogoutHandler)
            .WithName("Logout")
            .RequireAuthorization()
            .WithTags("Authentication")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> LogoutHandler(
        LogoutRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.DeviceId);
        var result = await sender.Send(command, cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}

public record LogoutRequest(string DeviceId);
