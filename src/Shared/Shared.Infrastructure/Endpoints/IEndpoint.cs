using Microsoft.AspNetCore.Routing;

namespace Shared.Infrastructure.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
