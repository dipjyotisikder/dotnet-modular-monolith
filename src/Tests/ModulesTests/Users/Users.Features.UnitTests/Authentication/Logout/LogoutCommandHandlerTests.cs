using Moq;
using Xunit;
using Users.Domain.Repositories;
using Users.Features.Authentication.Logout;
using Shared.Domain;
using Users.Domain.Entities;

namespace Users.Features.UnitTests.Authentication.Logout;

/// <summary>
/// Unit tests for <see cref="LogoutCommandHandler"/>.
/// </summary>
public class LogoutCommandHandlerTests
{
    private readonly Mock<IUserDeviceRepository> _deviceRepositoryMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _deviceRepositoryMock = new Mock<IUserDeviceRepository>();
        _handler = new LogoutCommandHandler(_deviceRepositoryMock.Object);
    }

    /// <summary>
    /// Tests that Handle returns failure with validation error when device is not found.
    /// </summary>
    [Fact]
    public async Task Handle_DeviceNotFound_ReturnsFailureWithValidationError()
    {
        // Arrange
        var deviceId = "non-existent-device-id";
        var command = new LogoutCommand(deviceId);
        var cancellationToken = CancellationToken.None;

        _deviceRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(deviceId, cancellationToken))
            .ReturnsAsync((UserDevice?)null);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("Device not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        _deviceRepositoryMock.Verify(x => x.GetByDeviceIdAsync(deviceId, cancellationToken), Times.Once);
        _deviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<UserDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure with internal error when GetByDeviceIdAsync throws exception.
    /// </summary>
    [Fact]
    public async Task Handle_GetByDeviceIdAsyncThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var deviceId = "device-id";
        var command = new LogoutCommand(deviceId);
        var cancellationToken = CancellationToken.None;

        _deviceRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(deviceId, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during logout", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
        _deviceRepositoryMock.Verify(x => x.GetByDeviceIdAsync(deviceId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle handles different exception types correctly and returns internal error.
    /// </summary>
    /// <param name="exception">The exception to test.</param>
    [Theory]
    [MemberData(nameof(GetExceptionTestCases))]
    public async Task Handle_VariousExceptionTypes_ReturnsFailureWithInternalError(Exception exception)
    {
        // Arrange
        var deviceId = "device-id";
        var command = new LogoutCommand(deviceId);
        var cancellationToken = CancellationToken.None;

        _deviceRepositoryMock
            .Setup(x => x.GetByDeviceIdAsync(deviceId, cancellationToken))
            .ThrowsAsync(exception);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during logout", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Provides test data for various exception types.
    /// </summary>
    public static TheoryData<Exception> GetExceptionTestCases()
    {
        return new TheoryData<Exception>
        {
            new Exception("Generic exception"),
            new InvalidOperationException("Invalid operation"),
            new ArgumentException("Argument exception"),
            new NullReferenceException("Null reference"),
            new TimeoutException("Timeout")
        };
    }

}