using Moq;


namespace Users.Features.UnitTests.UserManagement.CreateUser;

/// <summary>
/// Unit tests for the <see cref="CreateUserEndpoint"/> class.
/// </summary>
public class CreateUserEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// Input: null IEndpointRouteBuilder.
    /// Expected: ArgumentNullException is thrown.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new CreateUserEndpoint();
        IEndpointRouteBuilder app = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
    }

    /// <summary>
    /// Tests that MapEndpoint executes without throwing when provided with a valid IEndpointRouteBuilder.
    /// Input: Valid mocked IEndpointRouteBuilder with properly configured ServiceProvider and DataSources.
    /// Expected: Method executes successfully without throwing any exceptions.
    /// </summary>
    /// <remarks>
    /// Note: This test verifies basic execution flow. Due to the use of extension methods (MapGroup, MapPost, etc.)
    /// which operate on internal ASP.NET Core routing infrastructure, detailed verification of endpoint configuration
    /// (route patterns, authorization policies, tags, etc.) cannot be performed through standard unit testing with Moq.
    /// The extension methods modify internal state that is not directly observable through the IEndpointRouteBuilder interface.
    /// Integration tests would be more appropriate for verifying the complete endpoint configuration and behavior.
    /// </remarks>
    [Fact]
    public void MapEndpoint_ValidApp_ExecutesWithoutThrowing()
    {
        // Arrange
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

        // Act
        var exception = Record.Exception(() => endpoint.MapEndpoint(mockEndpointRouteBuilder.Object));

        // Assert
        Assert.Null(exception);
    }
}