using Users.Domain.ValueObjects;

namespace Users.Domain.UnitTests.ValueObjects;

public class TokenRevocationVersionTests
{
    [Fact]
    public void RevokeAccessTokens_InitialCall_IncrementsAccessTokenVersionToTwo()
    {
        var tokenRevocationVersion = new TokenRevocationVersion();

        tokenRevocationVersion.RevokeAccessTokens();

        Assert.Equal(2, tokenRevocationVersion.AccessTokenVersion);
    }

    [Fact]
    public void RevokeAccessTokens_WhenCalled_SetsLastRevokedAtToCurrentUtcTime()
    {
        var tokenRevocationVersion = new TokenRevocationVersion();
        var beforeCall = DateTime.UtcNow;

        tokenRevocationVersion.RevokeAccessTokens();

        var afterCall = DateTime.UtcNow;
        Assert.True(tokenRevocationVersion.LastRevokedAt >= beforeCall);
        Assert.True(tokenRevocationVersion.LastRevokedAt <= afterCall);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(5, 6)]
    [InlineData(10, 11)]
    [InlineData(100, 101)]
    public void RevokeAccessTokens_MultipleCalls_ContinuesIncrementingAccessTokenVersion(int numberOfCalls, int expectedVersion)
    {
        var tokenRevocationVersion = new TokenRevocationVersion();

        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocationVersion.RevokeAccessTokens();
        }

        Assert.Equal(expectedVersion, tokenRevocationVersion.AccessTokenVersion);
    }

    [Fact]
    public void RevokeAccessTokens_SuccessiveCalls_UpdatesLastRevokedAtToNewerValue()
    {
        var tokenRevocationVersion = new TokenRevocationVersion();

        tokenRevocationVersion.RevokeAccessTokens();
        var firstRevocation = tokenRevocationVersion.LastRevokedAt;

        System.Threading.Thread.Sleep(10);

        tokenRevocationVersion.RevokeAccessTokens();
        var secondRevocation = tokenRevocationVersion.LastRevokedAt;

        Assert.True(secondRevocation >= firstRevocation);
    }

    [Fact]
    public void RevokeRefreshTokens_InitialCall_IncrementsRefreshTokenVersionToTwo()
    {
        var tokenRevocation = new TokenRevocationVersion();
        var initialVersion = tokenRevocation.RefreshTokenVersion;

        tokenRevocation.RevokeRefreshTokens();

        Assert.Equal(initialVersion + 1, tokenRevocation.RefreshTokenVersion);
        Assert.Equal(2, tokenRevocation.RefreshTokenVersion);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void RevokeRefreshTokens_MultipleCalls_IncrementsRefreshTokenVersionCorrectly(int numberOfCalls)
    {
        var tokenRevocation = new TokenRevocationVersion();
        var initialVersion = tokenRevocation.RefreshTokenVersion;

        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocation.RevokeRefreshTokens();
        }

        Assert.Equal(initialVersion + numberOfCalls, tokenRevocation.RefreshTokenVersion);
    }

    [Fact]
    public void RevokeRefreshTokens_Called_UpdatesLastRevokedAtToCurrentUtcTime()
    {
        var tokenRevocation = new TokenRevocationVersion();
        var timeBefore = DateTime.UtcNow;

        tokenRevocation.RevokeRefreshTokens();

        var timeAfter = DateTime.UtcNow;
        Assert.InRange(tokenRevocation.LastRevokedAt, timeBefore, timeAfter);
    }

    [Fact]
    public void RevokeRefreshTokens_CalledMultipleTimes_UpdatesLastRevokedAtEachTime()
    {
        var tokenRevocation = new TokenRevocationVersion();
        tokenRevocation.RevokeRefreshTokens();
        var firstRevokedAt = tokenRevocation.LastRevokedAt;

        System.Threading.Thread.Sleep(10);

        tokenRevocation.RevokeRefreshTokens();

        var secondRevokedAt = tokenRevocation.LastRevokedAt;
        Assert.True(secondRevokedAt > firstRevokedAt);
    }

    [Fact]
    public void RevokeRefreshTokens_Called_DoesNotChangeAccessTokenVersion()
    {
        var tokenRevocation = new TokenRevocationVersion();
        var initialAccessTokenVersion = tokenRevocation.AccessTokenVersion;

        tokenRevocation.RevokeRefreshTokens();

        Assert.Equal(initialAccessTokenVersion, tokenRevocation.AccessTokenVersion);
    }

    [Fact]
    public void RevokeRefreshTokens_ManyIncrements_HandlesLargeVersionNumbers()
    {
        var tokenRevocation = new TokenRevocationVersion();
        int numberOfCalls = 1000;

        Exception? exception = Record.Exception(() =>
        {
            for (int i = 0; i < numberOfCalls; i++)
            {
                tokenRevocation.RevokeRefreshTokens();
            }
        });

        Assert.Null(exception);
        Assert.Equal(1 + numberOfCalls, tokenRevocation.RefreshTokenVersion);
    }

    [Fact]
    public void RevokeAllTokens_InitialState_IncrementsVersionsAndSetsLastRevokedAt()
    {
        var tokenRevocation = new TokenRevocationVersion();
        var beforeRevocation = DateTime.UtcNow;

        tokenRevocation.RevokeAllTokens();

        var afterRevocation = DateTime.UtcNow;
        Assert.Equal(2, tokenRevocation.AccessTokenVersion);
        Assert.Equal(2, tokenRevocation.RefreshTokenVersion);
        Assert.InRange(tokenRevocation.LastRevokedAt, beforeRevocation, afterRevocation);
    }

    [Fact]
    public void RevokeAllTokens_MultipleInvocations_ContinuesIncrementingVersions()
    {
        var tokenRevocation = new TokenRevocationVersion();

        tokenRevocation.RevokeAllTokens();
        var firstRevocationTime = tokenRevocation.LastRevokedAt;

        System.Threading.Thread.Sleep(10);

        tokenRevocation.RevokeAllTokens();
        var secondRevocationTime = tokenRevocation.LastRevokedAt;

        tokenRevocation.RevokeAllTokens();

        Assert.Equal(4, tokenRevocation.AccessTokenVersion);
        Assert.Equal(4, tokenRevocation.RefreshTokenVersion);
        Assert.True(secondRevocationTime > firstRevocationTime);
        Assert.True(tokenRevocation.LastRevokedAt >= secondRevocationTime);
    }

    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(5, 6, 6)]
    [InlineData(10, 11, 11)]
    public void RevokeAllTokens_SequentialCalls_IncrementsCorrectly(int numberOfCalls, int expectedAccessVersion, int expectedRefreshVersion)
    {
        var tokenRevocation = new TokenRevocationVersion();

        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocation.RevokeAllTokens();
        }

        Assert.Equal(expectedAccessVersion, tokenRevocation.AccessTokenVersion);
        Assert.Equal(expectedRefreshVersion, tokenRevocation.RefreshTokenVersion);
    }

    [Fact]
    public void RevokeAllTokens_CalledWithLastRevokedAtCheck_ConfirmsUtcTimestamp()
    {
        var tokenRevocation = new TokenRevocationVersion();
        var beforeCall = DateTime.UtcNow;

        tokenRevocation.RevokeAllTokens();

        var afterCall = DateTime.UtcNow;
        Assert.InRange(tokenRevocation.LastRevokedAt, beforeCall, afterCall);
    }

    [Fact]
    public void Constructor_WithoutParameters_InitializesWithDefaultValues()
    {
        var tokenRevocation = new TokenRevocationVersion();

        Assert.Equal(1, tokenRevocation.AccessTokenVersion);
        Assert.Equal(1, tokenRevocation.RefreshTokenVersion);
        Assert.Equal(default(DateTime), tokenRevocation.LastRevokedAt);
    }

    [Fact]
    public void Constructor_CreatesIndependentInstances_VersionsNotShared()
    {
        var revocation1 = new TokenRevocationVersion();
        var revocation2 = new TokenRevocationVersion();

        revocation1.RevokeAccessTokens();

        Assert.Equal(2, revocation1.AccessTokenVersion);
        Assert.Equal(1, revocation2.AccessTokenVersion);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(50)]
    [InlineData(1000)]
    public void RevokeAccessTokensAndRefreshTokens_MixedCalls_EachIncrementsIndependently(int callCount)
    {
        var tokenRevocation = new TokenRevocationVersion();

        for (int i = 0; i < callCount; i++)
        {
            tokenRevocation.RevokeAccessTokens();
            tokenRevocation.RevokeRefreshTokens();
        }

        Assert.Equal(1 + callCount, tokenRevocation.AccessTokenVersion);
        Assert.Equal(1 + callCount, tokenRevocation.RefreshTokenVersion);
    }

    [Fact]
    public void LastRevokedAt_IsAlwaysUtcKind_AfterRevokeAllTokens()
    {
        var tokenRevocation = new TokenRevocationVersion();

        tokenRevocation.RevokeAllTokens();

        Assert.Equal(DateTimeKind.Utc, tokenRevocation.LastRevokedAt.Kind);
    }
}
