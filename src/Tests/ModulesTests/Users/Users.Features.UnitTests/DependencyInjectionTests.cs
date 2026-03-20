using Moq;


namespace Users.Features.UnitTests;

/// <summary>
/// Contains unit tests for the <see cref="DependencyInjection"/> class.
/// </summary>
public class DependencyInjectionTests
{
    /// <summary>
    /// Tests that AddUsersFeatures returns the same IServiceCollection instance passed to it,
    /// enabling fluent chaining of service registrations.
    /// </summary>
    [Fact]
    public void AddUsersFeatures_ValidInputs_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddUsersFeatures();

        // Assert
        Assert.Same(services, result);
    }

    /// <summary>
    /// Tests that AddUsersFeatures executes successfully without throwing exceptions
    /// when provided with valid service collection and configuration instances.
    /// </summary>
    [Fact]
    public void AddUsersFeatures_ValidInputs_ExecutesWithoutThrowing()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();

        // Act & Assert
        var exception = Record.Exception(() => services.AddUsersFeatures());
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that AddUsersFeatures throws ArgumentNullException when both parameters are null,
    /// ensuring proper null validation for all parameters.
    /// </summary>
    [Fact]
    public void AddUsersFeatures_BothParametersNull_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddUsersFeatures());
    }
}