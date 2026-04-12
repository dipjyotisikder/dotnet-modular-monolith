using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Seeding;

public class RoomSeederTests
{
    [Fact]
    public async Task SeedAsync_WithCancelledToken_PropagatesCancellation()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockDomainEventOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var dbContext = new BookingsDbContext(options, mockAuditableInterceptor.Object, mockDomainEventOutboxInterceptor.Object);
        var seeder = new RoomSeeder(dbContext);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await seeder.SeedAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        var seeder = new RoomSeeder(null!);

        var priority = seeder.Priority;

        Assert.Equal(2, priority);
    }

    [Fact]
    public void Priority_WhenAccessed_ReturnsTwo()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDb").Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var auditInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, auditInterceptor, domainEventInterceptor);
        var roomSeeder = new RoomSeeder(dbContext);

        var result = roomSeeder.Priority;

        Assert.Equal(2, result);
    }
}
