using Moq;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.DeviceManagement.RevokeDevice;


namespace Users.Features.UnitTests.DeviceManagement.RevokeDevice;

public class RevokeDeviceCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeviceNotFound_ReturnsValidationError()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDevice?)null);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Test reason");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Device not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockDeviceRepository.Verify(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()), Times.Never);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidDevice_RevokesDeviceSuccessfully()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Security breach");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(device.IsRevoked);
        Assert.Equal("Security breach", device.RevokeReason);
        mockDeviceRepository.Verify(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()), Times.Once);
        mockDeviceRepository.Verify(x => x.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeviceAlreadyRevoked_ReturnsFailure()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;
        device.Revoke("First revocation");

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Second attempt");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Device already revoked", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockDeviceRepository.Verify(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()), Times.Never);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GetByDeviceIdAsyncThrowsException_ReturnsInternalError()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Test reason");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error revoking device", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_UpdateAsyncThrowsException_ReturnsInternalError()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Update failed"));

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Test reason");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error revoking device", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_SaveChangesAsyncThrowsException_ReturnsInternalError()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Test reason");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error revoking device", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_CancellationTokenPassed_PropagatesTokenCorrectly()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", cancellationToken))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(device, cancellationToken))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", "Test reason");

        Result result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        mockDeviceRepository.Verify(x => x.GetByDeviceIdAsync("device-123", cancellationToken), Times.Once);
        mockDeviceRepository.Verify(x => x.UpdateAsync(device, cancellationToken), Times.Once);
        mockUnitOfWork.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_DefaultReason_UsesDefaultValue()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal("User initiated", device.RevokeReason);
    }

    [Fact]
    public async Task Handle_EmptyStringReason_RevokesDeviceWithEmptyReason()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", string.Empty);

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(string.Empty, device.RevokeReason);
    }

    [Fact]
    public async Task Handle_VeryLongReason_RevokesDeviceSuccessfully()
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        var longReason = new string('A', 10000);

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", longReason);

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(longReason, device.RevokeReason);
    }

    [Theory]
    [InlineData("Reason with \n newline")]
    [InlineData("Reason with \t tab")]
    [InlineData("Reason with 特殊字符 unicode")]
    [InlineData("Reason with <html> tags")]
    [InlineData("Reason with \"quotes\"")]
    public async Task Handle_SpecialCharactersInReason_RevokesDeviceSuccessfully(string reason)
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            "device-123",
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync("device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand("device-123", reason);

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
        Assert.Equal(reason, device.RevokeReason);
    }

    [Theory]
    [InlineData("device-123")]
    [InlineData("DEVICE-ABC-XYZ")]
    [InlineData("device_with_underscores")]
    [InlineData("device.with.dots")]
    [InlineData("a")]
    [InlineData("verylongdeviceidwithmanychars123456789012345678901234567890")]
    public async Task Handle_VariousDeviceIdFormats_RevokesDeviceSuccessfully(string deviceId)
    {
        var mockDeviceRepository = new Mock<IUserDeviceRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var deviceResult = UserDevice.Create(
            Guid.NewGuid(),
            deviceId,
            "Test Device",
            "refresh-token",
            DateTime.UtcNow.AddDays(30));
        var device = deviceResult.Value;

        mockDeviceRepository
            .Setup(x => x.GetByDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        mockDeviceRepository
            .Setup(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RevokeDeviceCommandHandler(mockDeviceRepository.Object, mockUnitOfWork.Object);
        var command = new RevokeDeviceCommand(deviceId, "Test");

        Result result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(device.IsRevoked);
    }
}
