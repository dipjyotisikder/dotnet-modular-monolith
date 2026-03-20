using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Bookings.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Seeding;
/// <summary>
/// Unit tests for <see cref = "RoomSeeder"/>.
/// </summary>
public class RoomSeederTests
{
    /// <summary>
    /// Tests that SeedAsync respects the cancellation token when provided.
    /// This test verifies that the cancellation token is properly propagated through the async operations.
    /// Expected result: The method should handle cancellation appropriately.
    /// </summary>
    /// <remarks>
    /// This is a partial test. To fully test cancellation:
    /// 1. Mock the DbContext to throw OperationCanceledException when cancellation is requested
    /// 2. Set up the Hotels.FirstOrDefaultAsync to check the cancellation token
    /// 3. Verify that the method propagates the cancellation correctly
    /// </remarks>
    [Fact]
    public async Task SeedAsync_WithCancelledToken_PropagatesCancellation()
    {
        // Arrange
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
        // Act & Assert
        // The method should throw OperationCanceledException or TaskCanceledException
        // when the cancellation token is already cancelled
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await seeder.SeedAsync(cancellationTokenSource.Token));
    }

    /// <summary>
    /// Tests that the Priority property returns the expected value.
    /// The Priority determines the order in which seeders are executed.
    /// Expected result: Priority should be 2.
    /// </summary>
    [Fact]
    public void Priority_ReturnsExpectedValue()
    {
        // Arrange
        var seeder = new RoomSeeder(null!, null!);
        // Act
        var priority = seeder.Priority;
        // Assert
        Assert.Equal(2, priority);
    }

    /// <summary>
    /// Verifies that the Priority property returns the expected value of 2.
    /// </summary>
    [Fact]
    public void Priority_WhenAccessed_ReturnsTwo()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDb").Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, auditInterceptor, domainEventInterceptor);
        var mockUnitOfWork = new Mock<BookingsUnitOfWork>(dbContext);
        var roomSeeder = new RoomSeeder(dbContext, mockUnitOfWork.Object);
        // Act
        var result = roomSeeder.Priority;
        // Assert
        Assert.Equal(2, result);
    }
}