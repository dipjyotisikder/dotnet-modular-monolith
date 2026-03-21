namespace Shared.Infrastructure.Configuration.Options;

public class OAuthOptions
{
    public const string SectionName = "Authentication";
    public GoogleOAuthOptions Google { get; set; } = new();
}

public class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public ClaimsMappingOptions ClaimsMapping { get; set; } = new();
}

public class ClaimsMappingOptions
{
    public string UserIdClaim { get; set; } = "sub";
    public string EmailClaim { get; set; } = "email";
    public string RoleClaim { get; set; } = "role";
    public string TierClaim { get; set; } = "tier";
    public Dictionary<string, string> RoleMapping { get; set; } = new()
    {
        { "admin", "admin" },
        { "user", "user" },
        { "guest", "guest" }
    };
    public string DefaultRole { get; set; } = "user";
}
