using Microsoft.Extensions.Logging;
using Shared.Domain.Seeding;

namespace Shared.Infrastructure.Seeding;

public class SeederRunner(IEnumerable<ISeeder> seeders, ILogger<SeederRunner> logger)
{
    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        var orderedSeeders = seeders.OrderBy(s => s.Priority).ToList();
        if (orderedSeeders.Count == 0)
        {
            return;
        }

        foreach (var seeder in orderedSeeders)
        {
            try
            {
                logger.LogInformation("Seeding: {SeederName}", seeder.Name);
                await seeder.SeedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Seeding failed: {SeederName}", seeder.Name);
                throw;
            }
        }
    }
}
