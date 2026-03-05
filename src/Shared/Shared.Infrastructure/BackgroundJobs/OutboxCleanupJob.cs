using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Domain.Services;
using Shared.Infrastructure.Configuration.Options;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.BackgroundJobs;

public class OutboxCleanupJob(
    IServiceProvider serviceProvider,
    ILogger<OutboxCleanupJob> logger,
    IOptions<OutboxOptions> outboxOptions,
    ISystemClock clock) : BackgroundService
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(OutboxCleanupJobLogs.Started.CleanupJobStarted,
            _outboxOptions.RetentionDays, _outboxOptions.CleanupIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldMessagesAsync(_outboxOptions.RetentionDays, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, OutboxCleanupJobLogs.Errors.CleanupException);
            }

            await Task.Delay(TimeSpan.FromHours(_outboxOptions.CleanupIntervalHours), stoppingToken);
        }
    }

    private async Task CleanupOldMessagesAsync(int retentionDays, CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();

        var cutoffDate = clock.UtcNow.AddDays(-retentionDays);

        const int batchSize = 1000;
        int deletedCount = 0;

        while (true)
        {
            var oldMessages = await db.OutboxMessages
                .Where(m => m.Sent && m.ProcessedAt < cutoffDate)
                .Take(batchSize)
                .ToListAsync(ct);

            if (oldMessages.Count == 0) break;

            db.OutboxMessages.RemoveRange(oldMessages);
            await db.SaveChangesAsync(ct);

            deletedCount += oldMessages.Count;

            if (oldMessages.Count < batchSize) break;

            // SMALL DELAY BETWEEN BATCHES TO REDUCE DATABASE LOAD
            await Task.Delay(100, ct);
        }

        if (deletedCount > 0)
        {
            logger.LogInformation(OutboxCleanupJobLogs.Success.MessagesCleanedUp, deletedCount);
        }
    }
}
