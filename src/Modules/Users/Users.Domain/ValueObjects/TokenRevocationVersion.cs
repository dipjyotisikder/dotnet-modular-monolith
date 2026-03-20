namespace Users.Domain.ValueObjects;

public class TokenRevocationVersion
{
    public TokenRevocationVersion() { }

    public int AccessTokenVersion { get; set; } = 1;
    public int RefreshTokenVersion { get; set; } = 1;
    public DateTime LastRevokedAt { get; set; }

    public void RevokeAccessTokens()
    {
        AccessTokenVersion++;
        LastRevokedAt = DateTime.UtcNow;
    }

    public void RevokeRefreshTokens()
    {
        RefreshTokenVersion++;
        LastRevokedAt = DateTime.UtcNow;
    }

    public void RevokeAllTokens()
    {
        AccessTokenVersion++;
        RefreshTokenVersion++;
        LastRevokedAt = DateTime.UtcNow;
    }
}
