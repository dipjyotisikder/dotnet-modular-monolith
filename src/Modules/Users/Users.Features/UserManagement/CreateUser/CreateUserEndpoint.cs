using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

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

        return ResultToHttpResponseMapper.MapToHttpResponse(result,
            id => Results.Created($"/api/users/{id}", new { id }));
    }
}
