namespace Shared.Infrastructure.Configuration.Options;

public class OutboxOptions
{
    public const string SectionName = "Outbox";
    public bool Enabled { get; set; }
    public int MaxRetries { get; set; } = 5;
    public int CleanupDays { get; set; } = 7;
    public int BatchSize { get; set; } = 50;
    public int RetentionDays { get; set; } = 7;
    public int CleanupIntervalHours { get; set; } = 24;
    public int PollingIntervalSeconds { get; set; } = 10;
}
