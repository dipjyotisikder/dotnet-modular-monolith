using Xunit;
using Users.Domain.ValueObjects;

namespace Users.Domain.UnitTests.ValueObjects;

public class DeviceInfoTests
{
    /// <summary>
    /// Tests that the constructor properly assigns all provided parameters to their respective properties
    /// when all parameters (both required and optional) are provided with valid non-null values.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithAllParameters_AssignsAllPropertiesCorrectly()
    {
        // Arrange
        var deviceId = "device-123";
        var deviceName = "My Phone";
        var deviceType = "Mobile";
        var ipAddress = "192.168.1.1";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName, deviceType, ipAddress);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Equal(deviceType, deviceInfo.DeviceType);
        Assert.Equal(ipAddress, deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor properly assigns required parameters and sets optional parameters to null
    /// when only required parameters are provided (optional parameters use their default values).
    /// </summary>
    [Fact]
    public void DeviceInfo_WithOnlyRequiredParameters_SetsOptionalParametersToNull()
    {
        // Arrange
        var deviceId = "device-456";
        var deviceName = "My Tablet";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor properly handles null values explicitly passed to optional parameters.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithExplicitNullOptionalParameters_AssignsNullToProperties()
    {
        // Arrange
        var deviceId = "device-789";
        var deviceName = "My Laptop";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName, null, null);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor sets IssuedAt and LastActivityAt to the current UTC time.
    /// Verifies that both timestamps are set to approximately DateTime.UtcNow within a reasonable tolerance.
    /// </summary>
    [Fact]
    public void DeviceInfo_Constructor_SetsIssuedAtAndLastActivityAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var deviceId = "device-001";
        var deviceName = "Test Device";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.InRange(deviceInfo.IssuedAt, beforeCreation, afterCreation);
        Assert.InRange(deviceInfo.LastActivityAt, beforeCreation, afterCreation);
    }

    /// <summary>
    /// Tests that the constructor accepts empty strings for required parameters without throwing exceptions.
    /// Verifies that empty strings are properly assigned to the properties.
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("", "validName")]
    [InlineData("validId", "")]
    public void DeviceInfo_WithEmptyRequiredStrings_AssignsEmptyStrings(string deviceId, string deviceName)
    {
        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    /// <summary>
    /// Tests that the constructor accepts whitespace-only strings for required parameters.
    /// Verifies that whitespace strings are properly assigned without trimming or validation.
    /// </summary>
    [Theory]
    [InlineData("   ", "validName")]
    [InlineData("validId", "   ")]
    [InlineData("\t", "\n")]
    [InlineData(" \t\r\n ", " \t\r\n ")]
    public void DeviceInfo_WithWhitespaceRequiredStrings_AssignsWhitespaceStrings(string deviceId, string deviceName)
    {
        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    /// <summary>
    /// Tests that the constructor accepts empty strings for optional parameters.
    /// Verifies that empty strings are properly assigned to optional properties.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithEmptyOptionalStrings_AssignsEmptyStrings()
    {
        // Arrange
        var deviceId = "device-123";
        var deviceName = "My Device";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName, "", "");

        // Assert
        Assert.Equal("", deviceInfo.DeviceType);
        Assert.Equal("", deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor accepts whitespace-only strings for optional parameters.
    /// Verifies that whitespace strings are properly assigned without trimming.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithWhitespaceOptionalStrings_AssignsWhitespaceStrings()
    {
        // Arrange
        var deviceId = "device-123";
        var deviceName = "My Device";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName, "   ", "\t\n");

        // Assert
        Assert.Equal("   ", deviceInfo.DeviceType);
        Assert.Equal("\t\n", deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor properly handles very long strings for all parameters.
    /// Verifies that long strings are assigned without truncation or errors.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithVeryLongStrings_AssignsLongStringsCorrectly()
    {
        // Arrange
        var longDeviceId = new string('A', 10000);
        var longDeviceName = new string('B', 10000);
        var longDeviceType = new string('C', 10000);
        var longIpAddress = new string('D', 10000);

        // Act
        var deviceInfo = new DeviceInfo(longDeviceId, longDeviceName, longDeviceType, longIpAddress);

        // Assert
        Assert.Equal(longDeviceId, deviceInfo.DeviceId);
        Assert.Equal(longDeviceName, deviceInfo.DeviceName);
        Assert.Equal(longDeviceType, deviceInfo.DeviceType);
        Assert.Equal(longIpAddress, deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the constructor properly handles strings containing special and control characters.
    /// Verifies that special characters are preserved without sanitization or validation.
    /// </summary>
    [Theory]
    [InlineData("device<>123", "name\"with'quotes")]
    [InlineData("device\u0000null", "name\u0001control")]
    [InlineData("device🚀emoji", "name中文字符")]
    [InlineData("device\r\nlinebreak", "name\ttab")]
    [InlineData("device\\path\\like", "name/slash/like")]
    public void DeviceInfo_WithSpecialCharacters_AssignsSpecialCharactersCorrectly(string deviceId, string deviceName)
    {
        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName);

        // Assert
        Assert.Equal(deviceId, deviceInfo.DeviceId);
        Assert.Equal(deviceName, deviceInfo.DeviceName);
    }

    /// <summary>
    /// Tests that the constructor properly handles special characters in optional parameters.
    /// Verifies that special characters are preserved in optional string parameters.
    /// </summary>
    [Fact]
    public void DeviceInfo_WithSpecialCharactersInOptionalParameters_AssignsCorrectly()
    {
        // Arrange
        var deviceId = "device-123";
        var deviceName = "My Device";
        var deviceType = "Type<>&\"'";
        var ipAddress = "192.168.1.1\u0000";

        // Act
        var deviceInfo = new DeviceInfo(deviceId, deviceName, deviceType, ipAddress);

        // Assert
        Assert.Equal(deviceType, deviceInfo.DeviceType);
        Assert.Equal(ipAddress, deviceInfo.IpAddress);
    }

    /// <summary>
    /// Tests that the parameterless constructor initializes all properties to their expected default values.
    /// DeviceId and DeviceName should be empty strings, nullable properties should be null,
    /// and DateTime properties should be their default value (DateTime.MinValue).
    /// </summary>
    [Fact]
    public void DeviceInfo_ParameterlessConstructor_InitializesPropertiesWithDefaultValues()
    {
        // Arrange & Act
        var deviceInfo = new DeviceInfo();

        // Assert
        Assert.NotNull(deviceInfo);
        Assert.Equal(string.Empty, deviceInfo.DeviceId);
        Assert.Equal(string.Empty, deviceInfo.DeviceName);
        Assert.Null(deviceInfo.DeviceType);
        Assert.Null(deviceInfo.IpAddress);
        Assert.Equal(DateTime.MinValue, deviceInfo.LastActivityAt);
        Assert.Equal(DateTime.MinValue, deviceInfo.IssuedAt);
    }
}