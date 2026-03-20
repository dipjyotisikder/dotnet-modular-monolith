using Moq;
using Xunit;
using Users.Domain.Entities;
using Shared.Domain;
using Users.Features.DeviceManagement.GetUserDevices;
using Users.Domain.Repositories;

namespace Users.Features.UnitTests.DeviceManagement.GetUserDevices;

/// <summary>
/// Unit tests for <see cref="GetUserDevicesQueryHandler"/>.
/// </summary>
public class GetUserDevicesQueryHandlerTests
{
    /// <summary>
    /// Tests that Handle returns a successful result with correctly mapped devices when the repository returns multiple devices.
    /// Input: Valid query with userId, repository returns multiple devices.
    /// Expected: IsSuccess is true, all devices are correctly mapped to response objects.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequestWithMultipleDevices_ReturnsSuccessWithMappedDevices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userDevices = new List<UserDevice>
        {
            CreateUserDevice("device1", "Device One", "Mobile", "192.168.1.1", DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddHours(-1), false, null),
            CreateUserDevice("device2", "Device Two", "Desktop", "192.168.1.2", DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddHours(-2), true, "User logged out"),
            CreateUserDevice("device3", "Device Three", null, null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddMinutes(-30), false, null)
        };

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(userDevices);

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Error);

        var responseList = result.Value.ToList();
        Assert.Equal(3, responseList.Count);

        Assert.Equal("device1", responseList[0].DeviceId);
        Assert.Equal("Device One", responseList[0].DeviceName);
        Assert.Equal("Mobile", responseList[0].DeviceType);
        Assert.Equal("192.168.1.1", responseList[0].IpAddress);
        Assert.False(responseList[0].IsRevoked);
        Assert.Null(responseList[0].RevokeReason);

        Assert.Equal("device2", responseList[1].DeviceId);
        Assert.Equal("Device Two", responseList[1].DeviceName);
        Assert.Equal("Desktop", responseList[1].DeviceType);
        Assert.Equal("192.168.1.2", responseList[1].IpAddress);
        Assert.True(responseList[1].IsRevoked);
        Assert.Equal("User logged out", responseList[1].RevokeReason);

        Assert.Equal("device3", responseList[2].DeviceId);
        Assert.Equal("Device Three", responseList[2].DeviceName);
        Assert.Null(responseList[2].DeviceType);
        Assert.Null(responseList[2].IpAddress);
        Assert.False(responseList[2].IsRevoked);
        Assert.Null(responseList[2].RevokeReason);

        mockRepository.Verify(r => r.GetUserDevicesAsync(userId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns a successful result with an empty collection when the repository returns no devices.
    /// Input: Valid query with userId, repository returns empty collection.
    /// Expected: IsSuccess is true, Value contains an empty enumerable.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequestWithNoDevices_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice>());

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        mockRepository.Verify(r => r.GetUserDevicesAsync(userId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns a successful result with a single device when the repository returns one device.
    /// Input: Valid query with userId, repository returns single device.
    /// Expected: IsSuccess is true, Value contains one correctly mapped device.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequestWithSingleDevice_ReturnsSuccessWithSingleDevice()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;
        var issuedAt = DateTime.UtcNow.AddDays(-10);
        var lastActivityAt = DateTime.UtcNow.AddHours(-5);

        var userDevice = CreateUserDevice("single-device", "Single Device", "Tablet", "10.0.0.1", issuedAt, lastActivityAt, false, null);

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice> { userDevice });

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.Single();
        Assert.Equal("single-device", response.DeviceId);
        Assert.Equal("Single Device", response.DeviceName);
        Assert.Equal("Tablet", response.DeviceType);
        Assert.Equal("10.0.0.1", response.IpAddress);
        Assert.Equal(issuedAt, response.IssuedAt);
        Assert.Equal(lastActivityAt, response.LastActivityAt);
        Assert.False(response.IsRevoked);
        Assert.Null(response.RevokeReason);

        mockRepository.Verify(r => r.GetUserDevicesAsync(userId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns a failure result when the repository throws an exception.
    /// Input: Valid query, repository throws exception.
    /// Expected: IsFailure is true, error message is "Error retrieving user devices", error code is INTERNAL_ERROR.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ThrowsAsync(new Exception("Database connection failed"));

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error retrieving user devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
        mockRepository.Verify(r => r.GetUserDevicesAsync(userId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns a failure result when the repository throws an InvalidOperationException.
    /// Input: Valid query, repository throws InvalidOperationException.
    /// Expected: IsFailure is true, error message is "Error retrieving user devices", error code is INTERNAL_ERROR.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsInvalidOperationException_ReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Invalid operation"));

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Error retrieving user devices", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle correctly passes the CancellationToken to the repository method.
    /// Input: Valid query with a specific CancellationToken.
    /// Expected: Repository method is called with the same CancellationToken.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_PassesCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice>());

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        await handler.Handle(query, cancellationToken);

        // Assert
        mockRepository.Verify(r => r.GetUserDevicesAsync(userId, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle works correctly with Guid.Empty as userId.
    /// Input: Query with Guid.Empty as userId.
    /// Expected: Repository is called with Guid.Empty and returns success.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyGuidUserId_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.Empty;
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice>());

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        mockRepository.Verify(r => r.GetUserDevicesAsync(Guid.Empty, cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle correctly maps devices with nullable properties set to null.
    /// Input: Device with DeviceType, IpAddress, and RevokeReason all null.
    /// Expected: Response object has null values for nullable properties.
    /// </summary>
    [Fact]
    public async Task Handle_DevicesWithNullablePropertiesNull_MapsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userDevice = CreateUserDevice("device-id", "Device Name", null, null, DateTime.UtcNow, DateTime.UtcNow, false, null);

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice> { userDevice });

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.Single();
        Assert.Null(response.DeviceType);
        Assert.Null(response.IpAddress);
        Assert.Null(response.RevokeReason);
    }

    /// <summary>
    /// Tests that Handle correctly maps devices with nullable properties set to non-null values.
    /// Input: Device with all nullable properties populated.
    /// Expected: Response object has all properties correctly mapped.
    /// </summary>
    [Fact]
    public async Task Handle_DevicesWithAllPropertiesPopulated_MapsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userDevice = CreateUserDevice("device-id", "Device Name", "Smartphone", "172.16.0.1", DateTime.UtcNow, DateTime.UtcNow, true, "Security violation");

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice> { userDevice });

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.Single();
        Assert.Equal("Smartphone", response.DeviceType);
        Assert.Equal("172.16.0.1", response.IpAddress);
        Assert.Equal("Security violation", response.RevokeReason);
    }

    /// <summary>
    /// Tests that Handle correctly maps DateTime properties with precision.
    /// Input: Device with specific IssuedAt and LastActivityAt timestamps.
    /// Expected: Response object has exact DateTime values preserved.
    /// </summary>
    [Fact]
    public async Task Handle_DevicesWithSpecificTimestamps_PreservesDateTimePrecision()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;
        var issuedAt = new DateTime(2023, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var lastActivityAt = new DateTime(2023, 12, 20, 15, 45, 30, DateTimeKind.Utc);

        var userDevice = CreateUserDevice("device-id", "Device Name", "Desktop", "10.0.0.1", issuedAt, lastActivityAt, false, null);

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice> { userDevice });

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.Single();
        Assert.Equal(issuedAt, response.IssuedAt);
        Assert.Equal(lastActivityAt, response.LastActivityAt);
    }

    /// <summary>
    /// Tests that Handle correctly maps IsRevoked property for both true and false values.
    /// Input: Multiple devices with different IsRevoked values.
    /// Expected: Response objects have correct IsRevoked values.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_DevicesWithDifferentIsRevokedValues_MapsCorrectly(bool isRevoked)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserDevicesQuery(userId);
        var cancellationToken = CancellationToken.None;

        var userDevice = CreateUserDevice("device-id", "Device Name", "Mobile", "192.168.1.1", DateTime.UtcNow, DateTime.UtcNow, isRevoked, isRevoked ? "Revoke reason" : null);

        var mockRepository = new Mock<IUserDeviceRepository>();
        mockRepository.Setup(r => r.GetUserDevicesAsync(userId, cancellationToken))
            .ReturnsAsync(new List<UserDevice> { userDevice });

        var handler = new GetUserDevicesQueryHandler(mockRepository.Object);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var response = result.Value.Single();
        Assert.Equal(isRevoked, response.IsRevoked);
    }

    /// <summary>
    /// Helper method to create a UserDevice instance for testing purposes.
    /// Uses reflection to set private properties since UserDevice has a private constructor.
    /// </summary>
    private static UserDevice CreateUserDevice(
        string deviceId,
        string deviceName,
        string? deviceType,
        string? ipAddress,
        DateTime issuedAt,
        DateTime lastActivityAt,
        bool isRevoked,
        string? revokeReason)
    {
        var userDevice = (UserDevice)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(UserDevice));

        typeof(UserDevice).GetProperty(nameof(UserDevice.DeviceId))!.SetValue(userDevice, deviceId);
        typeof(UserDevice).GetProperty(nameof(UserDevice.DeviceName))!.SetValue(userDevice, deviceName);
        typeof(UserDevice).GetProperty(nameof(UserDevice.DeviceType))!.SetValue(userDevice, deviceType);
        typeof(UserDevice).GetProperty(nameof(UserDevice.IpAddress))!.SetValue(userDevice, ipAddress);
        typeof(UserDevice).GetProperty(nameof(UserDevice.IssuedAt))!.SetValue(userDevice, issuedAt);
        typeof(UserDevice).GetProperty(nameof(UserDevice.LastActivityAt))!.SetValue(userDevice, lastActivityAt);
        typeof(UserDevice).GetProperty(nameof(UserDevice.IsRevoked))!.SetValue(userDevice, isRevoked);
        typeof(UserDevice).GetProperty(nameof(UserDevice.RevokeReason))!.SetValue(userDevice, revokeReason);

        return userDevice;
    }
}