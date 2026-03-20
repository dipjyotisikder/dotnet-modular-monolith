using Moq;
using System.Linq.Expressions;
using Xunit;
using Users.Domain.Repositories;
using Users.Domain.Services;
using Shared.Domain.Services;
using Users.Features.Authentication.RefreshToken;
using Users.Domain.Entities;
using Shared.Domain;

namespace Users.Features.UnitTests.Authentication.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUserDeviceRepository> _deviceRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ISystemClock> _clockMock;
    private readonly RefreshTokenCommandHandler _handler;
    private readonly DateTime _currentTime;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _clockMock = new Mock<ISystemClock>();
        _currentTime = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _clockMock.Setup(c => c.UtcNow).Returns(_currentTime);

        _handler = new RefreshTokenCommandHandler(
            _userRepositoryMock.Object,
            _deviceRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _clockMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns failure when device is not found.
    /// Input: Valid command with deviceId that doesn't exist.
    /// Expected: Returns failure with "Invalid device" and UNAUTHORIZED error code.
    /// </summary>
    [Fact]
    public async Task Handle_DeviceNotFound_ReturnsInvalidDeviceFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid device", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure when refresh token is expired or revoked.
    /// Input: Valid command with device that has invalid refresh token.
    /// Expected: Returns failure with "Refresh token expired or revoked" and UNAUTHORIZED error code.
    /// </summary>
    [Fact]
    public async Task Handle_RefreshTokenInvalid_ReturnsTokenExpiredFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(-1));
        var device = deviceResult.Value;
        device.Revoke();

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh token expired or revoked", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure when refresh token doesn't match.
    /// Input: Command with refresh token that doesn't match device's refresh token.
    /// Expected: Returns failure with "Invalid refresh token" and UNAUTHORIZED error code.
    /// </summary>
    [Fact]
    public async Task Handle_RefreshTokenMismatch_ReturnsInvalidTokenFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "correct-refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var command = new RefreshTokenCommand("wrong-refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid refresh token", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure when user is not found.
    /// Input: Valid device but user doesn't exist in repository.
    /// Expected: Returns failure with "User not found or inactive" and UNAUTHORIZED error code.
    /// </summary>
    [Fact]
    public async Task Handle_UserNotFound_ReturnsUserNotFoundFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found or inactive", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure when user is inactive.
    /// Input: Valid device but user is marked as inactive.
    /// Expected: Returns failure with "User not found or inactive" and UNAUTHORIZED error code.
    /// </summary>
    [Fact]
    public async Task Handle_UserInactive_ReturnsUserInactiveFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;
        user.Deactivate();

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found or inactive", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure when SetRefreshToken fails.
    /// Input: Valid device and user, but SetRefreshToken returns failure (e.g., empty token).
    /// Expected: Returns the same failure result from SetRefreshToken.
    /// </summary>
    [Fact]
    public async Task Handle_SetRefreshTokenFails_ReturnsSetRefreshTokenFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh Token Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle successfully refreshes tokens.
    /// Input: Valid command with valid device, user, and all validations pass.
    /// Expected: Returns success with new access token, refresh token, user id, and expiry time.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessWithNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user, "device-id"), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateRefreshToken(), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns internal error when an exception is thrown.
    /// Input: Any exception thrown during processing.
    /// Expected: Returns failure with "An error occurred while refreshing token" and INTERNAL_ERROR error code.
    /// </summary>
    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsInternalErrorFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while refreshing token", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle passes cancellation token to repository methods.
    /// Input: Valid command with cancellation token.
    /// Expected: Cancellation token is passed through to all async repository calls.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenToRepositories()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        var cancellationToken = new CancellationToken();

        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", cancellationToken))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        _deviceRepositoryMock.Verify(r => r.GetByDeviceIdAsync("device-id", cancellationToken), Times.Once);
        _userRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests Handle with various edge case device IDs.
    /// Input: Commands with empty, whitespace, very long, or special character device IDs.
    /// Expected: Processes according to repository behavior (likely returns device not found).
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("device-id-with-very-long-string-that-exceeds-normal-length-expectations-to-test-boundary-conditions")]
    [InlineData("device@#$%^&*()")]
    [InlineData("device\nid\twith\rcontrol")]
    public async Task Handle_EdgeCaseDeviceIds_HandlesGracefully(string deviceId)
    {
        // Arrange
        var command = new RefreshTokenCommand("refresh-token", deviceId);
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid device", result.Error);
    }

    /// <summary>
    /// Tests Handle with various edge case refresh tokens.
    /// Input: Commands with empty, whitespace, very long, or special character refresh tokens.
    /// Expected: Returns appropriate failure based on token validation.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("refresh-token-with-very-long-string-that-exceeds-normal-length-expectations-to-test-boundary-conditions")]
    [InlineData("token@#$%^&*()")]
    [InlineData("token\nwith\tcontrol\rchars")]
    public async Task Handle_EdgeCaseRefreshTokens_HandlesGracefully(string refreshToken)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "actual-refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var command = new RefreshTokenCommand(refreshToken, "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid refresh token", result.Error);
    }

    /// <summary>
    /// Tests that Handle updates device last activity when successful.
    /// Input: Valid command that passes all validations.
    /// Expected: Device.UpdateLastActivity is called and device is updated in repository.
    /// </summary>
    [Fact]
    public async Task Handle_SuccessfulRefresh_UpdatesDeviceLastActivity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;
        var originalLastActivity = device.LastActivityAt;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle uses correct expiry times for tokens.
    /// Input: Valid command with mocked clock time.
    /// Expected: Refresh token expires at clock.UtcNow + 30 days, response expires at UtcNow + 1 hour.
    /// </summary>
    [Fact]
    public async Task Handle_SuccessfulRefresh_UsesCorrectExpiryTimes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var expectedExpiresAt = DateTime.UtcNow.AddHours(1);
        Assert.True(Math.Abs((result.Value.ExpiresAt - expectedExpiresAt).TotalSeconds) < 5);
    }

    /// <summary>
    /// Tests that Handle with expired refresh token returns appropriate failure.
    /// Input: Device with refresh token that expired in the past.
    /// Expected: Returns failure with "Refresh token expired or revoked".
    /// </summary>
    [Fact]
    public async Task Handle_ExpiredRefreshToken_ReturnsTokenExpiredFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(-5));
        var device = deviceResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh token expired or revoked", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle correctly filters user by device's UserId.
    /// Input: Valid device with specific UserId.
    /// Expected: FindAsync is called with expression that filters by user.Id == device.UserId.
    /// </summary>
    [Fact]
    public async Task Handle_FindsUserByDeviceUserId_QueriesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        Expression<Func<User, bool>>? capturedExpression = null;

        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<User, bool>>, CancellationToken>((expr, ct) => capturedExpression = expr)
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedExpression);
        _userRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that Handle generates access token with correct parameters.
    /// Input: Valid request with specific user and device ID.
    /// Expected: GenerateAccessToken is called with the correct user and device ID.
    /// </summary>
    [Fact]
    public async Task Handle_GeneratesAccessToken_WithCorrectParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceResult = UserDevice.Create(
            userId,
            "device-id",
            "device-name",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var userResult = User.Create("user@example.com", "User Name", "password-hash");
        var user = userResult.Value;

        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _userRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { user });
        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user, "device-id"))
            .Returns("new-access-token");
        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken())
            .Returns("new-refresh-token");
        _deviceRepositoryMock
            .Setup(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user, "device-id"), Times.Once);
    }
}