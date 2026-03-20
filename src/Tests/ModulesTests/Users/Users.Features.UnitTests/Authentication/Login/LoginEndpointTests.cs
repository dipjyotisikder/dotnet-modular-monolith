using Microsoft.AspNetCore.Routing;
using Users.Features.Authentication.Login;

namespace Users.Features.UnitTests.Authentication.Login;

/// <summary>
/// Unit tests for the <see cref="LoginEndpoint"/> class.
/// </summary>
public class LoginEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new LoginEndpoint();
        IEndpointRouteBuilder app = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app));
    }

}