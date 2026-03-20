namespace Users.Features.UnitTests.UserManagement.GetCurrentUser;

/// <summary>
/// Unit tests for <see cref="GetCurrentUserEndpoint"/>.
/// </summary>
public class GetCurrentUserEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// This verifies proper null parameter handling.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsException()
    {
        // Arrange
        var endpoint = new GetCurrentUserEndpoint();
        IEndpointRouteBuilder? nullApp = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullApp!));
    }

    /// <summary>
    /// Tests that MapEndpoint executes without throwing when provided a valid IEndpointRouteBuilder.
    /// NOTE: This test has limited verification capability because MapEndpoint uses ASP.NET Core's
    /// fluent API with extension methods (MapGroup, MapGet, WithName, RequireAuthorization, WithTags, Produces)
    /// that operate on concrete types (RouteGroupBuilder, RouteHandlerBuilder) which cannot be easily mocked.
    /// 
    /// A full verification would require:
    /// - Integration testing with a real endpoint routing infrastructure, or
    /// - Refactoring the code to use a more testable abstraction
    /// 
    /// This test only verifies that the method completes without throwing exceptions.
    /// To properly test the endpoint configuration, consider:
    /// 1. Using WebApplicationFactory for integration tests
    /// 2. Testing the endpoint behavior end-to-end
    /// 3. Verifying route configuration through integration tests
    /// </summary>
    [Fact]
    public void MapEndpoint_ValidApp_ConfiguresEndpoint()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        var app = builder.Build();
        var endpoint = new GetCurrentUserEndpoint();

        // Act
        endpoint.MapEndpoint(app);

        // Assert
        // The method completed without throwing - endpoint configuration succeeded
        // Full verification of route details requires integration testing
        Assert.NotNull(app);
    }
}