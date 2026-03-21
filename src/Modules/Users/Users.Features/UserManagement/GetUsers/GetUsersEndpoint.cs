using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Infrastructure.Endpoints;
using Shared.Infrastructure.Mappers;

namespace Users.Features.UserManagement.GetUsers;

public class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/users")
            .MapGet("/", GetUsersHandler)
            .WithName("GetUsers")
            .RequireAuthorization("CanListUsers")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetUsersHandler(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery();
        var result = await sender.Send(query, cancellationToken);

        return ResultToHttpResponseMapper.MapToHttpResponse(result);
    }
}
