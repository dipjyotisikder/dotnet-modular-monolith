using Bookings.Domain.Entities;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Persistence;

public class BookingsDbContextTests
{
    [Fact]
    public void OnModelCreating_WhenCalled_AppliesConfigurationsFromAssemblySuccessfully()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var clockMock = new Mock<Shared.Domain.Services.ISystemClock>();
        var userContextMock = new Mock<Shared.Domain.Services.IUserContext>();
        var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptorMock = new Mock<AuditableEntityInterceptor>(clockMock.Object, userContextMock.Object);
        var domainEventInterceptorMock = new Mock<DomainEventOutboxInterceptor>(clockMock.Object, serviceScopeFactoryMock.Object);

        using var context = new BookingsDbContext(options, auditableInterceptorMock.Object, domainEventInterceptorMock.Object);
        var exception = Record.Exception(() => _ = context.Model);
        Assert.Null(exception);
    }

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

    [Fact]
    public void Rooms_WhenAccessed_ReturnsDbSetOfRoom()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase_Rooms_Type").Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var mockAuditInterceptor = new Mock<AuditableEntityInterceptor>(mockClock.Object, mockUserContext.Object);
        var mockDomainEventInterceptor = new Mock<DomainEventOutboxInterceptor>(mockClock.Object, mockScopeFactory.Object);
        var context = new BookingsDbContext(options, mockAuditInterceptor.Object, mockDomainEventInterceptor.Object);

        var rooms = context.Rooms;

        Assert.IsAssignableFrom<DbSet<Room>>(rooms);
    }

    [Fact]
    public void Hotels_WhenAccessed_ReturnsDbSetOfHotelType()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDatabase_Hotels_Type").Options;
        var clockMock = new Mock<Shared.Domain.Services.ISystemClock>();
        var userContextMock = new Mock<Shared.Domain.Services.IUserContext>();
        var serviceScopeFactoryMock = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptorMock = new Mock<AuditableEntityInterceptor>(clockMock.Object, userContextMock.Object);
        var domainEventInterceptorMock = new Mock<DomainEventOutboxInterceptor>(clockMock.Object, serviceScopeFactoryMock.Object);
        var context = new BookingsDbContext(options, auditableInterceptorMock.Object, domainEventInterceptorMock.Object);

        var result = context.Hotels;

        Assert.IsAssignableFrom<DbSet<Hotel>>(result);
    }
}
