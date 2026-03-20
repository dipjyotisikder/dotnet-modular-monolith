using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Repositories;


/// <summary>
/// Unit tests for the <see cref="HotelRepository"/> class.
/// </summary>
public class HotelRepositoryTests
{
    /// <summary>
    /// Tests that GetWithRoomsAsync returns null when the hotel ID does not exist.
    /// </summary>
    [Fact]
    public async Task GetWithRoomsAsync_NonExistentHotelId_ReturnsNull()
    {
        // Arrange
        var nonExistentHotelId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await repository.GetWithRoomsAsync(nonExistentHotelId, cancellationToken);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetWithRoomsAsync handles Guid.Empty as a valid input and queries correctly.
    /// </summary>
    [Fact]
    public async Task GetWithRoomsAsync_EmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await repository.GetWithRoomsAsync(emptyGuid, cancellationToken);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetWithRoomsAsync respects cancellation token and throws OperationCanceledException
    /// when the operation is cancelled.
    /// </summary>
    [Fact]
    public async Task GetWithRoomsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var hotelId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetWithRoomsAsync(hotelId, cancellationTokenSource.Token));
    }

    /// <summary>
    /// Tests that GetRoomAsync returns null when the room ID does not exist in the database.
    /// Input: A valid Guid that does not correspond to any existing room.
    /// Expected: Null is returned.
    /// </summary>
    [Fact]
    public async Task GetRoomAsync_NonExistingRoomId_ReturnsNull()
    {
        // Arrange
        var nonExistingRoomId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetRoomAsync(nonExistingRoomId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetRoomAsync handles Guid.Empty correctly.
    /// Input: Guid.Empty.
    /// Expected: Returns null or the room with empty Guid if one exists (depends on database state).
    /// </summary>
    [Fact]
    public async Task GetRoomAsync_EmptyGuid_ReturnsNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);

        // Act
        var result = await repository.GetRoomAsync(emptyGuid);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that GetRoomAsync properly passes the cancellation token to the underlying FindAsync call.
    /// Input: A valid room ID and a cancellation token.
    /// Expected: The cancellation token is passed through to DbSet.FindAsync.
    /// </summary>
    [Fact]
    public async Task GetRoomAsync_WithCancellationToken_PassesTokenToFindAsync()
    {
        // Arrange
        var roomId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();

        var mockAuditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var mockOutboxInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);

        using var context = new BookingsDbContext(options, mockAuditInterceptor, mockOutboxInterceptor);
        var repository = new HotelRepository(context);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetRoomAsync(roomId, cancellationTokenSource.Token));
    }

}