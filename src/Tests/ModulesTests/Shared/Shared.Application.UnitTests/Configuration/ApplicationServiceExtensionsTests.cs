using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviors;
using Shared.Application.Configuration;


namespace Shared.Application.UnitTests.Configuration;

public class ApplicationServiceExtensionsTests
{
    /// <summary>
    /// Tests that AddApplicationServices throws ArgumentNullException when services parameter is null.
    /// </summary>
    [Fact]
    public void AddApplicationServices_NullServices_ThrowsNullReferenceException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddApplicationServices());
    }

    /// <summary>
    /// Tests that AddApplicationServices successfully registers both LoggingBehavior and ValidationBehavior
    /// as transient services and returns the service collection.
    /// </summary>
    [Fact]
    public void AddApplicationServices_ValidServices_RegistersBehaviorsAndReturnsServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.Same(services, result);
        Assert.Equal(2, services.Count);

        var loggingDescriptor = services[0];
        Assert.Equal(typeof(IPipelineBehavior<,>), loggingDescriptor.ServiceType);
        Assert.Equal(typeof(LoggingBehavior<,>), loggingDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, loggingDescriptor.Lifetime);

        var validationDescriptor = services[1];
        Assert.Equal(typeof(IPipelineBehavior<,>), validationDescriptor.ServiceType);
        Assert.Equal(typeof(ValidationBehavior<,>), validationDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, validationDescriptor.Lifetime);
    }

    /// <summary>
    /// Tests that AddApplicationServices can be called multiple times on the same service collection
    /// and each call adds the behaviors again.
    /// </summary>
    [Fact]
    public void AddApplicationServices_CalledMultipleTimes_RegistersBehaviorsMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddApplicationServices();
        services.AddApplicationServices();

        // Assert
        Assert.Equal(4, services.Count);
    }

    /// <summary>
    /// Tests that AddApplicationServices correctly registers behaviors when service collection
    /// already contains other services.
    /// </summary>
    [Fact]
    public void AddApplicationServices_ExistingServices_AppendsNewBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<string>("existing");

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.Same(services, result);
        Assert.Equal(3, services.Count);
        Assert.Equal(typeof(string), services[0].ServiceType);
        Assert.Equal(typeof(IPipelineBehavior<,>), services[1].ServiceType);
        Assert.Equal(typeof(IPipelineBehavior<,>), services[2].ServiceType);
    }
}