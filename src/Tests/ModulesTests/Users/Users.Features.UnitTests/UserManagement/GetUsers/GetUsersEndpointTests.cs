using Microsoft.AspNetCore.Routing;
using Users.Features.UserManagement.GetUsers;

namespace Users.Features.UnitTests.UserManagement.GetUsers;

public class GetUsersEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        var endpoint = new GetUsersEndpoint();
        IEndpointRouteBuilder? app = null;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app!));
    }
}
