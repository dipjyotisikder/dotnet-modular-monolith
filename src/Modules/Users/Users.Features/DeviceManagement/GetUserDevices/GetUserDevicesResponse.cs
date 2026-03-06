namespace Users.Features.DeviceManagement.GetUserDevices;

public record GetUserDevicesResponse(
    string DeviceId,
    string DeviceName,
    string? DeviceType,
    string? IpAddress,
    DateTime IssuedAt,
    DateTime LastActivityAt,
    bool IsRevoked,
    string? RevokeReason);
