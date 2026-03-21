using Moq;
using Shared.Domain;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.Authentication.Logout;

namespace Users.Features.UnitTests.Authentication.Logout;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _deviceRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        _handler = new LogoutCommandHandler(_deviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DeviceNotFound_ReturnsFailureWithValidationError()
    {
        var deviceId = "non-existent-device-id";
        var command = new LogoutCommand(deviceId);
        var cancellationToken = CancellationToken.None;

        _deviceRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(deviceId, cancellationToken))
            .ReturnsAsync((UserDevice?)null);

        var result = await _handler.Handle(command, cancellationToken);

        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Device not found", result.Error);
        Assert.Equal(ErrorCodes.RESOURCE_NOT_FOUND, result.ErrorCode);
        _deviceRepositoryMock.Verify(x => x.GetByDeviceIdAsync(deviceId, cancellationToken), Times.Once);
        _deviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

