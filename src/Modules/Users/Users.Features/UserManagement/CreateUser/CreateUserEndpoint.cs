using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;

namespace Users.Features.UserManagement.CreateUser;

public class CreateUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapPost("/", CreateUserHandler)
            .WithName("CreateUser")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users")
            .Produces(StatusCodes.Status201Created);
    }

    private static async Task<IResult> CreateUserHandler(
        CreateUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.Name);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
            : Results.BadRequest(result.Error);
    }
}
