using Moq;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.DeviceManagement.RevokeAllDevices;


namespace Users.Features.UnitTests.DeviceManagement.RevokeAllDevices;

public class RevokeAllDevicesCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidUserAndSuccessfulRevoke_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var reason = "Test revocation";
        var command = new RevokeAllDevicesCommand(userId, reason);
        var cancellationToken = CancellationToken.None;

        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value!;
        var users = new List<User> { user };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        mockUserRepository.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        mockDeviceRepository.Verify(x => x.RevokeUserDevicesAsync(userId, cancellationToken), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsValidationError()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockDeviceRepository.Verify(x => x.RevokeUserDevicesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExceptionThrown_ReturnsInternalError()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error revoking all devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_MultipleUsersReturned_ProcessesFirstUser()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var userResult1 = User.Create("test1@example.com", "Test User 1");
        var userResult2 = User.Create("test2@example.com", "Test User 2");
        var users = new List<User> { userResult1.Value!, userResult2.Value! };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DefaultReason_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId);
        var cancellationToken = CancellationToken.None;

        var userResult = User.Create("test@example.com", "Test User");
        var users = new List<User> { userResult.Value! };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("Valid reason")]
    [InlineData("Very long reason with lots of text that could potentially cause issues if there are size limitations in the system")]
    public async Task Handle_VariousReasonStrings_ReturnsSuccess(string reason)
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, reason);
        var cancellationToken = CancellationToken.None;

        var userResult = User.Create("test@example.com", "Test User");
        var users = new List<User> { userResult.Value! };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_EmptyGuidUserId_HandlesCorrectly()
    {
        var userId = Guid.Empty;
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_DeviceRepositoryThrowsException_ReturnsInternalError()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var userResult = User.Create("test@example.com", "Test User");
        var users = new List<User> { userResult.Value! };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .ThrowsAsync(new Exception("Device revocation failed"));

        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("Error revoking all devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UnitOfWorkThrowsException_ReturnsInternalError()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationToken = CancellationToken.None;

        var userResult = User.Create("test@example.com", "Test User");
        var users = new List<User> { userResult.Value! };

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(users);

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        mockDeviceRepository
            .Setup(x => x.RevokeUserDevicesAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Exception("Database save failed"));

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("Error revoking all devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_CancelledToken_ReturnsInternalError()
    {
        var userId = Guid.NewGuid();
        var command = new RevokeAllDevicesCommand(userId, "Test reason");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        var mockUserRepository = new Mock<IUserRepository>();
        mockUserRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeAllDevicesCommandHandler(
            mockUserRepository.Object,
            mockDeviceRepository.Object,
            mockUnitOfWork.Object);

        var result = await handler.Handle(command, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("Error revoking all devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }
}
