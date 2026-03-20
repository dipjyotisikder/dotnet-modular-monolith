using Microsoft.AspNetCore.Routing;
using Moq;
using Users.Features.UserManagement.CreateUser;


namespace Users.Features.UnitTests.UserManagement.CreateUser;

public class CreateUserEndpointTests
{
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        var endpoint = new CreateUserEndpoint();
        IEndpointRouteBuilder app = null!;

        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
    }

    [Fact]
    public void MapEndpoint_ValidApp_ExecutesWithoutThrowing()
    {
        var endpoint = new CreateUserEndpoint();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
        var mockEndpointDataSource = new Mock<EndpointDataSource>();

        mockEndpointRouteBuilder
            .Setup(x => x.ServiceProvider)
            .Returns(mockServiceProvider.Object);

        mockEndpointRouteBuilder
            .Setup(x => x.DataSources)
            .Returns(new System.Collections.Generic.List<EndpointDataSource> { mockEndpointDataSource.Object });

        var exception = Record.Exception(() => endpoint.MapEndpoint(mockEndpointRouteBuilder.Object));

        Assert.Null(exception);
    }
}
