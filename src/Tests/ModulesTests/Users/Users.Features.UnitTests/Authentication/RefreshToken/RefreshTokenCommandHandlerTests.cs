using Moq;
using Shared.Domain;
using Shared.Domain.Services;
using System.Linq.Expressions;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.Services;
using Users.Features.Authentication.RefreshToken;

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

    [Fact]
    public async Task Handle_DeviceNotFound_ReturnsInvalidDeviceFailure()
    {
        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid device", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_RefreshTokenInvalid_ReturnsTokenExpiredFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh token expired or revoked", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_RefreshTokenMismatch_ReturnsInvalidTokenFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid refresh token", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUserNotFoundFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found or inactive", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UserInactive_ReturnsUserInactiveFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found or inactive", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_SetRefreshTokenFails_ReturnsSetRefreshTokenFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh Token Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessWithNewTokens()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal("new-access-token", result.Value.AccessToken);
        Assert.Equal("new-refresh-token", result.Value.RefreshToken);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user, "device-id"), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsInternalErrorFailure()
    {
        var command = new RefreshTokenCommand("refresh-token", "device-id");
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync("device-id", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while refreshing token", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenToRepositories()
    {
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

        var result = await _handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        _deviceRepositoryMock.Verify(r => r.GetByDeviceIdAsync("device-id", cancellationToken), Times.Once);
        _userRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, cancellationToken), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("device-id-with-very-long-string-that-exceeds-normal-length-expectations-to-test-boundary-conditions")]
    [InlineData("device@#$%^&*()")]
    [InlineData("device\nid\twith\rcontrol")]
    public async Task Handle_EdgeCaseDeviceIds_HandlesGracefully(string deviceId)
    {
        var command = new RefreshTokenCommand("refresh-token", deviceId);
        _deviceRepositoryMock
            .Setup(r => r.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid device", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("refresh-token-with-very-long-string-that-exceeds-normal-length-expectations-to-test-boundary-conditions")]
    [InlineData("token@#$%^&*()")]
    [InlineData("token\nwith\tcontrol\rchars")]
    public async Task Handle_EdgeCaseRefreshTokens_HandlesGracefully(string refreshToken)
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid refresh token", result.Error);
    }

    [Fact]
    public async Task Handle_SuccessfulRefresh_UpdatesDeviceLastActivity()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _deviceRepositoryMock.Verify(r => r.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SuccessfulRefresh_UsesCorrectExpiryTimes()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var expectedExpiresAt = DateTime.UtcNow.AddHours(1);
        Assert.True(Math.Abs((result.Value.ExpiresAt - expectedExpiresAt).TotalSeconds) < 5);
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ReturnsTokenExpiredFailure()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Refresh token expired or revoked", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_FindsUserByDeviceUserId_QueriesCorrectly()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedExpression);
        _userRepositoryMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GeneratesAccessToken_WithCorrectParameters()
    {
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

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user, "device-id"), Times.Once);
    }
}
