namespace Shared.Infrastructure.Configuration.Options.Validation;

public static class ValidationMessages
{
    public static class Jwt
    {
        public const string SecretKeyRequired = "JwtSecretKeyIsRequired";
        public const string SecretKeyMinLength = "JwtSecretKeyMustBeAtLeast32Characters";
        public const string IssuerRequired = "JwtIssuerIsRequired";
        public const string AudienceRequired = "JwtAudienceIsRequired";
        public const string AccessTokenExpirationHours = "AccessTokenExpirationMustBeBetween1And24Hours";
        public const string RefreshTokenExpirationDays = "RefreshTokenExpirationMustBeBetween1And90Days";
    }

    public static class Cors
    {
        public const string AllowedOriginsRequired = "AtLeastOneAllowedOriginIsRequired";
        public const string PreflightMaxAge = "PreflightMaxAgeMustBeBetween1And1440Minutes";
    }

    public static class OAuth
    {
        public static class Google
        {
            public const string ClientIdRequired = "GoogleClientIdIsRequiredWhenEnabled";
            public const string ClientSecretRequired = "GoogleClientSecretIsRequiredWhenEnabled";
        }
    }

    public static class Outbox
    {
        public const string MaxRetries = "MaxRetriesMustBeBetween1And20";
        public const string BatchSize = "BatchSizeMustBeBetween1And1000";
        public const string PollingIntervalSeconds = "PollingIntervalMustBeBetween1And60Seconds";
        public const string RetentionDays = "RetentionDaysMustBeBetween1And90";
        public const string CleanupIntervalHours = "CleanupIntervalMustBeBetween1And168Hours";
    }

    public static class RabbitMq
    {
        public const string HostRequired = "RabbitMqHostIsRequiredWhenEnabled";
        public const string UsernameRequired = "RabbitMqUsernameIsRequiredWhenEnabled";
        public const string PasswordRequired = "RabbitMqPasswordIsRequiredWhenEnabled";
        public const string ExchangeRequired = "RabbitMqExchangeIsRequiredWhenEnabled";
        public const string Port = "RabbitMqPortMustBeBetween1And65535";
    }
}
