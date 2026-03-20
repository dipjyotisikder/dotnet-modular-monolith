using Users.Domain.ValueObjects;

namespace Users.Domain.UnitTests.ValueObjects;

public class DeviceInfoTests
{
    [Fact]
    public void DeviceInfo_WithAllParameters_AssignsAllPropertiesCorrectly()
    {
        var deviceId = "device-123";
        var deviceName = "My Phone";
        var deviceType = "Mobile";
        var ipAddress = "192.168.1.1";

        var deviceInfo = new DeviceInfo(deviceId, deviceName, deviceType, ipAddress);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Equal(deviceType, deviceInfo.DeviceType);
        Assert.Equal(ipAddress, deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_WithOnlyRequiredParameters_SetsOptionalParametersToNull()
    {
        var deviceId = "device-456";
        var deviceName = "My Tablet";

        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_WithExplicitNullOptionalParameters_AssignsNullToProperties()
    {
        var deviceId = "device-789";
        var deviceName = "My Laptop";

        var deviceInfo = new DeviceInfo(deviceId, deviceName, null, null);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_Constructor_SetsIssuedAtAndLastActivityAtToUtcNow()
    {
        var beforeCreation = DateTime.UtcNow;
        var deviceId = "device-001";
        var deviceName = "Test Device";

        var deviceInfo = new DeviceInfo(deviceId, deviceName);
        var afterCreation = DateTime.UtcNow;

        Assert.InRange(deviceInfo.IssuedAt, beforeCreation, afterCreation);
        Assert.InRange(deviceInfo.LastActivityAt, beforeCreation, afterCreation);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("", "validName")]
    [InlineData("validId", "")]
    public void DeviceInfo_WithEmptyRequiredStrings_AssignsEmptyStrings(string deviceId, string deviceName)
    {
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    [Theory]
    [InlineData("   ", "validName")]
    [InlineData("validId", "   ")]
    [InlineData("\t", "\n")]
    [InlineData(" \t\r\n ", " \t\r\n ")]
    public void DeviceInfo_WithWhitespaceRequiredStrings_AssignsWhitespaceStrings(string deviceId, string deviceName)
    {
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    [Fact]
    public void DeviceInfo_WithEmptyOptionalStrings_AssignsEmptyStrings()
    {
        var deviceId = "device-123";
        var deviceName = "My Device";

        var deviceInfo = new DeviceInfo(deviceId, deviceName, "", "");

        Assert.Equal("", deviceInfo.DeviceType);
        Assert.Equal("", deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_WithWhitespaceOptionalStrings_AssignsWhitespaceStrings()
    {
        var deviceId = "device-123";
        var deviceName = "My Device";

        var deviceInfo = new DeviceInfo(deviceId, deviceName, "   ", "\t\n");

        Assert.Equal("   ", deviceInfo.DeviceType);
        Assert.Equal("\t\n", deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_WithVeryLongStrings_AssignsLongStringsCorrectly()
    {
        var longDeviceId = new string('A', 10000);
        var longDeviceName = new string('B', 10000);
        var longDeviceType = new string('C', 10000);
        var longIpAddress = new string('D', 10000);

        var deviceInfo = new DeviceInfo(longDeviceId, longDeviceName, longDeviceType, longIpAddress);

        Assert.Equal(longDeviceId, deviceInfo.DeviceId);
        Assert.Equal(longDeviceName, deviceInfo.DeviceName);
        Assert.Equal(longDeviceType, deviceInfo.DeviceType);
        Assert.Equal(longIpAddress, deviceInfo.IpAddress);
    }

    [Theory]
    [InlineData("device<>123", "name\"with'quotes")]
    [InlineData("device\u0000null", "name\u0001control")]
    [InlineData("device🚀emoji", "name中文字符")]
    [InlineData("device\r\nlinebreak", "name\ttab")]
    [InlineData("device\\path\\like", "name/slash/like")]
    public void DeviceInfo_WithSpecialCharacters_AssignsSpecialCharactersCorrectly(string deviceId, string deviceName)
    {
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    [Fact]
    public void DeviceInfo_WithSpecialCharactersInOptionalParameters_AssignsCorrectly()
    {
        var deviceId = "device-123";
        var deviceName = "My Device";
        var deviceType = "Type<>&\"'";
        var ipAddress = "192.168.1.1\u0000";

        var deviceInfo = new DeviceInfo(deviceId, deviceName, deviceType, ipAddress);

        Assert.Equal(deviceType, deviceInfo.DeviceType);
        Assert.Equal(ipAddress, deviceInfo.IpAddress);
    }

    [Fact]
    public void DeviceInfo_ParameterlessConstructor_InitializesPropertiesWithDefaultValues()
    {
        var deviceInfo = new DeviceInfo();

        Assert.NotNull(deviceInfo);
        Assert.Equal(string.Empty, deviceInfo.DeviceId);
        Assert.Equal(string.Empty, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
        Assert.Equal(DateTime.MinValue, deviceInfo.LastActivityAt);
        Assert.Equal(DateTime.MinValue, deviceInfo.IssuedAt);
    }
}
