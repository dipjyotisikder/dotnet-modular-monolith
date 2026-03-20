using Xunit;
using Users.Domain.ValueObjects;

namespace Users.Domain.UnitTests.ValueObjects;

public class TokenRevocationVersionTests
{
    /// <summary>
    /// Tests that RevokeAccessTokens increments AccessTokenVersion from initial value of 1 to 2.
    /// Input: Newly instantiated TokenRevocationVersion object.
    /// Expected: AccessTokenVersion is incremented to 2.
    /// </summary>
    [Fact]
    public void RevokeAccessTokens_InitialCall_IncrementsAccessTokenVersionToTwo()
    {
        // Arrange
        var tokenRevocationVersion = new TokenRevocationVersion();

        // Act
        tokenRevocationVersion.RevokeAccessTokens();

        // Assert
        Assert.Equal(2, tokenRevocationVersion.AccessTokenVersion);
    }

    /// <summary>
    /// Tests that RevokeAccessTokens sets LastRevokedAt to current UTC time.
    /// Input: Newly instantiated TokenRevocationVersion object.
    /// Expected: LastRevokedAt is set to a DateTime value very close to DateTime.UtcNow.
    /// </summary>
    [Fact]
    public void RevokeAccessTokens_WhenCalled_SetsLastRevokedAtToCurrentUtcTime()
    {
        // Arrange
        var tokenRevocationVersion = new TokenRevocationVersion();
        var beforeCall = DateTime.UtcNow;

        // Act
        tokenRevocationVersion.RevokeAccessTokens();

        // Assert
        var afterCall = DateTime.UtcNow;
        Assert.True(tokenRevocationVersion.LastRevokedAt >= beforeCall);
        Assert.True(tokenRevocationVersion.LastRevokedAt <= afterCall);
    }

    /// <summary>
    /// Tests that multiple calls to RevokeAccessTokens continue to increment AccessTokenVersion correctly.
    /// Input: Variable number of successive calls to RevokeAccessTokens.
    /// Expected: AccessTokenVersion equals 1 + number of calls.
    /// </summary>
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(5, 6)]
    [InlineData(10, 11)]
    [InlineData(100, 101)]
    public void RevokeAccessTokens_MultipleCalls_ContinuesIncrementingAccessTokenVersion(int numberOfCalls, int expectedVersion)
    {
        // Arrange
        var tokenRevocationVersion = new TokenRevocationVersion();

        // Act
        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocationVersion.RevokeAccessTokens();
        }

        // Assert
        Assert.Equal(expectedVersion, tokenRevocationVersion.AccessTokenVersion);
    }

    /// <summary>
    /// Tests that successive calls to RevokeAccessTokens update LastRevokedAt to newer values.
    /// Input: Two successive calls to RevokeAccessTokens with a small delay between them.
    /// Expected: Second LastRevokedAt value is greater than or equal to the first.
    /// </summary>
    [Fact]
    public void RevokeAccessTokens_SuccessiveCalls_UpdatesLastRevokedAtToNewerValue()
    {
        // Arrange
        var tokenRevocationVersion = new TokenRevocationVersion();

        // Act
        tokenRevocationVersion.RevokeAccessTokens();
        var firstRevocation = tokenRevocationVersion.LastRevokedAt;

        System.Threading.Thread.Sleep(10);

        tokenRevocationVersion.RevokeAccessTokens();
        var secondRevocation = tokenRevocationVersion.LastRevokedAt;

        // Assert
        Assert.True(secondRevocation >= firstRevocation);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens increments RefreshTokenVersion from default value (1 to 2).
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_InitialCall_IncrementsRefreshTokenVersionToTwo()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var initialVersion = tokenRevocation.RefreshTokenVersion;

        // Act
        tokenRevocation.RevokeRefreshTokens();

        // Assert
        Assert.Equal(initialVersion + 1, tokenRevocation.RefreshTokenVersion);
        Assert.Equal(2, tokenRevocation.RefreshTokenVersion);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens increments RefreshTokenVersion correctly on multiple consecutive calls.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void RevokeRefreshTokens_MultipleCalls_IncrementsRefreshTokenVersionCorrectly(int numberOfCalls)
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var initialVersion = tokenRevocation.RefreshTokenVersion;

        // Act
        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocation.RevokeRefreshTokens();
        }

        // Assert
        Assert.Equal(initialVersion + numberOfCalls, tokenRevocation.RefreshTokenVersion);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens updates LastRevokedAt to current UTC time.
    /// Verifies the timestamp is within a reasonable time window (5 seconds).
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_Called_UpdatesLastRevokedAtToCurrentUtcTime()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var timeBefore = DateTime.UtcNow;

        // Act
        tokenRevocation.RevokeRefreshTokens();

        // Assert
        var timeAfter = DateTime.UtcNow;
        Assert.InRange(tokenRevocation.LastRevokedAt, timeBefore, timeAfter);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens updates LastRevokedAt on each call.
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_CalledMultipleTimes_UpdatesLastRevokedAtEachTime()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        tokenRevocation.RevokeRefreshTokens();
        var firstRevokedAt = tokenRevocation.LastRevokedAt;

        // Wait a small amount to ensure time difference
        System.Threading.Thread.Sleep(10);

        // Act
        tokenRevocation.RevokeRefreshTokens();

        // Assert
        var secondRevokedAt = tokenRevocation.LastRevokedAt;
        Assert.True(secondRevokedAt > firstRevokedAt, "LastRevokedAt should be updated to a later time on subsequent calls");
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens does not affect AccessTokenVersion.
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_Called_DoesNotChangeAccessTokenVersion()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var initialAccessTokenVersion = tokenRevocation.AccessTokenVersion;

        // Act
        tokenRevocation.RevokeRefreshTokens();

        // Assert
        Assert.Equal(initialAccessTokenVersion, tokenRevocation.AccessTokenVersion);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens can handle many increments without throwing exceptions.
    /// This tests for potential overflow scenarios (though unlikely in practice).
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_ManyIncrements_HandlesLargeVersionNumbers()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        int numberOfCalls = 1000;

        // Act
        Exception? exception = Record.Exception(() =>
        {
            for (int i = 0; i < numberOfCalls; i++)
            {
                tokenRevocation.RevokeRefreshTokens();
            }
        });

        // Assert
        Assert.Null(exception);
        Assert.Equal(1 + numberOfCalls, tokenRevocation.RefreshTokenVersion);
    }

    /// <summary>
    /// Verifies that RevokeAllTokens increments both AccessTokenVersion and RefreshTokenVersion
    /// from their initial values and sets LastRevokedAt to the current UTC time.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_InitialState_IncrementsVersionsAndSetsLastRevokedAt()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var beforeRevocation = DateTime.UtcNow;

        // Act
        tokenRevocation.RevokeAllTokens();

        // Assert
        var afterRevocation = DateTime.UtcNow;
        Assert.Equal(2, tokenRevocation.AccessTokenVersion);
        Assert.Equal(2, tokenRevocation.RefreshTokenVersion);
        Assert.InRange(tokenRevocation.LastRevokedAt, beforeRevocation, afterRevocation);
    }

    /// <summary>
    /// Verifies that calling RevokeAllTokens multiple times continues to increment
    /// both token versions sequentially and updates LastRevokedAt each time.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_MultipleInvocations_ContinuesIncrementingVersions()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();

        // Act
        tokenRevocation.RevokeAllTokens();
        var firstRevocationTime = tokenRevocation.LastRevokedAt;

        System.Threading.Thread.Sleep(10); // Ensure time difference

        tokenRevocation.RevokeAllTokens();
        var secondRevocationTime = tokenRevocation.LastRevokedAt;

        tokenRevocation.RevokeAllTokens();

        // Assert
        Assert.Equal(4, tokenRevocation.AccessTokenVersion);
        Assert.Equal(4, tokenRevocation.RefreshTokenVersion);
        Assert.True(secondRevocationTime > firstRevocationTime);
        Assert.True(tokenRevocation.LastRevokedAt >= secondRevocationTime);
    }

    /// <summary>
    /// Verifies that RevokeAllTokens correctly increments versions when called
    /// a specific number of times in sequence.
    /// Tests with 1, 5, and 10 invocations to ensure consistent behavior.
    /// </summary>
    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(5, 6, 6)]
    [InlineData(10, 11, 11)]
    public void RevokeAllTokens_SequentialCalls_IncrementsCorrectly(int numberOfCalls, int expectedAccessVersion, int expectedRefreshVersion)
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();

        // Act
        for (int i = 0; i < numberOfCalls; i++)
        {
            tokenRevocation.RevokeAllTokens();
        }

        // Assert
        Assert.Equal(expectedAccessVersion, tokenRevocation.AccessTokenVersion);
        Assert.Equal(expectedRefreshVersion, tokenRevocation.RefreshTokenVersion);
    }

    /// <summary>
    /// Verifies that LastRevokedAt is always set to a UTC DateTime value
    /// and is close to the current UTC time (within 1 second tolerance).
    /// </summary>
    [Fact]
    public void RevokeAllTokens_Always_SetsLastRevokedAtToUtcTime()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var expectedTime = DateTime.UtcNow;

        // Act
        tokenRevocation.RevokeAllTokens();

        // Assert
        var timeDifference = Math.Abs((tokenRevocation.LastRevokedAt - expectedTime).TotalSeconds);
        Assert.True(timeDifference < 1, $"LastRevokedAt should be within 1 second of UtcNow, but difference was {timeDifference} seconds");
    }

    /// <summary>
    /// Verifies that RevokeAllTokens updates all three properties (AccessTokenVersion,
    /// RefreshTokenVersion, and LastRevokedAt) in a single invocation.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_SingleInvocation_UpdatesAllThreeProperties()
    {
        // Arrange
        var tokenRevocation = new TokenRevocationVersion();
        var initialAccessVersion = tokenRevocation.AccessTokenVersion;
        var initialRefreshVersion = tokenRevocation.RefreshTokenVersion;
        var initialLastRevokedAt = tokenRevocation.LastRevokedAt;

        // Act
        tokenRevocation.RevokeAllTokens();

        // Assert
        Assert.NotEqual(initialAccessVersion, tokenRevocation.AccessTokenVersion);
        Assert.NotEqual(initialRefreshVersion, tokenRevocation.RefreshTokenVersion);
        Assert.NotEqual(initialLastRevokedAt, tokenRevocation.LastRevokedAt);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes all properties to their expected default values.
    /// Verifies that AccessTokenVersion is 1, RefreshTokenVersion is 1, and LastRevokedAt is DateTime.MinValue.
    /// </summary>
    [Fact]
    public void Constructor_WhenCalled_InitializesPropertiesWithDefaultValues()
    {
        // Arrange & Act
        var tokenRevocationVersion = new TokenRevocationVersion();

        // Assert
        Assert.NotNull(tokenRevocationVersion);
        Assert.Equal(1, tokenRevocationVersion.AccessTokenVersion);
        Assert.Equal(1, tokenRevocationVersion.RefreshTokenVersion);
        Assert.Equal(DateTime.MinValue, tokenRevocationVersion.LastRevokedAt);
    }
}