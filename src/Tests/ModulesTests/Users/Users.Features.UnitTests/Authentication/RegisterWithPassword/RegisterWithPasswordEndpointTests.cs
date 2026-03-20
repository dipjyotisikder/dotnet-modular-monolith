namespace Users.Features.UnitTests.Authentication.RegisterWithPassword;

/// <summary>
/// Unit tests for <see cref="RegisterWithPasswordEndpoint"/>.
/// </summary>
public class RegisterWithPasswordEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new RegisterWithPasswordEndpoint();
        IEndpointRouteBuilder app = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
        Assert.Equal("endpoints", exception.ParamName);
    }

}