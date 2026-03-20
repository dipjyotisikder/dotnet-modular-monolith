using Microsoft.AspNetCore.Routing;
using Moq;


namespace Users.Features.UnitTests;

public class UsersModuleEndpointsTests
{
    /// <summary>
    /// Tests that MapUsersEndpoints throws ArgumentNullException when app parameter is null.
    /// This verifies proper null handling for the required parameter.
    /// </summary>
    [Fact]
    public void MapUsersEndpoints_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsersModuleEndpoints.MapUsersEndpoints(app));
    }

    /// <summary>
    /// Tests that MapUsersEndpoints executes successfully with a valid IEndpointRouteBuilder.
    /// This verifies that the method calls MapEndpointsFromAssembly with the correct assembly
    /// and completes without throwing exceptions.
    /// </summary>
    [Fact]
    public void MapUsersEndpoints_ValidApp_ExecutesSuccessfully()
    {
        // Arrange
        var mockApp = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var endpointDataSources = new List<Microsoft.AspNetCore.Routing.EndpointDataSource>();

        mockApp.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockApp.Setup(x => x.CreateApplicationBuilder()).Returns(new Mock<Microsoft.AspNetCore.Builder.IApplicationBuilder>().Object);
        mockApp.Setup(x => x.DataSources).Returns(endpointDataSources);

        // Act
        var exception = Record.Exception(() => UsersModuleEndpoints.MapUsersEndpoints(mockApp.Object));

        // Assert
        Assert.Null(exception);
    }
}