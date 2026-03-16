using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Users.Features.Authentication.RegisterWithPassword;

public class RegisterWithPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/auth")
            .MapPost("/register", RegisterHandler)
            .WithName("Register")
            .AllowAnonymous()
            .WithTags("Authentication")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> RegisterHandler(
        RegisterWithPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RegisterWithPasswordCommand(request.Email, request.Name, request.Password);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
            : Results.BadRequest(result.Error);
    }
}
