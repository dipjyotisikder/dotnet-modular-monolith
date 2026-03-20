using Microsoft.AspNetCore.Routing;
using Users.Features.Authentication.Login;

namespace Users.Features.UnitTests.Authentication.Login;

public class LoginEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        var endpoint = new LoginEndpoint();
        IEndpointRouteBuilder app = null!;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
    }
}
