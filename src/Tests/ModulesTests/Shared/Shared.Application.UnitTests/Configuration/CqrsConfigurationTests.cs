using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Configuration;
using System.Reflection;

namespace Shared.Application.UnitTests.Configuration;

public class CqrsConfigurationTests
{
    [Fact]
    public void RegisterCqrsHandlers_ValidAssembly_RegistersHandlersAndReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var assembly = typeof(CqrsConfiguration).Assembly;

        var result = services.RegisterCqrsHandlers(assembly);

        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Theory]
    [InlineData(typeof(CqrsConfiguration))]
    [InlineData(typeof(ServiceCollection))]
    [InlineData(typeof(Assembly))]
    public void RegisterCqrsHandlers_DifferentValidAssemblies_RegistersHandlersSuccessfully(Type typeFromAssembly)
    {
        var services = new ServiceCollection();
        var assembly = typeFromAssembly.Assembly;

        var result = services.RegisterCqrsHandlers(assembly);

        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    [Fact]
    public void RegisterCqrsHandlers_ValidAssembly_AddsServicesToCollection()
    {
        var services = new ServiceCollection();
        var assembly = typeof(CqrsConfiguration).Assembly;
        var initialCount = services.Count;

        services.RegisterCqrsHandlers(assembly);

        Assert.True(services.Count > initialCount);
    }
}
