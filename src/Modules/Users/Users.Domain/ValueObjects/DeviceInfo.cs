namespace Users.Domain.ValueObjects;

public class DeviceInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? DeviceType { get; set; }
    public string? IpAddress { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime IssuedAt { get; set; }

    public DeviceInfo() { }

    public DeviceInfo(string deviceId, string deviceName, string? deviceType = null, string? ipAddress = null)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        IpAddress = ipAddress;
        IssuedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }
}
