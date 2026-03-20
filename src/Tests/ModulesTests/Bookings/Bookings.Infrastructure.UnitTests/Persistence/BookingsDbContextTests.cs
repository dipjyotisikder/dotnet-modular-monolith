using Moq;

namespace Bookings.Infrastructure.UnitTests.Persistence;
/// <summary>
/// Unit tests for the <see cref = "BookingsDbContext"/> class.
/// </summary>
public class BookingsDbContextTests
{
    /// <summary>
    /// Tests that OnModelCreating applies configurations from assembly without throwing exceptions.
    /// </summary>
    [Fact]
    public void OnModelCreating_WhenCalled_AppliesConfigurationsFromAssemblySuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var clockMock = new Mock<Shared.Domain.Services.ISystemClock>();
        var userContextMock = new Mock<Shared.Domain.Services.IUserContext>();
        var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptorMock = new Mock<AuditableEntityInterceptor>(clockMock.Object, userContextMock.Object);
        var domainEventInterceptorMock = new Mock<DomainEventOutboxInterceptor>(clockMock.Object, serviceScopeFactoryMock.Object);
        // Act & Assert
        using var context = new BookingsDbContext(options, auditableInterceptorMock.Object, domainEventInterceptorMock.Object);
        var exception = Record.Exception(() => _ = context.Model);
        Assert.Null(exception);
    }

    /// <summary>
    /// Helper class to expose the protected OnConfiguring method for testing.
    /// </summary>
    private class TestableBookingsDbContext : BookingsDbContext
    {
        public TestableBookingsDbContext(DbContextOptions<BookingsDbContext> options, AuditableEntityInterceptor auditableEntityInterceptor, DomainEventOutboxInterceptor domainEventOutboxInterceptor) : base(options, auditableEntityInterceptor, domainEventOutboxInterceptor)
        {
        }

        public void ExposeOnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            OnConfiguring(optionsBuilder);
        }
    }

    /// <summary>
    /// Tests that the Rooms property returns a DbSet of the correct entity type (Room).
    /// </summary>
    [Fact]
    public void Rooms_WhenAccessed_ReturnsDbSetOfRoom()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase_Rooms_Type").Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var mockAuditInterceptor = new Mock<AuditableEntityInterceptor>(mockClock.Object, mockUserContext.Object);
        var mockDomainEventInterceptor = new Mock<DomainEventOutboxInterceptor>(mockClock.Object, mockScopeFactory.Object);
        var context = new BookingsDbContext(options, mockAuditInterceptor.Object, mockDomainEventInterceptor.Object);
        // Act
        var rooms = context.Rooms;
        // Assert
        Assert.IsAssignableFrom<DbSet<Room>>(rooms);
    }

    /// <summary>
    /// Tests that the Hotels property returns a DbSet of the correct entity type.
    /// </summary>
    [Fact]
    public void Hotels_WhenAccessed_ReturnsDbSetOfHotelType()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase_Hotels_Type").Options;
        var clockMock = new Mock<Shared.Domain.Services.ISystemClock>();
        var userContextMock = new Mock<Shared.Domain.Services.IUserContext>();
        var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptorMock = new Mock<AuditableEntityInterceptor>(clockMock.Object, userContextMock.Object);
        var domainEventInterceptorMock = new Mock<DomainEventOutboxInterceptor>(clockMock.Object, serviceScopeFactoryMock.Object);
        var context = new BookingsDbContext(options, auditableInterceptorMock.Object, domainEventInterceptorMock.Object);
        // Act
        var result = context.Hotels;
        // Assert
        Assert.IsAssignableFrom<DbSet<Hotel>>(result);
    }

}