using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;
using System.Security.Claims;

namespace Users.Features.UserManagement.GetCurrentUser;

public class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapGet("/me", GetCurrentUserHandler)
            .WithName("GetCurrentUser")
            .RequireAuthorization()
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetCurrentUserHandler(
        ClaimsPrincipal claims,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var query = new GetCurrentUserQuery(userId);
        var result = await sender.Send(query, cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
