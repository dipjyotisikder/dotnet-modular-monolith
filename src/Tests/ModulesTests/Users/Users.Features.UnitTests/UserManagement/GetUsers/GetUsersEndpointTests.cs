using Xunit;
using Microsoft.AspNetCore.Routing;
using Users.Features.UserManagement.GetUsers;

namespace Users.Features.UnitTests.UserManagement.GetUsers;

/// <summary>
/// Unit tests for <see cref="GetUsersEndpoint"/>.
/// </summary>
public class GetUsersEndpointTests
{
    /// <summary>
    /// Tests that MapEndpoint throws ArgumentNullException when app parameter is null.
    /// This validates proper null checking for the required dependency.
    /// </summary>
    [Fact]
    public void MapEndpoint_NullApp_ThrowsArgumentNullException()
    {
        // Arrange
        var endpoint = new GetUsersEndpoint();
        IEndpointRouteBuilder? app = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => endpoint.MapEndpoint(app!));
    }

}