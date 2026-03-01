namespace Shared.Infrastructure.Configuration.Options;

public class OAuthOptions
{
    public const string SectionName = "Authentication";

    public GoogleOAuthOptions Google { get; set; } = new();
    public MicrosoftOAuthOptions Microsoft { get; set; } = new();
    public GitHubOAuthOptions GitHub { get; set; } = new();
}

public class GoogleOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

public class MicrosoftOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

public class GitHubOAuthOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}
