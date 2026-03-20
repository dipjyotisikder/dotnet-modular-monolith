using Moq;


namespace Users.Features.UnitTests.DeviceManagement.RevokeAllDevices;

/// <summary>
/// Unit tests for <see cref="RevokeAllDevicesEndpoint"/>.
/// </summary>
public class RevokeAllDevicesEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint does not throw an exception when provided with a valid IEndpointRouteBuilder.
    /// This verifies the basic execution path of the endpoint configuration.
    /// Expected result: Method completes without throwing an exception.
    /// </summary>
    /// <remarks>
    /// Note: Due to the use of static extension methods (MapGroup, MapPost, WithName, etc.) which cannot be mocked,
    /// this test provides limited verification. Full verification of the endpoint configuration would require
    /// integration testing with a real endpoint route builder.
    /// </remarks>
    [Fact]
    public void MapEndpoint_ValidEndpointRouteBuilder_DoesNotThrow()
    {
        // Arrange
        var mockRouteBuilder = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockRouteBuilder.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        mockRouteBuilder.Setup(x => x.DataSources).Returns(new System.Collections.Generic.List<EndpointDataSource>());

        var endpoint = new RevokeAllDevicesEndpoint();

        // Act & Assert
        var exception = Record.Exception(() => endpoint.MapEndpoint(mockRouteBuilder.Object));

        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when provided with a null IEndpointRouteBuilder.
    /// This verifies that the underlying ASP.NET Core framework performs null checking on the parameter.
    /// Expected result: ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullEndpointRouteBuilder_ThrowsNullReferenceException()
    {
        // Arrange
        var endpoint = new RevokeAllDevicesEndpoint();
        IEndpointRouteBuilder? nullRouteBuilder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullRouteBuilder!));
    }
}