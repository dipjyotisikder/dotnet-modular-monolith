using Moq;
using Shared.Domain;
using Shared.Domain.Repositories;
using Shared.Domain.Services;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.Services;
using Users.Features.Authentication.Login;

namespace Users.Features.UnitTests.Authentication.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentialsAndSuccessfulLogin_ReturnsSuccessWithLoginResponse()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var password = "ValidPassword123!";
        var deviceName = "TestDevice";
        var deviceType = "Mobile";
        var ipAddress = "192.168.1.1";

        var userResult = User.Create(email, "Test User", "hashedPassword", "free");
        var user = userResult.Value;

        var command = new LoginCommand(email, password, deviceName, deviceType, ipAddress);
        var cancellationToken = CancellationToken.None;

        var utcNow = DateTime.UtcNow;
        var accessToken = "access-token-value";
        var refreshToken = "refresh-token-value";

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword(password, "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(utcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns(accessToken);

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken, result.Value.RefreshToken);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal("Test User", result.Value.Name);
        Assert.NotNull(result.Value.DeviceId);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        passwordHasherMock.Verify(x => x.VerifyPassword(password, "hashedPassword"), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnauthorizedFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var command = new LoginCommand("nonexistent@example.com", "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid email or password", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserIsInactive_ReturnsUnauthorizedFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "inactive@example.com";
        var userResult = User.Create(email, "Inactive User", "hashedPassword");
        var user = userResult.Value;
        user.Deactivate();

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid email or password", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UserHasOAuthProvider_ReturnsUnauthorizedFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "oauth@example.com";
        var userResult = User.CreateWithOAuth(email, "OAuth User", "Google", "google-id");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid email or password", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsUnauthorizedFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "wrongPassword");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("wrongPassword", "hashedPassword"))
            .Returns(false);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid email or password", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_ValidateCredentialsThrowsException_ReturnsInternalErrorFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var command = new LoginCommand("test@example.com", "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during login", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_GenerateAccessTokenThrowsException_ReturnsInternalErrorFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Throws(new InvalidOperationException("Token generation error"));

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during login", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncThrowsException_ReturnsInternalErrorFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database save error"));

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during login", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenToAsyncOperations()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        deviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullOptionalParameters_ReturnsSuccessWithLoginResponse()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password", null, null, null);
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task Handle_UserHasNullPasswordHash_ReturnsUnauthorizedFailure()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", null);
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid email or password", result.Error);
        Assert.Equal(ErrorCodes.UNAUTHORIZED, result.ErrorCode);
        passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserWithMultipleRoles_ReturnsLoginResponseWithAllRoles()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "admin@example.com";
        var userResult = User.Create(email, "Admin User", "hashedPassword");
        var user = userResult.Value;
        user.AddRole("Admin");

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.Roles);
        Assert.Contains("User", result.Value.Roles);
        Assert.Contains("Admin", result.Value.Roles);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_UpdatesUserLastLogin()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password");
        var cancellationToken = CancellationToken.None;
        var utcNow = DateTime.UtcNow;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(utcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(user.LastLoginAt);
        clockMock.Verify(x => x.UtcNow, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_AddsDeviceToRepository()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        var passwordHasherMock = new Mock<IPasswordHasher>();
        var jwtTokenServiceMock = new Mock<IJwtTokenService>();
        var clockMock = new Mock<ISystemClock>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var email = "test@example.com";
        var userResult = User.Create(email, "Test User", "hashedPassword");
        var user = userResult.Value;

        var command = new LoginCommand(email, "password", "MyDevice");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        passwordHasherMock
            .Setup(x => x.VerifyPassword("password", "hashedPassword"))
            .Returns(true);

        clockMock
            .Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<string>()))
            .Returns("access-token");

        deviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<UserDevice>(), cancellationToken))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new LoginCommandHandler(
            userRepositoryMock.Object,
            deviceRepositoryMock.Object,
            passwordHasherMock.Object,
            jwtTokenServiceMock.Object,
            clockMock.Object,
            unitOfWorkMock.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        deviceRepositoryMock.Verify(x => x.AddAsync(It.Is<UserDevice>(d => d.DeviceName == "MyDevice"), cancellationToken), Times.Once);
    }
}
