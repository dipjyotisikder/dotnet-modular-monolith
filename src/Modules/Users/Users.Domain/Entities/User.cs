using Shared.Domain;
using Shared.Domain.Services;
using System.Text.Json;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string Tier { get; private set; } = "free";
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string Roles { get; private set; } = "User";
    public string? OAuthProvider { get; private set; }
    public string? OAuthProviderId { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    private string _tokenRevocationVersionJson = "{}";
    public DateTime? LastPasswordChangedAt { get; private set; }

    public TokenRevocationVersion TokenRevocationVersion
    {
        get
        {
            if (string.IsNullOrEmpty(_tokenRevocationVersionJson) || _tokenRevocationVersionJson == "{}")
                return new TokenRevocationVersion();
            return JsonSerializer.Deserialize<TokenRevocationVersion>(_tokenRevocationVersionJson) ?? new TokenRevocationVersion();
        }
        private set
        {
            _tokenRevocationVersionJson = JsonSerializer.Serialize(value);
        }
    }

    private User() { }

    public static Result<User> Create(string email, string name, string? passwordHash = null, string tier = "free")
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>("Email Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>("Name Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            PasswordHash = passwordHash,
            Tier = tier,
            Roles = "User"
        };

        return Result.Success(user);
    }

    public static Result<User> CreateWithOAuth(string email, string name, string oauthProvider, string oauthProviderId, string tier = "free")
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<User>("Email Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>("Name Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(oauthProvider))
            return Result.Failure<User>("OAuth Provider Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Name = name,
            Tier = tier,
            OAuthProvider = oauthProvider,
            OAuthProviderId = oauthProviderId,
            Roles = "User"
        };

        return Result.Success(user);
    }

    public Result UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure("Password Hash Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        PasswordHash = passwordHash;
        LastPasswordChangedAt = DateTime.UtcNow;
        RevokeAllCurrentTokens();

        return Result.Success();
    }

    public Result UpdateTier(string tier)
    {
        if (string.IsNullOrWhiteSpace(tier))
            return Result.Failure("Tier Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        Tier = tier;
        return Result.Success();
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Name Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        Name = name;
        return Result.Success();
    }

    public Result UpdateLastLogin(ISystemClock clock)
    {
        LastLoginAt = clock.UtcNow;
        return Result.Success();
    }

    public Result SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure("Refresh Token Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;

        return Result.Success();
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public Result RemoveRole(string role)
    {
        var roles = Roles.Split(',').ToList();
        roles.Remove(role);
        Roles = string.Join(",", roles.Any() ? roles : new List<string> { "User" });
        RevokeAllCurrentTokens();

        return Result.Success();
    }

    public Result AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return Result.Failure("Role Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        var roles = Roles.Split(',').ToList();
        if (!roles.Contains(role))
        {
            roles.Add(role);
            Roles = string.Join(",", roles);
            RevokeAllCurrentTokens();
        }

        return Result.Success();
    }

    public bool HasRole(string role)
    {
        return Roles.Split(',').Contains(role);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("User already inactive", ErrorCodes.VALIDATION_ERROR);

        IsActive = false;
        RevokeAllCurrentTokens();
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("User already active", ErrorCodes.VALIDATION_ERROR);

        IsActive = true;
        return Result.Success();
    }

    public List<string> GetRoles()
    {
        return Roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    public void RevokeAccessTokens()
    {
        var version = TokenRevocationVersion;
        version.RevokeAccessTokens();
        TokenRevocationVersion = version;
    }

    public void RevokeRefreshTokens()
    {
        var version = TokenRevocationVersion;
        version.RevokeRefreshTokens();
        TokenRevocationVersion = version;
    }

    public void RevokeAllCurrentTokens()
    {
        var version = TokenRevocationVersion;
        version.RevokeAllTokens();
        TokenRevocationVersion = version;
    }

    public Result RevokeAllTokens(string reason = "Admin revocation")
    {
        RevokeAllCurrentTokens();
        return Result.Success();
    }

    public Result RevokeAccessTokensOnly(string reason = "Permission change")
    {
        RevokeAccessTokens();
        return Result.Success();
    }
}
