using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Configuration;

namespace Shared.Application.UnitTests.Configuration;

public class ApplicationServiceExtensionsTests
{
    [Fact]
    public void AddApplicationServices_NullServices_ThrowsNullReferenceException()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddApplicationServices());
    }

    [Fact]
    public void AddApplicationServices_ValidServices_RegistersBehaviorsAndReturnsServices()
    {
        var services = new ServiceCollection();

        var result = services.AddApplicationServices();

        Assert.Same(services, result);
        Assert.Equal(3, services.Count);

        var loggingDescriptor = services[0];
        Assert.Equal(typeof(IPipelineBehavior<,>), loggingDescriptor.ServiceType);
        Assert.Equal(typeof(LoggingBehavior<,>), loggingDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, loggingDescriptor.Lifetime);

        var validationDescriptor = services[1];
        Assert.Equal(typeof(IPipelineBehavior<,>), validationDescriptor.ServiceType);
        Assert.Equal(typeof(ValidationBehavior<,>), validationDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, validationDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_CalledMultipleTimes_RegistersBehaviorsMultipleTimes()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();
        services.AddApplicationServices();

        Assert.Equal(6, services.Count);
    }

    [Fact]
    public void AddApplicationServices_ExistingServices_AppendsNewBehaviors()
    {
        var services = new ServiceCollection();
        services.AddSingleton<string>("existing");

        var result = services.AddApplicationServices();

        Assert.Same(services, result);
        Assert.Equal(4, services.Count);
        Assert.Equal(typeof(string), services[0].ServiceType);
        Assert.Equal(typeof(IPipelineBehavior<,>), services[1].ServiceType);
        Assert.Equal(typeof(IPipelineBehavior<,>), services[2].ServiceType);
    }
}
