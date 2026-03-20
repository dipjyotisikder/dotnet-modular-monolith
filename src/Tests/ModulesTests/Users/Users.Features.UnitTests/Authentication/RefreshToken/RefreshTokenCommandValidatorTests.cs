using Users.Features.Authentication.RefreshToken;

namespace Users.Features.UnitTests.Authentication.RefreshToken;

/// <summary>
/// Unit tests for <see cref="RefreshTokenCommandValidator"/>.
/// </summary>
public class RefreshTokenCommandValidatorTests
{
    /// <summary>
    /// Tests that validation passes when both RefreshToken and DeviceId are valid non-empty strings.
    /// </summary>
    [Fact]
    public void Constructor_ValidCommand_ValidationPasses()
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("valid-refresh-token", "valid-device-id");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Tests that validation fails when RefreshToken is null, empty, or whitespace.
    /// Verifies the correct error message is returned.
    /// </summary>
    /// <param name="refreshToken">The refresh token value to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_InvalidRefreshToken_ValidationFails(string? refreshToken)
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken!, "valid-device-id");

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("RefreshToken", result.Errors[0].PropertyName);
        Assert.Equal("Refresh token is required", result.Errors[0].ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when DeviceId is null, empty, or whitespace.
    /// Verifies the correct error message is returned.
    /// </summary>
    /// <param name="deviceId">The device ID value to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void Constructor_InvalidDeviceId_ValidationFails(string? deviceId)
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("valid-refresh-token", deviceId!);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("DeviceId", result.Errors[0].PropertyName);
        Assert.Equal("Device ID is required", result.Errors[0].ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when both RefreshToken and DeviceId are invalid.
    /// Verifies that both error messages are returned.
    /// </summary>
    /// <param name="refreshToken">The refresh token value to test.</param>
    /// <param name="deviceId">The device ID value to test.</param>
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("\t", "\n")]
    public void Constructor_BothPropertiesInvalid_ValidationFailsWithBothErrors(string? refreshToken, string? deviceId)
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken!, deviceId!);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken" && e.ErrorMessage == "Refresh token is required");
        Assert.Contains(result.Errors, e => e.PropertyName == "DeviceId" && e.ErrorMessage == "Device ID is required");
    }

    /// <summary>
    /// Tests that validation passes when RefreshToken and DeviceId contain special characters,
    /// as long as they are not empty or whitespace.
    /// </summary>
    /// <param name="refreshToken">The refresh token value to test.</param>
    /// <param name="deviceId">The device ID value to test.</param>
    [Theory]
    [InlineData("!@#$%^&*()", "special-chars-123")]
    [InlineData("token-with-dashes", "device_with_underscores")]
    [InlineData("a", "b")]
    [InlineData("very-long-refresh-token-that-exceeds-normal-length-expectations-but-is-still-valid", "very-long-device-id-that-exceeds-normal-length")]
    public void Constructor_ValidNonEmptyStringsWithSpecialCharacters_ValidationPasses(string refreshToken, string deviceId)
    {
        // Arrange
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken, deviceId);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}