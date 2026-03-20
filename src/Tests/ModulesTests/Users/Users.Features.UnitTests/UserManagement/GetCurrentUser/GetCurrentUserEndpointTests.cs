using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Users.Features.UserManagement.GetCurrentUser;

namespace Users.Features.UnitTests.UserManagement.GetCurrentUser;

public class GetCurrentUserEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsException()
    {
        var endpoint = new GetCurrentUserEndpoint();
        IEndpointRouteBuilder? nullApp = null;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullApp!));
    }

    [Fact]
    public void MapEndpoint_ValidApp_ConfiguresEndpoint()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        var endpoint = new GetCurrentUserEndpoint();

        endpoint.MapEndpoint(app);

        Assert.NotNull(app);
    }
}
