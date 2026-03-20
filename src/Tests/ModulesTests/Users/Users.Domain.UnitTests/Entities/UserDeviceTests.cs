namespace Users.Domain.UnitTests.Entities;


public class UserDeviceTests
{
    /// <summary>
    /// Tests that IsRefreshTokenValid returns true when the device is not revoked and the refresh token has not expired.
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(1); // Future expiration

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when the device is revoked, even if the refresh token has not expired.
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenRevokedAndNotExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(1); // Future expiration

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;
        device.Revoke();

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when the refresh token has expired, even if the device is not revoked.
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenNotRevokedAndExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(-1); // Past expiration

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when both the device is revoked and the refresh token has expired.
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenRevokedAndExpired_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(-1); // Past expiration

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;
        device.Revoke();

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when the refresh token expires exactly at the current UTC time (boundary condition).
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenExpiresAtExactlyNow_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow; // Expires now

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns true when the refresh token expires just slightly in the future (boundary condition).
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenExpiresInNearFuture_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddMilliseconds(100); // Expires slightly in future

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when the refresh token expired just slightly in the past (boundary condition).
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenExpiredInNearPast_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddMilliseconds(-100); // Expired slightly in past

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns false when the refresh token expiration is at DateTime.MinValue (extreme past boundary).
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenExpiresAtMinValue_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.MinValue; // Extreme past

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests that IsRefreshTokenValid returns true when the refresh token expiration is at DateTime.MaxValue (extreme future boundary).
    /// </summary>
    [Fact]
    public void IsRefreshTokenValid_WhenExpiresAtMaxValue_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "valid-token";
        var refreshTokenExpiresAt = DateTime.MaxValue; // Extreme future

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = result.Value!;

        // Act
        var isValid = device.IsRefreshTokenValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests that Revoke successfully revokes a device with the default reason when the device is not already revoked.
    /// </summary>
    [Fact]
    public void Revoke_DeviceNotRevoked_SuccessWithDefaultReason()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = device.Revoke();
        var afterRevoke = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.NotNull(device.RevokedAt);
        Assert.InRange(device.RevokedAt.Value, beforeRevoke, afterRevoke);
        Assert.Equal("User initiated logout", device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke successfully revokes a device with a custom reason when the device is not already revoked.
    /// </summary>
    [Fact]
    public void Revoke_DeviceNotRevokedWithCustomReason_SuccessWithCustomReason()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var customReason = "Security concern detected";
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = device.Revoke(customReason);
        var afterRevoke = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.NotNull(device.RevokedAt);
        Assert.InRange(device.RevokedAt.Value, beforeRevoke, afterRevoke);
        Assert.Equal(customReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke returns a failure result when attempting to revoke an already revoked device.
    /// </summary>
    [Fact]
    public void Revoke_DeviceAlreadyRevoked_ReturnsFailure()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        device.Revoke("First revoke");

        // Act
        var result = device.Revoke("Second revoke");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Device already revoked", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Revoke correctly handles an empty string reason.
    /// </summary>
    [Fact]
    public void Revoke_EmptyStringReason_SuccessWithEmptyReason()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;

        // Act
        var result = device.Revoke(string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(string.Empty, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke correctly handles a whitespace-only string reason.
    /// </summary>
    [Fact]
    public void Revoke_WhitespaceReason_SuccessWithWhitespaceReason()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var whitespaceReason = "   ";

        // Act
        var result = device.Revoke(whitespaceReason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(whitespaceReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke correctly handles a very long string reason.
    /// </summary>
    [Fact]
    public void Revoke_VeryLongReason_SuccessWithLongReason()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var longReason = new string('A', 10000);

        // Act
        var result = device.Revoke(longReason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(longReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke correctly handles a reason string with special characters.
    /// </summary>
    [Fact]
    public void Revoke_ReasonWithSpecialCharacters_SuccessWithSpecialCharacters()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var specialReason = "Revoked due to: <script>alert('XSS')</script> & other \"special\" characters!@#$%^&*()";

        // Act
        var result = device.Revoke(specialReason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(specialReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke correctly handles a reason string with control characters.
    /// </summary>
    [Fact]
    public void Revoke_ReasonWithControlCharacters_SuccessWithControlCharacters()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var controlCharReason = "Revoked\nWith\tControl\rCharacters";

        // Act
        var result = device.Revoke(controlCharReason);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(controlCharReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke preserves the original revoke reason and timestamp when attempting to revoke an already revoked device.
    /// </summary>
    [Fact]
    public void Revoke_DeviceAlreadyRevoked_PreservesOriginalRevokeDetails()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var firstReason = "First revoke reason";
        device.Revoke(firstReason);
        var firstRevokedAt = device.RevokedAt;
        var firstRevokeReason = device.RevokeReason;

        // Act
        var result = device.Revoke("Second revoke reason");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(firstRevokedAt, device.RevokedAt);
        Assert.Equal(firstRevokeReason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Revoke with null reason parameter handles the null value appropriately (defensive test).
    /// </summary>
    [Fact]
    public void Revoke_NullReason_HandlesNullValue()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;

        // Act
        var result = device.Revoke(null!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Null(device.RevokeReason);
    }

    /// <summary>
    /// Tests that multiple parameters are correctly set when revoking a device for the first time.
    /// </summary>
    [Fact]
    public void Revoke_FirstRevoke_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var createResult = UserDevice.Create(
            Guid.NewGuid(),
            "device123",
            "My Device",
            "refresh_token",
            DateTime.UtcNow.AddDays(7));
        var device = createResult.Value!;
        var reason = "Test reason";
        var beforeRevoke = DateTime.UtcNow;

        // Act
        var result = device.Revoke(reason);
        var afterRevoke = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.NotNull(device.RevokedAt);
        Assert.InRange(device.RevokedAt.Value, beforeRevoke, afterRevoke);
        Assert.Equal(reason, device.RevokeReason);
    }

    /// <summary>
    /// Tests that Create returns failure when userId is Guid.Empty.
    /// </summary>
    [Fact]
    public void Create_UserIdIsEmpty_ReturnsFailureWithValidationError()
    {
        // Arrange
        var userId = Guid.Empty;
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User Id Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns failure when deviceId is null, empty, or whitespace.
    /// </summary>
    /// <param name="deviceId">The device ID to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Create_DeviceIdIsNullOrWhitespace_ReturnsFailureWithValidationError(string? deviceId)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId!, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Device Id Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns failure when deviceName is null, empty, or whitespace.
    /// </summary>
    /// <param name="deviceName">The device name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Create_DeviceNameIsNullOrWhitespace_ReturnsFailureWithValidationError(string? deviceName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName!, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Device Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create returns failure when refreshToken is null, empty, or whitespace.
    /// </summary>
    /// <param name="refreshToken">The refresh token to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Create_RefreshTokenIsNullOrWhitespace_ReturnsFailureWithValidationError(string? refreshToken)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken!, refreshTokenExpiresAt);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Refresh Token Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Create successfully creates a UserDevice with all required parameters.
    /// </summary>
    [Fact]
    public void Create_ValidRequiredParameters_ReturnsSuccessWithUserDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(deviceId, result.Value.DeviceId);
        Assert.Equal(deviceName, result.Value.DeviceName);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        Assert.Equal(refreshTokenExpiresAt, result.Value.RefreshTokenExpiresAt);
        Assert.Null(result.Value.DeviceType);
        Assert.Null(result.Value.IpAddress);
        Assert.InRange(result.Value.IssuedAt, beforeCreation, afterCreation);
        Assert.InRange(result.Value.LastActivityAt, beforeCreation, afterCreation);
        Assert.False(result.Value.IsRevoked);
        Assert.Null(result.Value.RevokedAt);
        Assert.Null(result.Value.RevokeReason);
    }

    /// <summary>
    /// Tests that Create successfully creates a UserDevice with all parameters including optional ones.
    /// </summary>
    [Fact]
    public void Create_ValidAllParameters_ReturnsSuccessWithUserDeviceIncludingOptionalFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        var deviceType = "Mobile";
        var ipAddress = "192.168.1.1";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt, deviceType, ipAddress);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(deviceId, result.Value.DeviceId);
        Assert.Equal(deviceName, result.Value.DeviceName);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        Assert.Equal(refreshTokenExpiresAt, result.Value.RefreshTokenExpiresAt);
        Assert.Equal(deviceType, result.Value.DeviceType);
        Assert.Equal(ipAddress, result.Value.IpAddress);
        Assert.InRange(result.Value.IssuedAt, beforeCreation, afterCreation);
        Assert.InRange(result.Value.LastActivityAt, beforeCreation, afterCreation);
        Assert.False(result.Value.IsRevoked);
    }

    /// <summary>
    /// Tests that Create accepts null for optional deviceType parameter.
    /// </summary>
    [Fact]
    public void Create_DeviceTypeIsNull_ReturnsSuccessWithNullDeviceType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt, deviceType: null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.DeviceType);
    }

    /// <summary>
    /// Tests that Create accepts null for optional ipAddress parameter.
    /// </summary>
    [Fact]
    public void Create_IpAddressIsNull_ReturnsSuccessWithNullIpAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt, ipAddress: null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.IpAddress);
    }

    /// <summary>
    /// Tests that Create accepts empty string for optional deviceType parameter.
    /// </summary>
    [Fact]
    public void Create_DeviceTypeIsEmpty_ReturnsSuccessWithEmptyDeviceType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt, deviceType: string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.DeviceType);
    }

    /// <summary>
    /// Tests that Create accepts empty string for optional ipAddress parameter.
    /// </summary>
    [Fact]
    public void Create_IpAddressIsEmpty_ReturnsSuccessWithEmptyIpAddress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt, ipAddress: string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.IpAddress);
    }

    /// <summary>
    /// Tests that Create handles various DateTime values for refreshTokenExpiresAt parameter.
    /// </summary>
    /// <param name="testCase">Description of the test case.</param>
    /// <param name="daysOffset">Offset in days from UtcNow to create the test DateTime.</param>
    [Theory]
    [InlineData("Past date", -30)]
    [InlineData("Current date", 0)]
    [InlineData("Future date", 30)]
    public void Create_VariousRefreshTokenExpiresAtValues_ReturnsSuccess(string testCase, int daysOffset)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(daysOffset);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(refreshTokenExpiresAt, result.Value.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that Create handles DateTime.MinValue for refreshTokenExpiresAt parameter.
    /// </summary>
    [Fact]
    public void Create_RefreshTokenExpiresAtIsMinValue_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.MinValue;

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DateTime.MinValue, result.Value.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that Create handles DateTime.MaxValue for refreshTokenExpiresAt parameter.
    /// </summary>
    [Fact]
    public void Create_RefreshTokenExpiresAtIsMaxValue_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.MaxValue;

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DateTime.MaxValue, result.Value.RefreshTokenExpiresAt);
    }

    /// <summary>
    /// Tests that Create accepts very long strings for required string parameters.
    /// </summary>
    [Fact]
    public void Create_VeryLongStrings_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = new string('a', 10000);
        var deviceName = new string('b', 10000);
        var refreshToken = new string('c', 10000);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(deviceId, result.Value.DeviceId);
        Assert.Equal(deviceName, result.Value.DeviceName);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
    }

    /// <summary>
    /// Tests that Create accepts strings with special characters.
    /// </summary>
    [Fact]
    public void Create_StringsWithSpecialCharacters_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device!@#$%^&*()_+-={}[]|:;<>?,./";
        var deviceName = "My Device 中文 🔥";
        var refreshToken = "token\t\n\r\0";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(deviceId, result.Value.DeviceId);
        Assert.Equal(deviceName, result.Value.DeviceName);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
    }

    /// <summary>
    /// Tests that Create generates unique IDs for each UserDevice instance.
    /// </summary>
    [Fact]
    public void Create_CalledMultipleTimes_GeneratesUniqueIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "My Device";
        var refreshToken = "token123";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var result1 = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var result2 = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.NotEqual(result1.Value.Id, result2.Value.Id);
    }

    /// <summary>
    /// Tests that UpdateLastActivity returns success and updates LastActivityAt
    /// when the device is not revoked.
    /// </summary>
    [Fact]
    public void UpdateLastActivity_WhenDeviceNotRevoked_ReturnsSuccessAndUpdatesLastActivityAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh_token_value";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var createResult = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = createResult.Value!;
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = device.UpdateLastActivity();
        var afterUpdate = DateTime.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(device.LastActivityAt >= beforeUpdate);
        Assert.True(device.LastActivityAt <= afterUpdate);
    }

    /// <summary>
    /// Tests that UpdateLastActivity returns failure with correct error message and code
    /// when the device is revoked, and does not update LastActivityAt.
    /// </summary>
    [Fact]
    public void UpdateLastActivity_WhenDeviceIsRevoked_ReturnsFailureAndDoesNotUpdateLastActivityAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh_token_value";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var createResult = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = createResult.Value!;

        device.Revoke("Test revoke reason");
        var lastActivityBeforeUpdate = device.LastActivityAt;

        // Act
        var result = device.UpdateLastActivity();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Cannot update revoked device", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        Assert.Equal(lastActivityBeforeUpdate, device.LastActivityAt);
    }

    /// <summary>
    /// Tests that UpdateLastActivity returns failure when device is already revoked,
    /// regardless of the revoke reason.
    /// </summary>
    [Theory]
    [InlineData("User initiated logout")]
    [InlineData("Security breach")]
    [InlineData("Device stolen")]
    [InlineData("")]
    public void UpdateLastActivity_WhenDeviceRevokedWithDifferentReasons_ReturnsFailure(string revokeReason)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh_token_value";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var createResult = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = createResult.Value!;

        device.Revoke(revokeReason);

        // Act
        var result = device.UpdateLastActivity();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot update revoked device", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that UpdateLastActivity can be called multiple times on a non-revoked device
    /// and successfully updates LastActivityAt each time.
    /// </summary>
    [Fact]
    public void UpdateLastActivity_WhenCalledMultipleTimesOnNonRevokedDevice_SuccessfullyUpdatesEachTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh_token_value";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var createResult = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var device = createResult.Value!;

        // Act
        var result1 = device.UpdateLastActivity();
        var lastActivityAfterFirst = device.LastActivityAt;

        System.Threading.Thread.Sleep(10);

        var result2 = device.UpdateLastActivity();
        var lastActivityAfterSecond = device.LastActivityAt;

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(lastActivityAfterSecond >= lastActivityAfterFirst);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns false when the refresh token expiration time is in the future.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsInFuture_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(1);

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.False(isExpired);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns true when the refresh token expiration time is in the past.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsInPast_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddHours(-1);

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.True(isExpired);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns true when the refresh token expiration time is DateTime.MinValue.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsMinValue_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.MinValue;

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.True(isExpired);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns false when the refresh token expiration time is DateTime.MaxValue.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsMaxValue_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.MaxValue;

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.False(isExpired);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns true when the refresh token expiration time is far in the past.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsFarInPast_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddYears(-10);

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.True(isExpired);
    }

    /// <summary>
    /// Tests that IsRefreshTokenExpired returns false when the refresh token expiration time is far in the future.
    /// </summary>
    [Fact]
    public void IsRefreshTokenExpired_WhenExpirationTimeIsFarInFuture_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device123";
        var deviceName = "Test Device";
        var refreshToken = "refresh-token";
        var refreshTokenExpiresAt = DateTime.UtcNow.AddYears(10);

        var result = UserDevice.Create(userId, deviceId, deviceName, refreshToken, refreshTokenExpiresAt);
        var userDevice = result.Value!;

        // Act
        var isExpired = userDevice.IsRefreshTokenExpired();

        // Assert
        Assert.False(isExpired);
    }
}