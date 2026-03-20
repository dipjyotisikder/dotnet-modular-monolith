using Users.Features.Authentication.RefreshToken;

namespace Users.Features.UnitTests.Authentication.RefreshToken;

public class RefreshTokenCommandValidatorTests
{
    [Fact]
    public void Constructor_ValidCommand_ValidationPasses()
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("valid-refresh-token", "valid-device-id");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

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
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken!, "valid-device-id");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("RefreshToken", result.Errors[0].PropertyName);
        Assert.Equal("Refresh token is required", result.Errors[0].ErrorMessage);
    }

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
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("valid-refresh-token", deviceId!);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("DeviceId", result.Errors[0].PropertyName);
        Assert.Equal("Device ID is required", result.Errors[0].ErrorMessage);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("\t", "\n")]
    public void Constructor_BothPropertiesInvalid_ValidationFailsWithBothErrors(string? refreshToken, string? deviceId)
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken!, deviceId!);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken" && e.ErrorMessage == "Refresh token is required");
        Assert.Contains(result.Errors, e => e.PropertyName == "DeviceId" && e.ErrorMessage == "Device ID is required");
    }

    [Theory]
    [InlineData("!@#$%^&*()", "special-chars-123")]
    [InlineData("token-with-dashes", "device_with_underscores")]
    [InlineData("a", "b")]
    [InlineData("very-long-refresh-token-that-exceeds-normal-length-expectations-but-is-still-valid", "very-long-device-id-that-exceeds-normal-length")]
    public void Constructor_ValidNonEmptyStringsWithSpecialCharacters_ValidationPasses(string refreshToken, string deviceId)
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(refreshToken, deviceId);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
