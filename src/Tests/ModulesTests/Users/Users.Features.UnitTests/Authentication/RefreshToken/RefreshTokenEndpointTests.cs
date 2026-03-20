using Microsoft.AspNetCore.Routing;
using Users.Features.Authentication.RefreshToken;

namespace Users.Features.UnitTests.Authentication.RefreshToken;

/// <summary>
/// Unit tests for <see cref="RefreshTokenEndpoint"/> class.
/// </summary>
public class RefreshTokenEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when the app parameter is null.
    /// Even though the parameter is marked as non-nullable, at runtime null can still be passed,
    /// so this test verifies defensive behavior.
    /// </summary>
    [Fact]
    public void MapEndpoint_WithNullEndpointRouteBuilder_ThrowsException()
    {
        // Arrange
        var endpoint = new RefreshTokenEndpoint();
        IEndpointRouteBuilder nullApp = null!;

        // Act & Assert
        // The method will throw when trying to call extension methods on null
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(nullApp));
    }
}