using AppHost.Infrastructure.Seeding;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Domain.Seeding;
using Shared.Infrastructure.Seeding;

namespace AppHost.UnitTests.Infrastructure.Seeding;

public class SeedingExtensionsTests
{
    [Fact]
    public async Task SeedAsync_WhenSeedingSucceeds_CompletesSuccessfully()
    {
        var mockLogger = new Mock<ILogger<SeederRunner>>();
        var mockSeederRunner = new SeederRunner(Array.Empty<ISeeder>(), mockLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(mockSeederRunner);
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();

        await app.SeedAsync();
    }

    [Fact]
    public async Task SeedAsync_WhenSeedAllAsyncThrows_LogsErrorAndRethrowsException()
    {
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
        Assert.Same(expectedException, exception);
        mockProgramLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding failed")), expectedException, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WhenSeederRunnerNotRegistered_ThrowsInvalidOperationException()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
    }

    [Fact]
    public async Task SeedAsync_WhenLoggerNotRegisteredAndSeedingFails_ThrowsOriginalException()
    {
        var expectedException = new InvalidOperationException("Seeding error");
        var mockSeeder = new Mock<ISeeder>();
        mockSeeder.Setup(s => s.Priority).Returns(1);
        mockSeeder.Setup(s => s.Name).Returns("TestSeeder");
        mockSeeder.Setup(s => s.SeedAsync(default)).ThrowsAsync(expectedException);
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(new[] { mockSeeder.Object }, mockSeederLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        var app = builder.Build();

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
    }

    [Fact]
    public async Task SeedAsync_WithMultipleSeeders_ExecutesInPriorityOrderAndFailsOnFirstError()
    {
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

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
        Assert.Same(expectedException, exception);
        mockProgramLogger.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Seeding failed")), expectedException, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WithNoSeeders_CompletesSuccessfully()
    {
        var mockSeederLogger = new Mock<ILogger<SeederRunner>>();
        var seederRunner = new SeederRunner(Array.Empty<ISeeder>(), mockSeederLogger.Object);
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.Services.AddSingleton(seederRunner);
        builder.Services.AddSingleton<ILogger<Program>>(Mock.Of<ILogger<Program>>());
        var app = builder.Build();

        await app.SeedAsync();
    }

    [Fact]
    public async Task SeedAsync_WhenExceptionOccurs_DisposesServiceScope()
    {
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

        await Assert.ThrowsAsync<InvalidOperationException>(() => app.SeedAsync());
    }
}
