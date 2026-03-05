namespace Shared.Infrastructure.BackgroundJobs;

public static class OutboxProcessorLogs
{
    public static class Started
    {
        public const string ProcessorStarted = "Outbox Processor Started - MaxRetries: {MaxRetries}, BatchSize: {BatchSize}, PollingInterval: {PollingInterval}s";
    }

    public static class Success
    {
        public const string MessagePublished = "Published Message {MessageId} Of Type {EventType}";
    }

    public static class Warnings
    {
        public const string PublishFailed = "Failed To Publish Message {MessageId}. Retry {RetryCount}/{MaxRetries}";
    }

    public static class Errors
    {
        public const string ProcessingException = "Error Processing Outbox Messages";
    }
}
