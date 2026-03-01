namespace Shared.Infrastructure.Configuration.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenExpirationHours { get; set; } = 1;

    public int RefreshTokenExpirationDays { get; set; } = 30;
}
