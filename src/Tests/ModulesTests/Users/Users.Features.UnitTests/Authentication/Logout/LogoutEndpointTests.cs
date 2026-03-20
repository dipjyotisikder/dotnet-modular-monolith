using Microsoft.AspNetCore.Routing;
using Moq;
using Users.Features.Authentication.Logout;


namespace Users.Features.UnitTests.Authentication.Logout;

public class LogoutEndpointTests
{
    [Fact]
    public void MapEndpoint_ValidApp_ExecutesWithoutThrowing()
    {
        var endpoint = new LogoutEndpoint();
        var mockApp = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockRouteGroupBuilder = new Mock<IEndpointRouteBuilder>();

        mockApp.Setup(a => a.ServiceProvider).Returns(mockServiceProvider.Object);
        mockApp.Setup(a => a.DataSources).Returns(new List<EndpointDataSource>());

        var exception = Record.Exception(() => endpoint.MapEndpoint(mockApp.Object));

        Assert.Null(exception);
    }
}
