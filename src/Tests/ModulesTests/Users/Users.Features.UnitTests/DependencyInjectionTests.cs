using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;


namespace Users.Features.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddUsersFeatures_ValidInputs_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddUsersFeatures();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddUsersFeatures_ValidInputs_ExecutesWithoutThrowing()
    {
        var services = new ServiceCollection();
        var configurationMock = new Mock<IConfiguration>();

        var exception = Record.Exception(() => services.AddUsersFeatures());
        Assert.Null(exception);
    }

    [Fact]
    public void AddUsersFeatures_BothParametersNull_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddUsersFeatures());
    }
}
