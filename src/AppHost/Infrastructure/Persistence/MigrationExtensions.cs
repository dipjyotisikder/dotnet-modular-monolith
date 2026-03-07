using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Persistence;
using Users.Infrastructure.Persistence;
using Bookings.Infrastructure.Persistence;

namespace AppHost.Infrastructure.Persistence;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            await services.GetRequiredService<OutboxDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<DistributedLocksDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
            await services.GetRequiredService<BookingsDbContext>().Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Database migration failed.", ex);
        }
    }
}
