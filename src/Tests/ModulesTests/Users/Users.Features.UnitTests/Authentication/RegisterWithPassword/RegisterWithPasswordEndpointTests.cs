using Microsoft.AspNetCore.Routing;
using Users.Features.Authentication.RegisterWithPassword;

namespace Users.Features.UnitTests.Authentication.RegisterWithPassword;

public class RegisterWithPasswordEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        var endpoint = new RegisterWithPasswordEndpoint();
        IEndpointRouteBuilder app = null!;

        var exception = Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
        Assert.Equal("endpoints", exception.ParamName);
    }
}
