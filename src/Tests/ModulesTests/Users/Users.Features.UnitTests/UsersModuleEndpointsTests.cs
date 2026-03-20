using Microsoft.AspNetCore.Routing;
using Moq;


namespace Users.Features.UnitTests;

public class UsersModuleEndpointsTests
{
    [Fact]
    public void MapUsersEndpoints_NullApp_ThrowsArgumentNullException()
    {
        IEndpointRouteBuilder app = null!;

        Assert.Throws<ArgumentNullException>(() => UsersModuleEndpoints.MapUsersEndpoints(app));
    }

    [Fact]
    public void MapUsersEndpoints_ValidApp_ExecutesSuccessfully()
    {
        var mockApp = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var endpointDataSources = new List<Microsoft.AspNetCore.Routing.EndpointDataSource>();

        mockApp.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockApp.Setup(x => x.CreateApplicationBuilder()).Returns(new Mock<Microsoft.AspNetCore.Builder.IApplicationBuilder>().Object);
        mockApp.Setup(x => x.DataSources).Returns(endpointDataSources);

        var exception = Record.Exception(() => UsersModuleEndpoints.MapUsersEndpoints(mockApp.Object));

        Assert.Null(exception);
    }
}
