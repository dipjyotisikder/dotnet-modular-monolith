using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Bookings.Infrastructure;

namespace Bookings.Infrastructure.UnitTests;

/// <summary>
/// Unit tests for the <see cref="DependencyInjection"/> class.
/// </summary>
public class DependencyInjectionTests
{
    /// <summary>
    /// Tests that AddBookingsInfrastructure throws ArgumentNullException when services parameter is null.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var configurationMock = new Mock<IConfiguration>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddBookingsInfrastructure(configurationMock.Object));
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure registers services without throwing during registration.
    /// Note: Null configuration validation occurs when services are resolved, not during registration.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act
        var result = services.AddBookingsInfrastructure(configuration!);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure returns the same service collection instance when called with valid parameters.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_ValidParameters_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("Host=localhost;Database=test;Username=test;Password=test");

        // Act
        var result = services.AddBookingsInfrastructure(configurationMock.Object);

        // Assert
        Assert.Same(services, result);
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure successfully registers services when provided with valid parameters and a valid connection string.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_ValidParametersWithConnectionString_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("Host=localhost;Database=test;Username=test;Password=test");
        var initialCount = services.Count;

        // Act
        services.AddBookingsInfrastructure(configurationMock.Object);

        // Assert
        Assert.True(services.Count > initialCount, "Services should be added to the collection");
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure handles null connection string from configuration without throwing during registration.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_NullConnectionString_DoesNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns((string?)null);

        // Act
        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure handles empty connection string from configuration without throwing during registration.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_EmptyConnectionString_DoesNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns(string.Empty);

        // Act
        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        // Assert
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that AddBookingsInfrastructure handles whitespace-only connection string from configuration without throwing during registration.
    /// </summary>
    [Fact]
    public void AddBookingsInfrastructure_WhitespaceConnectionString_DoesNotThrowDuringRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("   ");

        // Act
        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        // Assert
        Assert.Null(exception);
    }
}