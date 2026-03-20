using Microsoft.AspNetCore.Routing;
using Users.Features.Authentication.RefreshToken;

namespace Users.Features.UnitTests.Authentication.RefreshToken;

public class RefreshTokenEndpointTests
{
    [Fact]
    public void MapEndpoint_WithNullEndpointRouteBuilder_ThrowsException()
    {
        var endpoint = new RefreshTokenEndpoint();
        IEndpointRouteBuilder nullApp = null!;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullApp));
    }
}
