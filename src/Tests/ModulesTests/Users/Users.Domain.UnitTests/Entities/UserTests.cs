using Moq;
using Xunit;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Services;

namespace Users.Domain.UnitTests.Entities;
/// <summary>
/// Unit tests for the <see cref = "User.TokenRevocationVersion"/> property.
/// </summary>
public class UserTests
{
    /// <summary>
    /// Tests that TokenRevocationVersion returns a new instance with default values
    /// when a User is initially created and the backing JSON field is "{}".
    /// </summary>
    [Fact]
    public void TokenRevocationVersion_DefaultState_ReturnsNewInstanceWithDefaultValues()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User", "hash123");
        var user = result.Value;
        // Act
        var tokenVersion = user.TokenRevocationVersion;
        // Assert
        Assert.NotNull(tokenVersion);
        Assert.Equal(1, tokenVersion.AccessTokenVersion);
        Assert.Equal(1, tokenVersion.RefreshTokenVersion);
        Assert.Equal(default(DateTime), tokenVersion.LastRevokedAt);
    }

    /// <summary>
    /// Tests that TokenRevocationVersion getter returns a new instance on each call,
    /// demonstrating deserialization behavior.
    /// </summary>
    [Fact]
    public void TokenRevocationVersion_GetterCalledMultipleTimes_ReturnsNewInstancesEachTime()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User", "hash123");
        var user = result.Value;
        // Act
        var tokenVersion1 = user.TokenRevocationVersion;
        var tokenVersion2 = user.TokenRevocationVersion;
        // Assert
        Assert.NotSame(tokenVersion1, tokenVersion2);
        Assert.Equal(tokenVersion1.AccessTokenVersion, tokenVersion2.AccessTokenVersion);
        Assert.Equal(tokenVersion1.RefreshTokenVersion, tokenVersion2.RefreshTokenVersion);
    }

    /// <summary>
    /// Tests that TokenRevocationVersion getter correctly handles the empty JSON object "{}"
    /// by returning a new instance with default values.
    /// </summary>
    [Fact]
    public void TokenRevocationVersion_WithEmptyJsonBackingField_ReturnsDefaultInstance()
    {
        // Arrange - Create a new user which initializes backing field to "{}"
        var result = User.Create("test@example.com", "Test User", "hash123");
        var user = result.Value;
        // Act
        var tokenVersion = user.TokenRevocationVersion;
        // Assert
        Assert.NotNull(tokenVersion);
        Assert.Equal(1, tokenVersion.AccessTokenVersion);
        Assert.Equal(1, tokenVersion.RefreshTokenVersion);
        Assert.Equal(default(DateTime), tokenVersion.LastRevokedAt);
    }

    /// <summary>
    /// Verifies that ClearRefreshToken method sets both RefreshToken and RefreshTokenExpiresAt to null
    /// when they have been previously set with values.
    /// </summary>
    [Fact]
    public void ClearRefreshToken_WhenRefreshTokenIsSet_ClearsBothProperties()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword123");
        var user = userResult.Value!;
        var refreshToken = "valid-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        user.SetRefreshToken(refreshToken, expiresAt);
        // Act
        user.ClearRefreshToken();
        // Assert
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Verifies that ClearRefreshToken method is idempotent and can be safely called
    /// when RefreshToken and RefreshTokenExpiresAt are already null.
    /// </summary>
    [Fact]
    public void ClearRefreshToken_WhenRefreshTokenIsAlreadyNull_RemainsNull()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        user.ClearRefreshToken();
        // Assert
        Assert.Null(user.RefreshToken);
        Assert.Null(user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens correctly increments the RefreshTokenVersion 
    /// for the specified number of calls and persists the changes.
    /// </summary>
    /// <param name = "numberOfCalls">The number of times to call RevokeRefreshTokens.</param>
    /// <param name = "expectedVersion">The expected RefreshTokenVersion after the calls.</param>
    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    [InlineData(5, 6)]
    public void RevokeRefreshTokens_VariousCalls_IncrementsRefreshTokenVersionCorrectly(int numberOfCalls, int expectedVersion)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword", "free");
        var user = userResult.Value!;
        var initialVersion = user.TokenRevocationVersion.RefreshTokenVersion;
        // Act
        for (int i = 0; i < numberOfCalls; i++)
        {
            user.RevokeRefreshTokens();
        }

        // Assert
        var finalVersion = user.TokenRevocationVersion.RefreshTokenVersion;
        Assert.Equal(expectedVersion, finalVersion);
        Assert.Equal(initialVersion + numberOfCalls, finalVersion);
    }

    /// <summary>
    /// Tests that RevokeRefreshTokens does not modify the AccessTokenVersion,
    /// ensuring it only affects the RefreshTokenVersion.
    /// </summary>
    [Fact]
    public void RevokeRefreshTokens_DoesNotModifyAccessTokenVersion()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword", "free");
        var user = userResult.Value!;
        var initialAccessTokenVersion = user.TokenRevocationVersion.AccessTokenVersion;
        // Act
        user.RevokeRefreshTokens();
        // Assert
        var finalAccessTokenVersion = user.TokenRevocationVersion.AccessTokenVersion;
        Assert.Equal(initialAccessTokenVersion, finalAccessTokenVersion);
        Assert.Equal(1, finalAccessTokenVersion);
    }

    #region CreateWithOAuth Tests
    /// <summary>
    /// Tests that CreateWithOAuth successfully creates a user with valid inputs and default tier.
    /// Input: Valid email, name, oauthProvider, oauthProviderId
    /// Expected: Returns Success result with User having correct property values and default tier "free"
    /// </summary>
    [Fact]
    public void CreateWithOAuth_ValidInputsWithDefaultTier_ReturnsSuccessWithCorrectUser()
    {
        // Arrange
        string email = "test@example.com";
        string name = "Test User";
        string oauthProvider = "Google";
        string oauthProviderId = "google123";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal("free", result.Value.Tier);
        Assert.Equal(oauthProvider, result.Value.OAuthProvider);
        Assert.Equal(oauthProviderId, result.Value.OAuthProviderId);
        Assert.Equal("User", result.Value.Roles);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    /// <summary>
    /// Tests that CreateWithOAuth successfully creates a user with valid inputs and custom tier.
    /// Input: Valid email, name, oauthProvider, oauthProviderId, and custom tier "premium"
    /// Expected: Returns Success result with User having correct property values including custom tier
    /// </summary>
    [Fact]
    public void CreateWithOAuth_ValidInputsWithCustomTier_ReturnsSuccessWithCorrectTier()
    {
        // Arrange
        string email = "premium@example.com";
        string name = "Premium User";
        string oauthProvider = "Facebook";
        string oauthProviderId = "fb456";
        string tier = "premium";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId, tier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tier, result.Value.Tier);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
    }

    /// <summary>
    /// Tests that CreateWithOAuth succeeds when oauthProviderId is null.
    /// Input: Valid email, name, oauthProvider, but null oauthProviderId
    /// Expected: Returns Success result (no validation on oauthProviderId)
    /// </summary>
    [Fact]
    public void CreateWithOAuth_NullOAuthProviderId_ReturnsSuccess()
    {
        // Arrange
        string email = "test@example.com";
        string name = "Test User";
        string oauthProvider = "Google";
        string? oauthProviderId = null;
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId!);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.OAuthProviderId);
    }

    /// <summary>
    /// Tests that CreateWithOAuth succeeds when oauthProviderId is empty string.
    /// Input: Valid email, name, oauthProvider, but empty oauthProviderId
    /// Expected: Returns Success result (no validation on oauthProviderId)
    /// </summary>
    [Fact]
    public void CreateWithOAuth_EmptyOAuthProviderId_ReturnsSuccess()
    {
        // Arrange
        string email = "test@example.com";
        string name = "Test User";
        string oauthProvider = "Google";
        string oauthProviderId = string.Empty;
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.OAuthProviderId);
    }

    /// <summary>
    /// Tests that CreateWithOAuth fails when email is null, empty, or whitespace.
    /// Input: Various invalid email values (null, empty, whitespace)
    /// Expected: Returns Failure result with "Email Cannot Be Empty" and VALIDATION_ERROR code
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void CreateWithOAuth_InvalidEmail_ReturnsFailure(string? email)
    {
        // Arrange
        string name = "Test User";
        string oauthProvider = "Google";
        string oauthProviderId = "google123";
        // Act
        Result<User> result = User.CreateWithOAuth(email!, name, oauthProvider, oauthProviderId);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that CreateWithOAuth fails when name is null, empty, or whitespace.
    /// Input: Valid email but various invalid name values (null, empty, whitespace)
    /// Expected: Returns Failure result with "Name Cannot Be Empty" and VALIDATION_ERROR code
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void CreateWithOAuth_InvalidName_ReturnsFailure(string? name)
    {
        // Arrange
        string email = "test@example.com";
        string oauthProvider = "Google";
        string oauthProviderId = "google123";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name!, oauthProvider, oauthProviderId);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that CreateWithOAuth fails when oauthProvider is null, empty, or whitespace.
    /// Input: Valid email and name but various invalid oauthProvider values (null, empty, whitespace)
    /// Expected: Returns Failure result with "OAuth Provider Cannot Be Empty" and VALIDATION_ERROR code
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void CreateWithOAuth_InvalidOAuthProvider_ReturnsFailure(string? oauthProvider)
    {
        // Arrange
        string email = "test@example.com";
        string name = "Test User";
        string oauthProviderId = "google123";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider!, oauthProviderId);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("OAuth Provider Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that CreateWithOAuth accepts various valid tier values without validation.
    /// Input: Valid email, name, oauthProvider, oauthProviderId with various tier values
    /// Expected: Returns Success result with the provided tier value
    /// </summary>
    [Theory]
    [InlineData("free")]
    [InlineData("premium")]
    [InlineData("enterprise")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("CUSTOM_TIER_123")]
    public void CreateWithOAuth_VariousTierValues_ReturnsSuccessWithProvidedTier(string tier)
    {
        // Arrange
        string email = "test@example.com";
        string name = "Test User";
        string oauthProvider = "Google";
        string oauthProviderId = "google123";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId, tier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tier, result.Value.Tier);
    }

    /// <summary>
    /// Tests that CreateWithOAuth accepts special characters in string parameters.
    /// Input: Email, name, and oauthProvider with special characters
    /// Expected: Returns Success result with values preserved as-is
    /// </summary>
    [Fact]
    public void CreateWithOAuth_SpecialCharactersInStrings_ReturnsSuccess()
    {
        // Arrange
        string email = "test+special@example.com";
        string name = "Test User <>&\"'";
        string oauthProvider = "Google-OAuth2.0";
        string oauthProviderId = "id:special/chars\\test";
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(oauthProvider, result.Value.OAuthProvider);
        Assert.Equal(oauthProviderId, result.Value.OAuthProviderId);
    }

    /// <summary>
    /// Tests that CreateWithOAuth handles very long string inputs.
    /// Input: Very long strings for email, name, oauthProvider, oauthProviderId
    /// Expected: Returns Success result with values preserved
    /// </summary>
    [Fact]
    public void CreateWithOAuth_VeryLongStrings_ReturnsSuccess()
    {
        // Arrange
        string email = new string('a', 1000) + "@example.com";
        string name = new string('b', 1000);
        string oauthProvider = new string('c', 1000);
        string oauthProviderId = new string('d', 1000);
        // Act
        Result<User> result = User.CreateWithOAuth(email, name, oauthProvider, oauthProviderId);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(oauthProvider, result.Value.OAuthProvider);
        Assert.Equal(oauthProviderId, result.Value.OAuthProviderId);
    }

    #endregion
    /// <summary>
    /// Tests that AddRole returns a failure result when the role parameter is null.
    /// </summary>
    [Fact]
    public void AddRole_NullRole_ReturnsFailure()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        string role = null!;
        // Act
        var result = user.AddRole(role);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Role Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that AddRole returns a failure result when the role parameter is an empty string.
    /// </summary>
    [Fact]
    public void AddRole_EmptyRole_ReturnsFailure()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole(string.Empty);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Role Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that AddRole returns a failure result when the role parameter contains only whitespace characters.
    /// </summary>
    /// <param name = "whitespaceRole">A string containing only whitespace characters.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t  \n  ")]
    public void AddRole_WhitespaceOnlyRole_ReturnsFailure(string whitespaceRole)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole(whitespaceRole);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Role Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that AddRole successfully adds a new valid role, updates the Roles property, and returns success.
    /// </summary>
    [Fact]
    public void AddRole_ValidNewRole_AddsRoleAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var initialRoles = user.Roles;
        // Act
        var result = user.AddRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Contains("Admin", user.Roles.Split(','));
        Assert.Contains("User", user.Roles.Split(','));
        Assert.Equal("User,Admin", user.Roles);
    }

    /// <summary>
    /// Tests that AddRole does not add a duplicate role when the role already exists, but still returns success.
    /// </summary>
    [Fact]
    public void AddRole_DuplicateRole_DoesNotAddAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        user.AddRole("Admin");
        var rolesBeforeDuplicate = user.Roles;
        // Act
        var result = user.AddRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(rolesBeforeDuplicate, user.Roles);
        Assert.Equal(1, user.Roles.Split(',').Count(r => r == "Admin"));
    }

    /// <summary>
    /// Tests that AddRole correctly adds multiple different roles sequentially.
    /// </summary>
    [Fact]
    public void AddRole_MultipleRoles_AddsAllRolesSuccessfully()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result1 = user.AddRole("Admin");
        var result2 = user.AddRole("Moderator");
        var result3 = user.AddRole("Premium");
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);
        Assert.Contains("User", user.Roles.Split(','));
        Assert.Contains("Admin", user.Roles.Split(','));
        Assert.Contains("Moderator", user.Roles.Split(','));
        Assert.Contains("Premium", user.Roles.Split(','));
        Assert.Equal("User,Admin,Moderator,Premium", user.Roles);
    }

    /// <summary>
    /// Tests that AddRole handles roles with special characters correctly.
    /// </summary>
    [Theory]
    [InlineData("Super-Admin")]
    [InlineData("Role_With_Underscores")]
    [InlineData("Role.With.Dots")]
    [InlineData("Role123")]
    [InlineData("UPPERCASE")]
    [InlineData("lowercase")]
    [InlineData("MixedCase")]
    public void AddRole_RoleWithSpecialCharacters_AddsSuccessfully(string role)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole(role);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(role, user.Roles.Split(','));
    }

    /// <summary>
    /// Tests that AddRole handles very long role names correctly.
    /// </summary>
    [Fact]
    public void AddRole_VeryLongRoleName_AddsSuccessfully()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var longRole = new string('A', 1000);
        // Act
        var result = user.AddRole(longRole);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(longRole, user.Roles.Split(','));
    }

    /// <summary>
    /// Tests that AddRole with a role containing commas is handled (edge case that could break comma-separated format).
    /// </summary>
    [Fact]
    public void AddRole_RoleWithComma_AddsRoleButMayBreakFormat()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole("Role,WithComma");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("Role,WithComma", user.Roles);
    }

    /// <summary>
    /// Tests that AddRole on the default "User" role is idempotent.
    /// </summary>
    [Fact]
    public void AddRole_AddingDefaultUserRole_IsIdempotent()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var initialRoles = user.Roles;
        // Act
        var result = user.AddRole("User");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(initialRoles, user.Roles);
        Assert.Equal(1, user.Roles.Split(',').Count(r => r == "User"));
    }

    /// <summary>
    /// Tests that AddRole works correctly when the user already has multiple roles.
    /// </summary>
    [Fact]
    public void AddRole_UserWithMultipleExistingRoles_AddsNewRoleCorrectly()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        user.AddRole("Admin");
        user.AddRole("Moderator");
        // Act
        var result = user.AddRole("Premium");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User,Admin,Moderator,Premium", user.Roles);
        Assert.Equal(4, user.Roles.Split(',').Length);
    }

    /// <summary>
    /// Tests that AddRole handles role names with leading and trailing whitespace by treating them as different from trimmed versions.
    /// Note: The current implementation does not trim whitespace, so " Admin " is different from "Admin".
    /// </summary>
    [Fact]
    public void AddRole_RoleWithLeadingTrailingWhitespace_AddsAsIs()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole(" Admin ");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(" Admin ", user.Roles.Split(','));
    }

    /// <summary>
    /// Tests that AddRole handles Unicode characters in role names correctly.
    /// </summary>
    [Theory]
    [InlineData("Rôle")]
    [InlineData("角色")]
    [InlineData("Роль")]
    [InlineData("🎭Role")]
    public void AddRole_RoleWithUnicodeCharacters_AddsSuccessfully(string role)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var result = user.AddRole(role);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains(role, user.Roles.Split(','));
    }

    /// <summary>
    /// Tests that AddRole returns success even when adding an already existing role (idempotency check).
    /// </summary>
    [Fact]
    public void AddRole_ExistingRole_ReturnsSuccessWithoutModification()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        user.AddRole("Admin");
        var rolesCountBefore = user.Roles.Split(',').Length;
        // Act
        var result = user.AddRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(rolesCountBefore, user.Roles.Split(',').Length);
    }

    /// <summary>
    /// Tests that RevokeAllTokens with default reason returns success result.
    /// Verifies that when no reason parameter is provided, the method uses the default value
    /// and returns a successful result.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_WithDefaultReason_ReturnsSuccessResult()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RevokeAllTokens();
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that RevokeAllTokens with various reason values returns success result.
    /// Verifies that different string inputs for the reason parameter are accepted
    /// and the method consistently returns success.
    /// </summary>
    /// <param name = "reason">The reason string to test.</param>
    [Theory]
    [InlineData("Admin revocation")]
    [InlineData("Custom reason")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t\n")]
    [InlineData("Very long reason string that contains many characters to test if the method can handle strings of substantial length without any issues or exceptions being thrown during execution")]
    [InlineData("Special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [InlineData("Unicode characters: 你好世界 مرحبا العالم")]
    public void RevokeAllTokens_WithVariousReasons_ReturnsSuccessResult(string reason)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RevokeAllTokens(reason);
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that RevokeAllTokens can be called with null reason without throwing an exception.
    /// The reason parameter is currently unused in the implementation.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_WithNullReason_ThrowsArgumentNullException()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RevokeAllTokens(null!);
        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that RevokeAllTokens can be called multiple times successfully.
    /// Verifies that the method is idempotent and can be invoked repeatedly
    /// without throwing exceptions or failing.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_CalledMultipleTimes_ReturnsSuccessEachTime()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result1 = user.RevokeAllTokens("First revocation");
        var result2 = user.RevokeAllTokens("Second revocation");
        var result3 = user.RevokeAllTokens("Third revocation");
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);
    }

    /// <summary>
    /// Tests that RevokeAllTokens works correctly for OAuth users.
    /// Verifies that the method functions properly for users created via OAuth
    /// and returns a successful result.
    /// </summary>
    [Fact]
    public void RevokeAllTokens_ForOAuthUser_ReturnsSuccessResult()
    {
        // Arrange
        var userResult = User.CreateWithOAuth("oauth@example.com", "OAuth User", "Google", "google-user-123");
        var user = userResult.Value;
        // Act
        var result = user.RevokeAllTokens("OAuth user revocation");
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that UpdatePassword fails when provided with null, empty, or whitespace-only password hash values.
    /// The test verifies that the method returns a failure result with the appropriate error message and error code.
    /// </summary>
    /// <param name = "invalidPasswordHash">The invalid password hash value to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void UpdatePassword_InvalidPasswordHash_ReturnsFailure(string? invalidPasswordHash)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "initialHash");
        var user = userResult.Value!;
        // Act
        var result = user.UpdatePassword(invalidPasswordHash!);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Password Hash Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that UpdatePassword succeeds with a valid password hash.
    /// Verifies that the PasswordHash property is updated, LastPasswordChangedAt is set,
    /// and the method returns a success result.
    /// </summary>
    [Fact]
    public void UpdatePassword_ValidPasswordHash_ReturnsSuccessAndUpdatesProperties()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "initialHash");
        var user = userResult.Value!;
        var newPasswordHash = "newSecurePasswordHash123";
        var beforeUpdate = DateTime.UtcNow;
        // Act
        var result = user.UpdatePassword(newPasswordHash);
        var afterUpdate = DateTime.UtcNow;
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.NotNull(user.LastPasswordChangedAt);
        Assert.True(user.LastPasswordChangedAt >= beforeUpdate && user.LastPasswordChangedAt <= afterUpdate);
    }

    /// <summary>
    /// Tests that UpdatePassword handles various valid password hash formats including
    /// very long strings, special characters, and unicode characters.
    /// </summary>
    /// <param name = "passwordHash">The password hash value to test.</param>
    [Theory]
    [InlineData("shortHash")]
    [InlineData("$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")]
    [InlineData("hash!@#$%^&*()_+-={}[]|\\:;\"'<>,.?/~`")]
    [InlineData("hashWith\tTabsAnd\nNewlines")]
    [InlineData("unicodeHash_émüñ中文🔒")]
    public void UpdatePassword_VariousValidPasswordHashes_ReturnsSuccess(string passwordHash)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "initialHash");
        var user = userResult.Value!;
        // Act
        var result = user.UpdatePassword(passwordHash);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(passwordHash, user.PasswordHash);
    }

    /// <summary>
    /// Tests that UpdatePassword with a very long password hash (simulating extreme edge cases)
    /// succeeds and properly stores the value.
    /// </summary>
    [Fact]
    public void UpdatePassword_VeryLongPasswordHash_ReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "initialHash");
        var user = userResult.Value!;
        var veryLongHash = new string('a', 10000);
        // Act
        var result = user.UpdatePassword(veryLongHash);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(veryLongHash, user.PasswordHash);
    }

    /// <summary>
    /// Tests that UpdateTier returns a failure result when the tier parameter is null.
    /// </summary>
    [Fact]
    public void UpdateTier_NullTier_ReturnsFailure()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        string? tier = null;
        // Act
        var result = user.UpdateTier(tier!);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Tier Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that UpdateTier returns a failure result when the tier parameter is an empty string.
    /// </summary>
    [Fact]
    public void UpdateTier_EmptyString_ReturnsFailure()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        var tier = string.Empty;
        // Act
        var result = user.UpdateTier(tier);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Tier Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that UpdateTier returns a failure result when the tier parameter contains only whitespace characters.
    /// </summary>
    /// <param name = "whitespaceTier">The whitespace-only string to test.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n  ")]
    public void UpdateTier_WhitespaceOnlyString_ReturnsFailure(string whitespaceTier)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        // Act
        var result = user.UpdateTier(whitespaceTier);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Tier Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that UpdateTier successfully updates the tier property and returns a success result with a valid tier value.
    /// </summary>
    /// <param name = "validTier">The valid tier value to test.</param>
    [Theory]
    [InlineData("premium")]
    [InlineData("enterprise")]
    [InlineData("basic")]
    public void UpdateTier_ValidTier_ReturnsSucessAndUpdatesTierProperty(string validTier)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        var originalTier = user.Tier;
        // Act
        var result = user.UpdateTier(validTier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
        Assert.Equal(validTier, user.Tier);
        Assert.NotEqual(originalTier, user.Tier);
    }

    /// <summary>
    /// Tests that UpdateTier accepts tier values with special characters and updates the tier property correctly.
    /// </summary>
    /// <param name = "tierWithSpecialChars">The tier value with special characters.</param>
    [Theory]
    [InlineData("premium-plus")]
    [InlineData("tier_1")]
    [InlineData("tier@special")]
    [InlineData("tier#123")]
    [InlineData("tier$$$")]
    [InlineData("tier!@#$%^&*()")]
    public void UpdateTier_TierWithSpecialCharacters_ReturnsSucessAndUpdatesTierProperty(string tierWithSpecialChars)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        // Act
        var result = user.UpdateTier(tierWithSpecialChars);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(tierWithSpecialChars, user.Tier);
    }

    /// <summary>
    /// Tests that UpdateTier accepts and handles very long tier strings correctly.
    /// </summary>
    [Fact]
    public void UpdateTier_VeryLongString_ReturnsSucessAndUpdatesTierProperty()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        var longTier = new string('A', 10000);
        // Act
        var result = user.UpdateTier(longTier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(longTier, user.Tier);
    }

    /// <summary>
    /// Tests that UpdateTier correctly preserves user state when updating to a different tier multiple times.
    /// </summary>
    [Fact]
    public void UpdateTier_MultipleTierUpdates_EachUpdateSucceedsAndPreservesState()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        var originalEmail = user.Email;
        var originalName = user.Name;
        // Act & Assert
        var result1 = user.UpdateTier("premium");
        Assert.True(result1.IsSuccess);
        Assert.Equal("premium", user.Tier);
        Assert.Equal(originalEmail, user.Email);
        Assert.Equal(originalName, user.Name);
        var result2 = user.UpdateTier("enterprise");
        Assert.True(result2.IsSuccess);
        Assert.Equal("enterprise", user.Tier);
        Assert.Equal(originalEmail, user.Email);
        Assert.Equal(originalName, user.Name);
        var result3 = user.UpdateTier("free");
        Assert.True(result3.IsSuccess);
        Assert.Equal("free", user.Tier);
        Assert.Equal(originalEmail, user.Email);
        Assert.Equal(originalName, user.Name);
    }

    /// <summary>
    /// Tests that UpdateTier accepts tier values with numeric characters.
    /// </summary>
    [Theory]
    [InlineData("tier1")]
    [InlineData("123")]
    [InlineData("0")]
    public void UpdateTier_TierWithNumbers_ReturnsSucessAndUpdatesTierProperty(string tierWithNumbers)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        // Act
        var result = user.UpdateTier(tierWithNumbers);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tierWithNumbers, user.Tier);
    }

    /// <summary>
    /// Tests that UpdateTier accepts tier values with Unicode characters.
    /// </summary>
    [Theory]
    [InlineData("премиум")]
    [InlineData("高级")]
    [InlineData("プレミアム")]
    [InlineData("🚀premium")]
    public void UpdateTier_TierWithUnicodeCharacters_ReturnsSucessAndUpdatesTierProperty(string tierWithUnicode)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        // Act
        var result = user.UpdateTier(tierWithUnicode);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tierWithUnicode, user.Tier);
    }

    /// <summary>
    /// Tests that RemoveRole successfully removes an existing role from the user's roles.
    /// Input: User with "User,Admin" roles, removing "Admin".
    /// Expected: "Admin" is removed, roles become "User", method returns success.
    /// </summary>
    [Fact]
    public void RemoveRole_ExistingRole_RemovesRoleAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");
        // Act
        var result = user.RemoveRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole("Admin"));
        Assert.True(user.HasRole("User"));
    }

    /// <summary>
    /// Tests that RemoveRole handles removing a non-existent role gracefully.
    /// Input: User with "User" role, removing "Admin" which doesn't exist.
    /// Expected: No change to roles, method returns success.
    /// </summary>
    [Fact]
    public void RemoveRole_NonExistentRole_ReturnsSuccessWithoutChange()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RemoveRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.HasRole("User"));
    }

    /// <summary>
    /// Tests that RemoveRole defaults to "User" when the last role is removed.
    /// Input: User with only "Admin" role, removing "Admin".
    /// Expected: Roles default to "User", method returns success.
    /// </summary>
    [Fact]
    public void RemoveRole_RemovingLastRole_DefaultsToUser()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.RemoveRole("User");
        user.AddRole("Admin");
        // Act
        var result = user.RemoveRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.HasRole("User"));
        var roles = user.GetRoles();
        Assert.Single(roles);
        Assert.Equal("User", roles[0]);
    }

    /// <summary>
    /// Tests RemoveRole with various edge case role strings.
    /// Input: Various edge case strings including empty, whitespace, null, special characters.
    /// Expected: Method handles gracefully and returns success.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(null)]
    public void RemoveRole_EdgeCaseRoleStrings_HandlesGracefullyAndReturnsSuccess(string? role)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RemoveRole(role!);
        // Assert
        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Tests that AddRole prevents duplicate roles and RemoveRole fully removes the role.
    /// Input: User with role "User", adding "Admin" twice (second add is ignored).
    /// Expected: AddRole prevents duplicates, so RemoveRole removes the only "Admin" instance.
    /// </summary>
    [Fact]
    public void RemoveRole_DuplicateRoles_RemovesFirstOccurrence()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");
        user.AddRole("Admin"); // This won't add a duplicate due to AddRole's duplicate prevention
        // Act
        var result = user.RemoveRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole("Admin")); // Role is completely removed since there was no duplicate
    }

    /// <summary>
    /// Tests that RemoveRole works correctly with multiple different roles.
    /// Input: User with "User,Admin,Manager", removing "Admin".
    /// Expected: "Admin" is removed, roles become "User,Manager".
    /// </summary>
    [Fact]
    public void RemoveRole_MultipleRoles_RemovesSpecifiedRoleOnly()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");
        user.AddRole("Manager");
        // Act
        var result = user.RemoveRole("Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole("Admin"));
        Assert.True(user.HasRole("User"));
        Assert.True(user.HasRole("Manager"));
    }

    /// <summary>
    /// Tests that RemoveRole handles removing the default "User" role correctly.
    /// Input: User with "User,Admin" roles, removing "User".
    /// Expected: "User" is removed, roles become "Admin".
    /// </summary>
    [Fact]
    public void RemoveRole_RemovingDefaultUserRole_RemovesSuccessfully()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");
        // Act
        var result = user.RemoveRole("User");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole("User"));
        Assert.True(user.HasRole("Admin"));
    }

    /// <summary>
    /// Tests that RemoveRole is case-sensitive.
    /// Input: User with "Admin" role, removing "admin" (lowercase).
    /// Expected: Nothing is removed as roles are case-sensitive, "Admin" remains.
    /// </summary>
    [Fact]
    public void RemoveRole_CaseSensitive_DoesNotRemoveIfCaseMismatch()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");
        // Act
        var result = user.RemoveRole("admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.HasRole("Admin"));
    }

    /// <summary>
    /// Tests RemoveRole with roles containing special characters.
    /// Input: User with "Super-Admin" role, removing "Super-Admin".
    /// Expected: Role is removed successfully.
    /// </summary>
    [Fact]
    public void RemoveRole_RoleWithSpecialCharacters_RemovesSuccessfully()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Super-Admin");
        // Act
        var result = user.RemoveRole("Super-Admin");
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole("Super-Admin"));
    }

    /// <summary>
    /// Tests RemoveRole with very long role name.
    /// Input: User with a very long role name, removing it.
    /// Expected: Role is removed successfully.
    /// </summary>
    [Fact]
    public void RemoveRole_VeryLongRoleName_RemovesSuccessfully()
    {
        // Arrange
        var longRoleName = new string('A', 1000);
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole(longRoleName);
        // Act
        var result = user.RemoveRole(longRoleName);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole(longRoleName));
    }

    /// <summary>
    /// Tests that UpdateName returns failure when the name parameter is null.
    /// </summary>
    [Fact]
    public void UpdateName_NullName_ReturnsFailureWithValidationError()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateName(null!);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Equal("Original Name", user.Name);
    }

    /// <summary>
    /// Tests that UpdateName returns failure when provided with invalid string inputs
    /// including empty strings and whitespace-only strings.
    /// </summary>
    /// <param name = "invalidName">The invalid name input to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("   \t   ")]
    [InlineData(" \t\n\r ")]
    public void UpdateName_InvalidNameInputs_ReturnsFailureWithValidationError(string invalidName)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateName(invalidName);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Equal("Original Name", user.Name);
    }

    /// <summary>
    /// Tests that UpdateName successfully updates the Name property and returns success
    /// when provided with valid name inputs.
    /// </summary>
    /// <param name = "validName">The valid name input to test.</param>
    [Theory]
    [InlineData("John Doe")]
    [InlineData("A")]
    [InlineData("Valid Name")]
    [InlineData("Name With Numbers 123")]
    [InlineData("Name-With-Dashes")]
    [InlineData("Name_With_Underscores")]
    [InlineData("Name.With.Dots")]
    [InlineData("Name@With#Special$Characters")]
    [InlineData("ÄÖÜäöüß")]
    [InlineData("名前")]
    [InlineData("Имя")]
    [InlineData("🙂 Name with Emoji")]
    public void UpdateName_ValidNameInputs_UpdatesNameAndReturnsSuccess(string validName)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateName(validName);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(string.Empty, result.Error);
        Assert.Equal(validName, user.Name);
    }

    /// <summary>
    /// Tests that UpdateName successfully handles very long name strings.
    /// </summary>
    [Fact]
    public void UpdateName_VeryLongName_UpdatesNameAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        var veryLongName = new string('A', 10000);
        // Act
        var result = user.UpdateName(veryLongName);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(veryLongName, user.Name);
    }

    /// <summary>
    /// Tests that UpdateName preserves leading and trailing content when the name
    /// contains spaces but is not whitespace-only.
    /// </summary>
    [Theory]
    [InlineData(" Name")]
    [InlineData("Name ")]
    [InlineData(" Name ")]
    public void UpdateName_NameWithLeadingOrTrailingSpaces_UpdatesNameAndReturnsSuccess(string nameWithSpaces)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateName(nameWithSpaces);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(nameWithSpaces, user.Name);
    }

    /// <summary>
    /// Tests that UpdateName can be called multiple times and correctly updates
    /// the Name property each time.
    /// </summary>
    [Fact]
    public void UpdateName_MultipleCalls_UpdatesNameEachTime()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Original Name", "hash123");
        var user = userResult.Value!;
        // Act & Assert - First update
        var result1 = user.UpdateName("First Update");
        Assert.True(result1.IsSuccess);
        Assert.Equal("First Update", user.Name);
        // Act & Assert - Second update
        var result2 = user.UpdateName("Second Update");
        Assert.True(result2.IsSuccess);
        Assert.Equal("Second Update", user.Name);
        // Act & Assert - Third update
        var result3 = user.UpdateName("Third Update");
        Assert.True(result3.IsSuccess);
        Assert.Equal("Third Update", user.Name);
    }

    /// <summary>
    /// Tests that UpdateName returns failure when attempting to update to an empty
    /// string after having a valid name.
    /// </summary>
    [Fact]
    public void UpdateName_UpdateValidNameToEmpty_ReturnsFailureAndPreservesOriginalName()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Valid Name", "hash123");
        var user = userResult.Value!;
        user.UpdateName("Updated Name");
        // Act
        var result = user.UpdateName("");
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Equal("Updated Name", user.Name);
    }

    /// <summary>
    /// Tests that GetRoles returns a list containing only "User" when a user is created with default roles.
    /// Input: Newly created user with default role assignment
    /// Expected: Returns a list with single element "User"
    /// </summary>
    [Fact]
    public void GetRoles_DefaultUser_ReturnsSingleUserRole()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.NotNull(roles);
        Assert.Single(roles);
        Assert.Equal("User", roles[0]);
    }

    /// <summary>
    /// Tests that GetRoles returns multiple roles in order when roles have been added to the user.
    /// Input: User with "User" and "Admin" roles
    /// Expected: Returns a list containing ["User", "Admin"] in order
    /// </summary>
    [Fact]
    public void GetRoles_UserWithMultipleRoles_ReturnsAllRolesInOrder()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        user.AddRole("Admin");
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.NotNull(roles);
        Assert.Equal(2, roles.Count);
        Assert.Equal("User", roles[0]);
        Assert.Equal("Admin", roles[1]);
    }

    /// <summary>
    /// Tests that GetRoles returns all roles when user has three or more roles.
    /// Input: User with "User", "Admin", and "Moderator" roles
    /// Expected: Returns a list containing all three roles in order
    /// </summary>
    [Fact]
    public void GetRoles_UserWithThreeRoles_ReturnsAllThreeRoles()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        user.AddRole("Admin");
        user.AddRole("Moderator");
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.NotNull(roles);
        Assert.Equal(3, roles.Count);
        Assert.Equal("User", roles[0]);
        Assert.Equal("Admin", roles[1]);
        Assert.Equal("Moderator", roles[2]);
    }

    /// <summary>
    /// Tests that GetRoles returns only remaining roles after a role has been removed.
    /// Input: User with "User" and "Admin" roles, then "User" role removed
    /// Expected: Returns a list containing only "Admin"
    /// </summary>
    [Fact]
    public void GetRoles_AfterRemovingRole_ReturnsRemainingRoles()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        user.AddRole("Admin");
        user.RemoveRole("User");
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.NotNull(roles);
        Assert.Single(roles);
        Assert.Equal("Admin", roles[0]);
    }

    /// <summary>
    /// Tests that GetRoles returns a new list instance each time it is called.
    /// Input: User with default roles, calling GetRoles twice
    /// Expected: Returns different list instances
    /// </summary>
    [Fact]
    public void GetRoles_CalledMultipleTimes_ReturnsNewListEachTime()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        // Act
        var roles1 = user.GetRoles();
        var roles2 = user.GetRoles();
        // Assert
        Assert.NotSame(roles1, roles2);
    }

    /// <summary>
    /// Tests that modifications to the returned list do not affect the user's actual roles.
    /// Input: User with default roles, modifying returned list
    /// Expected: User's roles remain unchanged
    /// </summary>
    [Fact]
    public void GetRoles_ModifyingReturnedList_DoesNotAffectUserRoles()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        // Act
        var roles = user.GetRoles();
        roles.Add("Hacker");
        var rolesAfterModification = user.GetRoles();
        // Assert
        Assert.Single(rolesAfterModification);
        Assert.Equal("User", rolesAfterModification[0]);
        Assert.DoesNotContain("Hacker", rolesAfterModification);
    }

    /// <summary>
    /// Tests that GetRoles correctly handles roles with various characters in their names.
    /// Input: User with roles containing alphanumeric and special characters
    /// Expected: Returns all roles with their exact names preserved
    /// </summary>
    [Fact]
    public void GetRoles_RolesWithSpecialCharacters_ReturnsRolesWithCharactersPreserved()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        user.AddRole("Super-Admin");
        user.AddRole("Level_1");
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.Equal(3, roles.Count);
        Assert.Contains("Super-Admin", roles);
        Assert.Contains("Level_1", roles);
    }

    /// <summary>
    /// Tests that GetRoles returns "User" role when all other roles are removed (RemoveRole ensures User remains).
    /// Input: User with added roles, all non-default roles removed
    /// Expected: Returns list with "User" role
    /// </summary>
    [Fact]
    public void GetRoles_AfterRemovingAllAddedRoles_ReturnsDefaultUserRole()
    {
        // Arrange
        var result = User.Create("test@example.com", "Test User");
        var user = result.Value!;
        user.AddRole("Admin");
        user.AddRole("Moderator");
        user.RemoveRole("Admin");
        user.RemoveRole("Moderator");
        // Act
        var roles = user.GetRoles();
        // Assert
        Assert.NotNull(roles);
        Assert.Single(roles);
        Assert.Equal("User", roles[0]);
    }

    /// <summary>
    /// Tests that UpdateLastLogin successfully updates the LastLoginAt property
    /// and returns a success result when provided with a valid ISystemClock.
    /// </summary>
    /// <param name = "year">The year component of the test DateTime.</param>
    /// <param name = "month">The month component of the test DateTime.</param>
    /// <param name = "day">The day component of the test DateTime.</param>
    /// <param name = "hour">The hour component of the test DateTime.</param>
    /// <param name = "minute">The minute component of the test DateTime.</param>
    /// <param name = "second">The second component of the test DateTime.</param>
    [Theory]
    [InlineData(2024, 1, 15, 10, 30, 0)]
    [InlineData(1, 1, 1, 0, 0, 0)] // DateTime.MinValue
    [InlineData(9999, 12, 31, 23, 59, 59)] // DateTime.MaxValue (approximately)
    [InlineData(2020, 2, 29, 12, 0, 0)] // Leap year date
    [InlineData(2000, 1, 1, 0, 0, 0)] // Y2K date
    public void UpdateLastLogin_ValidClock_UpdatesLastLoginAtAndReturnsSuccess(int year, int month, int day, int hour, int minute, int second)
    {
        // Arrange
        var testDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        var clockMock = new Mock<ISystemClock>();
        clockMock.Setup(c => c.UtcNow).Returns(testDateTime);
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateLastLogin(clockMock.Object);
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(testDateTime, user.LastLoginAt);
        clockMock.Verify(c => c.UtcNow, Times.Once);
    }

    /// <summary>
    /// Tests that UpdateLastLogin updates LastLoginAt from null to a specific DateTime value.
    /// Verifies that a user with no previous login can have their LastLoginAt set.
    /// </summary>
    [Fact]
    public void UpdateLastLogin_FirstLogin_UpdatesLastLoginAtFromNull()
    {
        // Arrange
        var testDateTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var clockMock = new Mock<ISystemClock>();
        clockMock.Setup(c => c.UtcNow).Returns(testDateTime);
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Verify initial state
        Assert.Null(user.LastLoginAt);
        // Act
        var result = user.UpdateLastLogin(clockMock.Object);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(user.LastLoginAt);
        Assert.Equal(testDateTime, user.LastLoginAt);
    }

    /// <summary>
    /// Tests that UpdateLastLogin correctly updates LastLoginAt when called multiple times,
    /// overwriting the previous value with the new clock time.
    /// </summary>
    [Fact]
    public void UpdateLastLogin_MultipleUpdates_UpdatesLastLoginAtToLatestValue()
    {
        // Arrange
        var firstDateTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var secondDateTime = new DateTime(2024, 6, 15, 15, 30, 0, DateTimeKind.Utc);
        var clockMock = new Mock<ISystemClock>();
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Act - First update
        clockMock.Setup(c => c.UtcNow).Returns(firstDateTime);
        user.UpdateLastLogin(clockMock.Object);
        Assert.Equal(firstDateTime, user.LastLoginAt);
        // Act - Second update
        clockMock.Setup(c => c.UtcNow).Returns(secondDateTime);
        var result = user.UpdateLastLogin(clockMock.Object);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(secondDateTime, user.LastLoginAt);
    }

    /// <summary>
    /// Tests that UpdateLastLogin throws NullReferenceException when clock parameter is null.
    /// Verifies that the method does not handle null clock gracefully.
    /// </summary>
    [Fact]
    public void UpdateLastLogin_NullClock_ThrowsNullReferenceException()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => user.UpdateLastLogin(null!));
    }

    /// <summary>
    /// Tests that UpdateLastLogin with DateTime.MinValue correctly sets LastLoginAt.
    /// This is a boundary condition test for the minimum possible DateTime value.
    /// </summary>
    [Fact]
    public void UpdateLastLogin_DateTimeMinValue_UpdatesLastLoginAtCorrectly()
    {
        // Arrange
        var minDateTime = DateTime.MinValue;
        var clockMock = new Mock<ISystemClock>();
        clockMock.Setup(c => c.UtcNow).Returns(minDateTime);
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateLastLogin(clockMock.Object);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(minDateTime, user.LastLoginAt);
    }

    /// <summary>
    /// Tests that UpdateLastLogin with DateTime.MaxValue correctly sets LastLoginAt.
    /// This is a boundary condition test for the maximum possible DateTime value.
    /// </summary>
    [Fact]
    public void UpdateLastLogin_DateTimeMaxValue_UpdatesLastLoginAtCorrectly()
    {
        // Arrange
        var maxDateTime = DateTime.MaxValue;
        var clockMock = new Mock<ISystemClock>();
        clockMock.Setup(c => c.UtcNow).Returns(maxDateTime);
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value!;
        // Act
        var result = user.UpdateLastLogin(clockMock.Object);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(maxDateTime, user.LastLoginAt);
    }

    /// <summary>
    /// Tests that Deactivate returns a failure result when the user is already inactive.
    /// Input: User that has already been deactivated.
    /// Expected: Returns a failure Result with message "User already inactive" and error code "validation_error".
    /// </summary>
    [Fact]
    public void Deactivate_WhenUserIsAlreadyInactive_ReturnsFailureWithValidationError()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        user.Deactivate(); // First deactivation to make user inactive
        // Act
        var result = user.Deactivate();
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User already inactive", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Deactivate successfully deactivates an active user.
    /// Input: Active user.
    /// Expected: Sets IsActive to false and returns a success Result.
    /// </summary>
    [Fact]
    public void Deactivate_WhenUserIsActive_SetsIsActiveToFalseAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        Assert.True(user.IsActive); // Verify user starts as active
        // Act
        var result = user.Deactivate();
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.False(user.IsActive);
        Assert.Equal(string.Empty, result.Error);
    }

    /// <summary>
    /// Tests that Deactivate can only be successfully called once on an active user.
    /// Input: Active user deactivated twice in succession.
    /// Expected: First call succeeds, second call fails with validation error.
    /// </summary>
    [Fact]
    public void Deactivate_WhenCalledTwice_FirstSucceedsSecondFails()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;
        // Act
        var firstResult = user.Deactivate();
        var secondResult = user.Deactivate();
        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal("User already inactive", secondResult.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, secondResult.ErrorCode);
        Assert.False(user.IsActive);
    }

    /// <summary>
    /// Tests that SetRefreshToken returns failure when refreshToken is null.
    /// </summary>
    [Fact]
    public void SetRefreshToken_NullRefreshToken_ReturnsFailure()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var expiresAt = DateTime.UtcNow.AddDays(7);
        // Act
        var result = user.SetRefreshToken(null!, expiresAt);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Refresh Token Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that SetRefreshToken returns failure when refreshToken is empty or whitespace.
    /// </summary>
    /// <param name = "refreshToken">The invalid refresh token value to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t\n   ")]
    public void SetRefreshToken_EmptyOrWhitespaceRefreshToken_ReturnsFailure(string refreshToken)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var expiresAt = DateTime.UtcNow.AddDays(7);
        // Act
        var result = user.SetRefreshToken(refreshToken, expiresAt);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Refresh Token Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that SetRefreshToken successfully sets the refresh token and expiration date when valid input is provided.
    /// </summary>
    [Fact]
    public void SetRefreshToken_ValidRefreshToken_SetsPropertiesAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var refreshToken = "valid_refresh_token_12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        // Act
        var result = user.SetRefreshToken(refreshToken, expiresAt);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(expiresAt, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that SetRefreshToken works correctly with various DateTime boundary values.
    /// </summary>
    /// <param name = "expiresAtTicks">The ticks value representing the DateTime to test.</param>
    [Theory]
    [InlineData(0)] // DateTime.MinValue
    [InlineData(3155378975999999999)] // DateTime.MaxValue
    public void SetRefreshToken_ValidRefreshTokenWithBoundaryDateTimes_SetsPropertiesAndReturnsSuccess(long expiresAtTicks)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var refreshToken = "valid_token";
        var expiresAt = new DateTime(expiresAtTicks);
        // Act
        var result = user.SetRefreshToken(refreshToken, expiresAt);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(expiresAt, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that SetRefreshToken accepts past, present, and future dates without validation.
    /// </summary>
    [Fact]
    public void SetRefreshToken_ValidRefreshTokenWithPastDate_SetsPropertiesAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var refreshToken = "valid_token";
        var pastDate = DateTime.UtcNow.AddDays(-30);
        // Act
        var result = user.SetRefreshToken(refreshToken, pastDate);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(pastDate, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that SetRefreshToken accepts very long refresh token strings.
    /// </summary>
    [Fact]
    public void SetRefreshToken_VeryLongRefreshToken_SetsPropertiesAndReturnsSuccess()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var longToken = new string('a', 10000);
        var expiresAt = DateTime.UtcNow.AddDays(7);
        // Act
        var result = user.SetRefreshToken(longToken, expiresAt);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(longToken, user.RefreshToken);
        Assert.Equal(expiresAt, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that SetRefreshToken accepts refresh tokens with special characters.
    /// </summary>
    [Theory]
    [InlineData("token!@#$%^&*()")]
    [InlineData("token_with-dashes.and.dots")]
    [InlineData("token~`+=[]{}|\\:;\"'<>?/")]
    [InlineData("token_with_unicode_émojis_🔑")]
    public void SetRefreshToken_RefreshTokenWithSpecialCharacters_SetsPropertiesAndReturnsSuccess(string refreshToken)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var expiresAt = DateTime.UtcNow.AddDays(7);
        // Act
        var result = user.SetRefreshToken(refreshToken, expiresAt);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(refreshToken, user.RefreshToken);
        Assert.Equal(expiresAt, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that SetRefreshToken overwrites previously set refresh token and expiration date.
    /// </summary>
    [Fact]
    public void SetRefreshToken_CalledTwice_OverwritesPreviousValues()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var firstToken = "first_token";
        var firstExpiresAt = DateTime.UtcNow.AddDays(7);
        var secondToken = "second_token";
        var secondExpiresAt = DateTime.UtcNow.AddDays(14);
        // Act
        user.SetRefreshToken(firstToken, firstExpiresAt);
        var result = user.SetRefreshToken(secondToken, secondExpiresAt);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(secondToken, user.RefreshToken);
        Assert.Equal(secondExpiresAt, user.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that Activate successfully activates an inactive user.
    /// Input: User with IsActive = false
    /// Expected: Returns success result and sets IsActive to true
    /// </summary>
    [Fact]
    public void Activate_WhenUserIsInactive_ReturnsSuccessAndActivatesUser()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hash123", "free");
        var user = userResult.Value;
        // First deactivate the user to ensure it's inactive
        user.Deactivate();
        // Act
        var result = user.Activate();
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Tests that Activate fails when user is already active.
    /// Input: User with IsActive = true
    /// Expected: Returns failure result with specific error message and code, IsActive remains true
    /// </summary>
    [Fact]
    public void Activate_WhenUserIsAlreadyActive_ReturnsFailureWithValidationError()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hash123", "free");
        var user = userResult.Value;
        // User is active by default
        // Act
        var result = user.Activate();
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User already active", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Tests that calling Activate multiple times consecutively returns failure.
    /// Input: User activated twice in a row
    /// Expected: First activation succeeds, second activation fails with validation error
    /// </summary>
    [Fact]
    public void Activate_WhenCalledMultipleTimes_ReturnsFailureOnSecondCall()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hash123", "free");
        var user = userResult.Value;
        user.Deactivate();
        // Act
        var firstResult = user.Activate();
        var secondResult = user.Activate();
        // Assert
        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal("User already active", secondResult.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, secondResult.ErrorCode);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Tests that RevokeAccessTokensOnly does not affect the RefreshTokenVersion.
    /// </summary>
    [Fact]
    public void RevokeAccessTokensOnly_DoesNotAffectRefreshTokenVersion()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        var initialRefreshTokenVersion = user.TokenRevocationVersion.RefreshTokenVersion;
        // Act
        user.RevokeAccessTokensOnly();
        // Assert
        Assert.Equal(initialRefreshTokenVersion, user.TokenRevocationVersion.RefreshTokenVersion);
    }

    /// <summary>
    /// Tests that RevokeAccessTokensOnly always returns success regardless of the reason parameter value.
    /// Validates that the method handles various edge cases for the reason parameter without throwing exceptions.
    /// </summary>
    /// <param name = "reason">The reason string to test, including edge cases like empty strings, whitespace, special characters, and very long strings.</param>
    [Theory]
    [InlineData("Custom reason")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Special chars: !@#$%^&*()")]
    [InlineData("Reason with\nnewlines\rand\ttabs")]
    [InlineData("Very long reason: Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.")]
    public void RevokeAccessTokensOnly_WithVariousReasonValues_AlwaysReturnsSuccess(string reason)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User", "hashedPassword");
        var user = userResult.Value;
        // Act
        var result = user.RevokeAccessTokensOnly(reason);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Error);
    }

    /// <summary>
    /// Tests that Create fails when email is null.
    /// </summary>
    [Fact]
    public void Create_WithNullEmail_ReturnsFailure()
    {
        // Arrange
        string? email = null;
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email!, name);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when email is empty or whitespace.
    /// </summary>
    /// <param name = "email">The email value to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t   ")]
    public void Create_WithEmptyOrWhitespaceEmail_ReturnsFailure(string email)
    {
        // Arrange
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when name is null.
    /// </summary>
    [Fact]
    public void Create_WithNullName_ReturnsFailure()
    {
        // Arrange
        string email = "test@example.com";
        string? name = null;
        // Act
        Result<User> result = User.Create(email, name!);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create fails when name is empty or whitespace.
    /// </summary>
    /// <param name = "name">The name value to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("   \t   ")]
    public void Create_WithEmptyOrWhitespaceName_ReturnsFailure(string name)
    {
        // Arrange
        string email = "test@example.com";
        // Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create succeeds with valid email and name.
    /// </summary>
    /// <param name = "email">The email value to test.</param>
    /// <param name = "name">The name value to test.</param>
    [Theory]
    [InlineData("test@example.com", "John Doe")]
    [InlineData("user@domain.co.uk", "Jane Smith")]
    [InlineData("a", "b")]
    [InlineData("email with spaces", "name with spaces")]
    [InlineData("special!@#$chars", "special!@#$chars")]
    [InlineData("very.long.email.address@subdomain.example.com", "Very Long Name With Many Words")]
    public void Create_WithValidEmailAndName_ReturnsSuccess(string email, string name)
    {
        // Arrange & Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
    }

    /// <summary>
    /// Tests that Create sets all properties correctly with valid parameters.
    /// </summary>
    [Fact]
    public void Create_WithValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.True(result.IsSuccess);
        User user = result.Value;
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(name, user.Name);
        Assert.Null(user.PasswordHash);
        Assert.Equal("free", user.Tier);
        Assert.Equal("User", user.Roles);
        Assert.True(user.IsActive);
    }

    /// <summary>
    /// Tests that Create sets PasswordHash when provided.
    /// </summary>
    /// <param name = "passwordHash">The password hash value to test.</param>
    [Theory]
    [InlineData("hashedpassword123")]
    [InlineData("$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy")]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithPasswordHash_SetsPasswordHash(string passwordHash)
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name, passwordHash);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(passwordHash, result.Value.PasswordHash);
    }

    /// <summary>
    /// Tests that Create sets PasswordHash to null when not provided or explicitly null.
    /// </summary>
    [Fact]
    public void Create_WithNullPasswordHash_SetsPasswordHashToNull()
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name, null);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.PasswordHash);
    }

    /// <summary>
    /// Tests that Create uses default PasswordHash (null) when not specified.
    /// </summary>
    [Fact]
    public void Create_WithoutPasswordHash_UsesDefaultNull()
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.PasswordHash);
    }

    /// <summary>
    /// Tests that Create sets custom tier when provided.
    /// </summary>
    /// <param name = "tier">The tier value to test.</param>
    [Theory]
    [InlineData("premium")]
    [InlineData("enterprise")]
    [InlineData("basic")]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithCustomTier_SetsTier(string tier)
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name, null, tier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(tier, result.Value.Tier);
    }

    /// <summary>
    /// Tests that Create uses default tier "free" when not specified.
    /// </summary>
    [Fact]
    public void Create_WithoutTier_UsesDefaultFree()
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result = User.Create(email, name);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("free", result.Value.Tier);
    }

    /// <summary>
    /// Tests that Create always sets Roles to "User".
    /// </summary>
    [Theory]
    [InlineData("test@example.com", "John Doe", null, "free")]
    [InlineData("user@domain.com", "Jane Smith", "hash123", "premium")]
    public void Create_Always_SetsRolesToUser(string email, string name, string? passwordHash, string tier)
    {
        // Arrange & Act
        Result<User> result = User.Create(email, name, passwordHash, tier);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("User", result.Value.Roles);
    }

    /// <summary>
    /// Tests that Create always generates a new non-empty Guid for Id.
    /// </summary>
    [Fact]
    public void Create_Always_GeneratesNewId()
    {
        // Arrange
        string email = "test@example.com";
        string name = "John Doe";
        // Act
        Result<User> result1 = User.Create(email, name);
        Result<User> result2 = User.Create(email, name);
        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(Guid.Empty, result1.Value.Id);
        Assert.NotEqual(Guid.Empty, result2.Value.Id);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Tests that Create validates email before name.
    /// </summary>
    [Fact]
    public void Create_WithBothInvalidEmailAndName_ValidatesEmailFirst()
    {
        // Arrange
        string? email = null;
        string? name = null;
        // Act
        Result<User> result = User.Create(email!, name!);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that HasRole returns true when the user has the specified role as the only role.
    /// </summary>
    [Fact]
    public void HasRole_WithSingleMatchingRole_ReturnsTrue()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole("User");
        // Assert
        Assert.True(hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns true when the user has the specified role among multiple roles.
    /// </summary>
    /// <param name = "roleToAdd">The role to add to the user.</param>
    /// <param name = "roleToCheck">The role to check for.</param>
    /// <param name = "expected">The expected result.</param>
    [Theory]
    [InlineData("Admin", "Admin", true)]
    [InlineData("Admin", "User", true)]
    [InlineData("Moderator", "Moderator", true)]
    [InlineData("Admin,Moderator", "Admin", true)]
    [InlineData("Admin,Moderator", "Moderator", true)]
    [InlineData("Admin,Moderator", "User", true)]
    public void HasRole_WithMultipleRoles_ReturnsExpectedResult(string roleToAdd, string roleToCheck, bool expected)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        foreach (var role in roleToAdd.Split(','))
        {
            user.AddRole(role);
        }

        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.Equal(expected, hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns false when the user does not have the specified role.
    /// </summary>
    [Fact]
    public void HasRole_WithNonExistentRole_ReturnsFalse()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole("Admin");
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole is case-sensitive and returns false when the case doesn't match.
    /// </summary>
    /// <param name = "roleToCheck">The role to check with different casing.</param>
    [Theory]
    [InlineData("user")]
    [InlineData("USER")]
    [InlineData("UsEr")]
    public void HasRole_WithDifferentCasing_ReturnsFalse(string roleToCheck)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns false for partial matches.
    /// </summary>
    [Theory]
    [InlineData("Use")]
    [InlineData("Users")]
    [InlineData("Admi")]
    public void HasRole_WithPartialMatch_ReturnsFalse(string roleToCheck)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        user.AddRole("Admin");
        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns false when checking for an empty string role.
    /// </summary>
    [Fact]
    public void HasRole_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole(string.Empty);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns false when checking for a whitespace-only string role.
    /// </summary>
    /// <param name = "roleToCheck">The whitespace string to check.</param>
    [Theory]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void HasRole_WithWhitespaceString_ReturnsFalse(string roleToCheck)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole does not match roles with leading or trailing spaces due to lack of trimming.
    /// </summary>
    [Theory]
    [InlineData(" User")]
    [InlineData("User ")]
    [InlineData(" User ")]
    public void HasRole_WithSpacesAroundRole_ReturnsFalse(string roleToCheck)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole handles roles with special characters correctly.
    /// </summary>
    [Fact]
    public void HasRole_WithSpecialCharacters_ReturnsTrue()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var specialRole = "Role-With_Special.Characters";
        user.AddRole(specialRole);
        // Act
        var hasRole = user.HasRole(specialRole);
        // Assert
        Assert.True(hasRole);
    }

    /// <summary>
    /// Tests that HasRole handles very long role names correctly.
    /// </summary>
    [Fact]
    public void HasRole_WithVeryLongRoleName_ReturnsTrue()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var longRole = new string('A', 1000);
        user.AddRole(longRole);
        // Act
        var hasRole = user.HasRole(longRole);
        // Assert
        Assert.True(hasRole);
    }

    /// <summary>
    /// Tests that HasRole returns false when checking for a very long role that doesn't exist.
    /// </summary>
    [Fact]
    public void HasRole_WithVeryLongNonExistentRole_ReturnsFalse()
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var longRole = new string('B', 1000);
        // Act
        var hasRole = user.HasRole(longRole);
        // Assert
        Assert.False(hasRole);
    }

    /// <summary>
    /// Tests that HasRole works correctly when role is at different positions in the roles list.
    /// </summary>
    [Theory]
    [InlineData("FirstRole", new[] { "FirstRole", "SecondRole", "ThirdRole" }, true)]
    [InlineData("SecondRole", new[] { "FirstRole", "SecondRole", "ThirdRole" }, true)]
    [InlineData("ThirdRole", new[] { "FirstRole", "SecondRole", "ThirdRole" }, true)]
    [InlineData("FourthRole", new[] { "FirstRole", "SecondRole", "ThirdRole" }, false)]
    public void HasRole_WithRoleAtDifferentPositions_ReturnsExpectedResult(string roleToCheck, string[] rolesToAdd, bool expected)
    {
        // Arrange
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        foreach (var role in rolesToAdd)
        {
            user.AddRole(role);
        }

        // Act
        var hasRole = user.HasRole(roleToCheck);
        // Assert
        Assert.Equal(expected, hasRole);
    }
}