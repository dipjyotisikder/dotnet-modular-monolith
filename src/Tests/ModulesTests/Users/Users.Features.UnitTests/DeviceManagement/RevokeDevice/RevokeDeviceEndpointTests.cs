using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Moq;
using Users.Features.DeviceManagement.RevokeDevice;


namespace Users.Features.UnitTests.DeviceManagement.RevokeDevice;

public class RevokeDeviceEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        var endpoint = new RevokeDeviceEndpoint();
        IEndpointRouteBuilder? app = null;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app!));
    }

    [Fact]
    public void MapEndpoint_ValidApp_ExecutesWithoutThrowing()
    {
        var endpoint = new RevokeDeviceEndpoint();
        var mockApp = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDataSources = new List<EndpointDataSource>();

        mockApp.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockApp.Setup(x => x.DataSources).Returns(mockDataSources);
        mockApp.Setup(x => x.CreateApplicationBuilder()).Returns(new Mock<IApplicationBuilder>().Object);

        var exception = Record.Exception(() => endpoint.MapEndpoint(mockApp.Object));

        Assert.Null(exception);
    }
}
