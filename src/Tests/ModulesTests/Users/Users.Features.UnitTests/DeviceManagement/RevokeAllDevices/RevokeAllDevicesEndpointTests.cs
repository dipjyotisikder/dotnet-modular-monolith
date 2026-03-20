using Microsoft.AspNetCore.Routing;
using Moq;
using Users.Features.DeviceManagement.RevokeAllDevices;


namespace Users.Features.UnitTests.DeviceManagement.RevokeAllDevices;

public class RevokeAllDevicesEndpointTests
{
    [Fact]
    public void MapEndpoint_ValidEndpointRouteBuilder_DoesNotThrow()
    {
        var mockRouteBuilder = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockRouteBuilder.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockRouteBuilder.Setup(x => x.DataSources).Returns(new System.Collections.Generic.List<EndpointDataSource>());

        var endpoint = new RevokeAllDevicesEndpoint();

        var exception = Record.Exception(() => endpoint.MapEndpoint(mockRouteBuilder.Object));

        Assert.Null(exception);
    }

    [Fact]
    public void MapEndpoint_NullEndpointRouteBuilder_ThrowsNullReferenceException()
    {
        var endpoint = new RevokeAllDevicesEndpoint();
        IEndpointRouteBuilder? nullRouteBuilder = null;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullRouteBuilder!));
    }
}
