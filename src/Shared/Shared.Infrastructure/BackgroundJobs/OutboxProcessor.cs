using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Domain.Services;
using Shared.Infrastructure.Configuration.Options;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Persistence.Locks;

namespace Shared.Infrastructure.BackgroundJobs;

public class OutboxProcessor(
    IServiceProvider serviceProvider,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> outboxOptions,
    ISystemClock clock) : BackgroundService
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;
    private const string OUTBOX_PROCESSING_LOCK = "outbox-processing-lock";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(OutboxProcessorLogs.Started.ProcessorStarted,
            _outboxOptions.MaxRetries,
            _outboxOptions.BatchSize,
            _outboxOptions.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, OutboxProcessorLogs.Errors.ProcessingException);
            }

            await Task.Delay(TimeSpan.FromSeconds(_outboxOptions.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var outboxDb = scope.ServiceProvider.GetRequiredService<OutboxDbContext>();
        var locksDb = scope.ServiceProvider.GetRequiredService<DistributedLocksDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var lockService = new DatabaseDistributedLock(locksDb, clock, OUTBOX_PROCESSING_LOCK);

        if (!await lockService.TryAcquireAsync(TimeSpan.FromSeconds(5), ct))
        {
            logger.LogDebug(OutboxProcessorLogs.Warnings.PublishFailed, "Lock not acquired", 0, _outboxOptions.MaxRetries);
            return;
        }

        try
        {
            var pending = await outboxDb.OutboxMessages
                .Where(m => m.ShouldRetry(_outboxOptions.MaxRetries))
                .OrderBy(m => m.CreatedAt)
                .Take(_outboxOptions.BatchSize)
                .ToListAsync(ct);

            if (pending.Count == 0)
                return;

            foreach (var msg in pending)
            {
                try
                {
                    await publisher.PublishAsync(msg.EventType, msg.Payload, msg.IdempotencyKey);
                    msg.MarkAsSent(clock);
                    logger.LogInformation(OutboxProcessorLogs.Success.MessagePublished, msg.Id, msg.EventType);
                }
                catch (Exception ex)
                {
                    msg.RecordFailure(ex.Message);

                    if (msg.RetryCount >= _outboxOptions.MaxRetries)
                    {
                        msg.MoveToDeadLetterQueue(clock, $"Max retries ({_outboxOptions.MaxRetries}) exceeded");
                        await publisher.PublishToDeadLetterQueueAsync(msg.EventType, msg.Payload, ex.Message);
                        logger.LogError("Message {MessageId} moved to DLQ after {RetryCount} retries", msg.Id, msg.RetryCount);
                    }

                    logger.LogWarning(ex, OutboxProcessorLogs.Warnings.PublishFailed, msg.Id, msg.RetryCount, _outboxOptions.MaxRetries);
                }
            }

            await outboxDb.SaveChangesAsync(ct);
        }
        finally
        {
            await lockService.ReleaseAsync(ct);
            await lockService.DisposeAsync();
        }
    }
}




