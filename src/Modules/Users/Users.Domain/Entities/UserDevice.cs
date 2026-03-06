using Shared.Domain;

namespace Users.Domain.Entities;

public class UserDevice
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string DeviceName { get; private set; } = string.Empty;
    public string? DeviceType { get; set; }
    public string? IpAddress { get; set; }
    public string RefreshToken { get; private set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokeReason { get; private set; }

    private UserDevice() { }

    public static Result<UserDevice> Create(
        Guid userId,
        string deviceId,
        string deviceName,
        string refreshToken,
        DateTime refreshTokenExpiresAt,
        string? deviceType = null,
        string? ipAddress = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<UserDevice>("User Id Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(deviceId))
            return Result.Failure<UserDevice>("Device Id Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(deviceName))
            return Result.Failure<UserDevice>("Device Name Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Failure<UserDevice>("Refresh Token Cannot Be Empty", ErrorCodes.VALIDATION_ERROR);

        var device = new UserDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            DeviceType = deviceType,
            IpAddress = ipAddress,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            IssuedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false
        };

        return Result.Success(device);
    }

    public Result Revoke(string reason = "User initiated logout")
    {
        if (IsRevoked)
            return Result.Failure("Device already revoked", ErrorCodes.VALIDATION_ERROR);

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokeReason = reason;

        return Result.Success();
    }

    public Result UpdateLastActivity()
    {
        if (IsRevoked)
            return Result.Failure("Cannot update revoked device", ErrorCodes.VALIDATION_ERROR);

        LastActivityAt = DateTime.UtcNow;
        return Result.Success();
    }

    public bool IsRefreshTokenExpired()
    {
        return DateTime.UtcNow > RefreshTokenExpiresAt;
    }

    public bool IsRefreshTokenValid()
    {
        return !IsRevoked && !IsRefreshTokenExpired();
    }
}
