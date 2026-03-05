namespace Shared.Infrastructure.BackgroundJobs;

public static class OutboxCleanupJobLogs
{
    public static class Started
    {
        public const string CleanupJobStarted = "Outbox Cleanup Job Started - Retention: {RetentionDays} Days, Interval: {CleanupIntervalHours} Hours";
    }

    public static class Success
    {
        public const string MessagesCleanedUp = "Cleaned Up {Count} Old Outbox Messages";
    }

    public static class Errors
    {
        public const string CleanupException = "Error Cleaning Up Outbox Messages";
    }
}
