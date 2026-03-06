namespace Users.Domain.ValueObjects;

public class TokenRevocationVersion
{
    public int AccessTokenVersion { get; private set; } = 1;
    public int RefreshTokenVersion { get; private set; } = 1;
    public DateTime LastRevokedAt { get; private set; }

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
