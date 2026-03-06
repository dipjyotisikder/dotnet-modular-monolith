using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Users.Features.UserManagement.GetUsers;

namespace Users.Presentation.UserManagement;

public class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapGet("/", GetUsersHandler)
            .WithName("GetUsers")
            .RequireAuthorization("AdminPolicy")
            .WithTags("Users");
    }

    private static async Task<IResult> GetUsersHandler(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery();
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
