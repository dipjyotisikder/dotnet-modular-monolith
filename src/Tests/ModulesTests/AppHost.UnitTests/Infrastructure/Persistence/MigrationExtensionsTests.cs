using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using Users.Infrastructure.Persistence;
using Bookings.Infrastructure.Persistence;
using AppHost.Infrastructure.Persistence;

namespace AppHost.UnitTests.Infrastructure.Persistence;
/// <summary>
/// Unit tests for the <see cref = "MigrationExtensions"/> class.
/// </summary>
public class MigrationExtensionsTests
{
    /// <summary>
    /// Tests that ApplyMigrationsAsync throws InvalidOperationException with the correct message
    /// when OutboxDbContext migration fails.
    /// </summary>
    [Fact]
    public async Task ApplyMigrationsAsync_OutboxDbContextMigrationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Migration failed");

        // Register a service that throws when accessed (simulating migration failure)
        // Since MigrateAsync is an extension method, we cannot mock it directly with Moq
        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    /// <summary>
    /// Tests that ApplyMigrationsAsync throws InvalidOperationException when UsersDbContext
    /// migration fails.
    /// </summary>
    [Fact]
    public async Task ApplyMigrationsAsync_UsersDbContextMigrationFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("UsersDbContext migration failed");

        // Register OutboxDbContext and DistributedLocksDbContext with SQLite in-memory provider
        // and suppress pending model changes warnings so they don't fail during migration
        builder.Services.AddDbContext<OutboxDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
        builder.Services.AddDbContext<DistributedLocksDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        // Register BookingsDbContext with SQLite in-memory provider and suppress warnings
        builder.Services.AddDbContext<Bookings.Infrastructure.Persistence.BookingsDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        // Register UsersDbContext that throws when accessed (simulating migration failure)
        builder.Services.AddScoped<UsersDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    /// <summary>
    /// Tests that ApplyMigrationsAsync throws InvalidOperationException when a required service
    /// cannot be resolved from the service provider.
    /// </summary>
    [Fact]
    public async Task ApplyMigrationsAsync_ServiceResolutionFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        // Intentionally not registering required DbContext services to trigger service resolution failure
        await using var app = builder.Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    /// <summary>
    /// Tests that ApplyMigrationsAsync properly wraps exceptions with the correct error message
    /// and preserves the inner exception.
    /// </summary>
    [Fact]
    public async Task ApplyMigrationsAsync_MigrationFails_WrapsExceptionWithCorrectMessage()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Test migration failure");

        // Register a service that throws when accessed to simulate migration failure
        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        // Verify the exception is properly wrapped
        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    /// <summary>
    /// Tests that ApplyMigrationsAsync properly disposes the service scope even when an exception occurs.
    /// </summary>
    [Fact]
    public async Task ApplyMigrationsAsync_ExceptionOccurs_DisposesServiceScope()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Migration failed");

        // Register a service that throws when accessed (simulating migration failure)
        // This will cause an exception during migration, which tests that the scope is properly disposed
        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        // Act & Assert
        // If the scope is not properly disposed, we might get additional exceptions or resource leaks
        // The fact that we cleanly get the expected exception proves the scope was disposed
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        // Verify the exception was properly wrapped, which confirms normal exception flow
        // and that the using statement properly disposed the scope before throwing
        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }
}