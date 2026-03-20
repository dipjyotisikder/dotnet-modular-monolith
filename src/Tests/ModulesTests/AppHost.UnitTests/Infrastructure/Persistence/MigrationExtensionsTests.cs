using AppHost.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Persistence;
using Users.Infrastructure.Persistence;

namespace AppHost.UnitTests.Infrastructure.Persistence;

public class MigrationExtensionsTests
{
    [Fact]
    public async Task ApplyMigrationsAsync_OutboxDbContextMigrationFails_ThrowsInvalidOperationException()
    {
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Migration failed");

        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    [Fact]
    public async Task ApplyMigrationsAsync_UsersDbContextMigrationFails_ThrowsInvalidOperationException()
    {
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("UsersDbContext migration failed");

        builder.Services.AddDbContext<OutboxDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
        builder.Services.AddDbContext<DistributedLocksDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        builder.Services.AddDbContext<Bookings.Infrastructure.Persistence.BookingsDbContext>(options =>
            options.UseSqlite("DataSource=:memory:")
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        builder.Services.AddScoped<UsersDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    [Fact]
    public async Task ApplyMigrationsAsync_ServiceResolutionFails_ThrowsInvalidOperationException()
    {
        var builder = WebApplication.CreateBuilder();
        await using var app = builder.Build();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task ApplyMigrationsAsync_MigrationFails_WrapsExceptionWithCorrectMessage()
    {
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Test migration failure");

        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }

    [Fact]
    public async Task ApplyMigrationsAsync_ExceptionOccurs_DisposesServiceScope()
    {
        var builder = WebApplication.CreateBuilder();

        var expectedInnerException = new InvalidOperationException("Migration failed");

        builder.Services.AddScoped<OutboxDbContext>(_ => throw expectedInnerException);

        await using var app = builder.Build();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => app.ApplyMigrationsAsync()
        );

        Assert.Equal("Database migration failed.", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Same(expectedInnerException, exception.InnerException);
    }
}
