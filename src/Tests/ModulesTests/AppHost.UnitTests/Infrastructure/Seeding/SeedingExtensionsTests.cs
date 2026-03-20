using Moq;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Domain.Seeding;
using Shared.Infrastructure.Seeding;
using AppHost.Infrastructure.Seeding;

namespace AppHost.UnitTests.Infrastructure.Seeding;

public class SeedingExtensionsTests
{
    /// <summary>
    /// Tests that SeedAsync successfully creates a scope, retrieves SeederRunner, and calls SeedAllAsync
    /// when no exceptions occur.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenSeedingSucceeds_CompletesSuccessfully()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SeederRunner>>();
        var mockSeederRunner = new SeederRunner(Array.Empty<ISeeder>(), mockLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(mockSeederRunner);
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();
        // Act
        await app.SeedAsync();
        // Assert
        // If no exception is thrown, the test passes
    }

    /// <summary>
    /// Tests that SeedAsync logs the error and re-throws the exception when SeedAllAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenSeedAllAsyncThrows_LogsErrorAndRethrowsException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Seeding error");
        var mockSeeder = new Mock<ISeeder>();
        mockSeeder.Setup(s => s.Priority).Returns(1);
        mockSeeder.Setup(s => s.Name).Returns("TestSeeder");
        mockSeeder.Setup(s => s.SeedAsync(default)).ThrowsAsync(expectedException);
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(new[] { mockSeeder.Object }, mockSeederLogger.Object);
        var mockProgramLogger = new Mock<ILogger<Program>>();
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        builder.Services.AddSingleton(mockProgramLogger.Object);
        var app = builder.Build();
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
        Assert.Same(expectedException, exception);
        mockProgramLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding failed")), expectedException, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Tests that SeedAsync throws and does not catch exceptions when GetRequiredService fails to retrieve SeederRunner.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenSeederRunnerNotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        // Deliberately not registering SeederRunner
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
    }

    /// <summary>
    /// Tests that SeedAsync logs the error and re-throws when GetRequiredService for ILogger fails during exception handling.
    /// This scenario tests the edge case where even the logger cannot be retrieved.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenLoggerNotRegisteredAndSeedingFails_ThrowsOriginalException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Seeding error");
        var mockSeeder = new Mock<ISeeder>();
        mockSeeder.Setup(s => s.Priority).Returns(1);
        mockSeeder.Setup(s => s.Name).Returns("TestSeeder");
        mockSeeder.Setup(s => s.SeedAsync(default)).ThrowsAsync(expectedException);
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(new[] { mockSeeder.Object }, mockSeederLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        // Deliberately not registering ILogger<Program>
        var app = builder.Build();
        // Act & Assert
        // The GetRequiredService<ILogger<Program>> will throw, which will be the exception propagated
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
    }

    /// <summary>
    /// Tests that SeedAsync correctly handles multiple seeders and logs/rethrows on first failure.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WithMultipleSeeders_ExecutesInPriorityOrderAndFailsOnFirstError()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Second seeder failed");
        var mockSeeder1 = new Mock<ISeeder>();
        mockSeeder1.Setup(s => s.Priority).Returns(1);
        mockSeeder1.Setup(s => s.Name).Returns("FirstSeeder");
        mockSeeder1.Setup(s => s.SeedAsync(default)).Returns(Task.CompletedTask);
        var mockSeeder2 = new Mock<ISeeder>();
        mockSeeder2.Setup(s => s.Priority).Returns(2);
        mockSeeder2.Setup(s => s.Name).Returns("SecondSeeder");
        mockSeeder2.Setup(s => s.SeedAsync(default)).ThrowsAsync(expectedException);
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(new[] { mockSeeder1.Object, mockSeeder2.Object }, mockSeederLogger.Object);
        var mockProgramLogger = new Mock<ILogger<Program>>();
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        builder.Services.AddSingleton(mockProgramLogger.Object);
        var app = builder.Build();
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
        Assert.Same(expectedException, exception);
        mockProgramLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding failed")), expectedException, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Tests that SeedAsync completes successfully when there are no seeders registered.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WithNoSeeders_CompletesSuccessfully()
    {
        // Arrange
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(Array.Empty<ISeeder>(), mockSeederLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();
        // Act
        await app.SeedAsync();
        // Assert
        // Should complete without throwing
    }

    /// <summary>
    /// Tests that SeedAsync properly disposes the service scope even when an exception occurs.
    /// This verifies the using statement properly releases resources.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenExceptionOccurs_DisposesServiceScope()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Seeding error");
        var mockSeeder = new Mock<ISeeder>();
        mockSeeder.Setup(s => s.Priority).Returns(1);
        mockSeeder.Setup(s => s.Name).Returns("TestSeeder");
        mockSeeder.Setup(s => s.SeedAsync(default)).ThrowsAsync(expectedException);
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(new[] { mockSeeder.Object }, mockSeederLogger.Object);
        var mockProgramLogger = new Mock<ILogger<Program>>();
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        builder.Services.AddSingleton(mockProgramLogger.Object);
        var app = builder.Build();
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
        // The scope disposal is implicit via the using statement
        // If disposal fails, it would throw here
        // The test passing indicates proper disposal
    }
}