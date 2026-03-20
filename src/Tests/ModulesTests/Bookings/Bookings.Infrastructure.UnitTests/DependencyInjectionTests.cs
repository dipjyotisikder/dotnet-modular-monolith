using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bookings.Infrastructure.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddBookingsInfrastructure_NullServices_ThrowsArgumentNullException()
    {
        IServiceCollection? services = null;
        var configurationMock = new Mock<IConfiguration>();

        Assert.Throws<ArgumentNullException>(() => services!.AddBookingsInfrastructure(configurationMock.Object));
    }

    [Fact]
    public void AddBookingsInfrastructure_NullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        var result = services.AddBookingsInfrastructure(configuration!);

        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void AddBookingsInfrastructure_ValidParameters_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("Host=localhost;Database=test;Username=test;Password=test");

        var result = services.AddBookingsInfrastructure(configurationMock.Object);

        Assert.Same(services, result);
    }

    [Fact]
    public void AddBookingsInfrastructure_ValidParametersWithConnectionString_RegistersServices()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("Host=localhost;Database=test;Username=test;Password=test");
        var initialCount = services.Count;

        services.AddBookingsInfrastructure(configurationMock.Object);

        Assert.True(services.Count > initialCount, "Services should be added to the collection");
    }

    [Fact]
    public void AddBookingsInfrastructure_NullConnectionString_DoesNotThrowDuringRegistration()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns((string?)null);

        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        Assert.Null(exception);
    }

    [Fact]
    public void AddBookingsInfrastructure_EmptyConnectionString_DoesNotThrowDuringRegistration()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns(string.Empty);

        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        Assert.Null(exception);
    }

    [Fact]
    public void AddBookingsInfrastructure_WhitespaceConnectionString_DoesNotThrowDuringRegistration()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c.GetSection("ConnectionStrings")["DefaultConnection"])
            .Returns("   ");

        var exception = Record.Exception(() => services.AddBookingsInfrastructure(configurationMock.Object));

        Assert.Null(exception);
    }
}
