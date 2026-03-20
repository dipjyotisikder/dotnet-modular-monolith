using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Bookings.Infrastructure.UnitTests.Repositories;

public class HotelRepositoryTests
{
    [Fact]
    public async Task GetWithRoomsAsync_NonExistentHotelId_ReturnsNull()
    {
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

        var result = await repository.GetWithRoomsAsync(nonExistentHotelId, cancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithRoomsAsync_EmptyGuid_ReturnsNull()
    {
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

        var result = await repository.GetWithRoomsAsync(emptyGuid, cancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithRoomsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
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

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetWithRoomsAsync(hotelId, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetRoomAsync_NonExistingRoomId_ReturnsNull()
    {
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

        var result = await repository.GetRoomAsync(nonExistingRoomId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRoomAsync_EmptyGuid_ReturnsNull()
    {
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

        var result = await repository.GetRoomAsync(emptyGuid);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRoomAsync_WithCancellationToken_PassesTokenToFindAsync()
    {
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

        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await repository.GetRoomAsync(roomId, cancellationTokenSource.Token));
    }
}
