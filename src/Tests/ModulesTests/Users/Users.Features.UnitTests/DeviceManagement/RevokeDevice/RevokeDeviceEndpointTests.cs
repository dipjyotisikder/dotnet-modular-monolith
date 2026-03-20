using Moq;


namespace Users.Features.UnitTests.DeviceManagement.RevokeDevice;

/// <summary>
/// Unit tests for the <see cref="RevokeDeviceEndpoint"/> class.
/// </summary>
public class RevokeDeviceEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// This verifies proper null parameter validation.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new RevokeDeviceEndpoint();
        IEndpointRouteBuilder? app = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app!));
    }

    /// <summary>
    /// Tests that MapEndpoint executes without throwing when provided a valid IEndpointRouteBuilder.
    /// Note: This test verifies basic execution flow. Full endpoint configuration verification
    /// requires integration testing with the ASP.NET Core framework, as MapGroup, MapPost, and
    /// other fluent API methods are extension methods that operate on concrete framework types
    /// and cannot be effectively mocked in isolation.
    /// </summary>
    [Fact]
    public void MapEndpoint_ValidApp_ExecutesWithoutThrowing()
    {
        // Arrange
        var endpoint = new RevokeDeviceEndpoint();
        var mockApp = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDataSources = new List<EndpointDataSource>();

        // Setup required properties that extension methods may access
        mockApp.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockApp.Setup(x => x.DataSources).Returns(mockDataSources);
        mockApp.Setup(x => x.CreateApplicationBuilder()).Returns(new Mock<IApplicationBuilder>().Object);

        // Act
        var exception = Record.Exception(() => endpoint.MapEndpoint(mockApp.Object));

        // Assert
        // Verify the method can be called without throwing exceptions
        Assert.Null(exception);
    }
}