using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
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
        var unitOfWork = new BookingsUnitOfWork(dbContext);
        var seeder = new RoomSeeder(dbContext, unitOfWork);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await seeder.SeedAsync(cancellationTokenSource.Token));
    }

    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        var seeder = new RoomSeeder(null!, null!);

        var priority = seeder.Priority;

        Assert.Equal(2, priority);
    }

    [Fact]
    public void Priority_WhenAccessed_ReturnsTwo()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDb").Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, auditInterceptor, domainEventInterceptor);
        var mockUnitOfWork = new Mock<BookingsUnitOfWork>(dbContext);
        var roomSeeder = new RoomSeeder(dbContext, mockUnitOfWork.Object);

        var result = roomSeeder.Priority;

        Assert.Equal(2, result);
    }
}
