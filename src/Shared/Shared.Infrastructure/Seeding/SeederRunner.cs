using Microsoft.Extensions.Logging;
using Shared.Domain.Seeding;

namespace Shared.Infrastructure.Seeding;

public class SeederRunner
{
    private readonly IEnumerable<ISeeder> _seeders;
    private readonly ILogger<SeederRunner> _logger;

    public SeederRunner(IEnumerable<ISeeder> seeders, ILogger<SeederRunner> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        var orderedSeeders = _seeders.OrderBy(s => s.Priority).ToList();

        if (!orderedSeeders.Any())
            return;

        foreach (var seeder in orderedSeeders)
        {
            try
            {
                _logger.LogInformation("Seeding: {SeederName}", seeder.Name);
                await seeder.SeedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seeding failed: {SeederName}", seeder.Name);
                throw;
            }
        }
    }
}
