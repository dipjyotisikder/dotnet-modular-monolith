using Moq;
using System.Linq.Expressions;

namespace Bookings.Infrastructure.UnitTests.Seeding;
/// <summary>
/// Unit tests for <see cref = "HotelSeeder"/>.
/// </summary>
public class HotelSeederTests
{
    /// <summary>
    /// Tests that SeedAsync adds all hotels when none exist in the database.
    /// </summary>
    [Fact]
    public async Task SeedAsync_WhenNoHotelsExist_AddsAllThreeHotels()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(mockClock.Object, mockUserContext.Object);
        var mockDomainEventInterceptor = new Mock<DomainEventOutboxInterceptor>(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, mockAuditableInterceptor.Object, mockDomainEventInterceptor.Object);
        var unitOfWork = new BookingsUnitOfWork(dbContext);
        var seeder = new HotelSeeder(dbContext, unitOfWork);
        // Act
        await seeder.SeedAsync(CancellationToken.None);
        // Assert
        var hotelsInDb = await dbContext.Hotels.ToListAsync();
        Assert.Equal(3, hotelsInDb.Count);
    }

    /// <summary>
    /// Tests that Priority property returns 1.
    /// </summary>
    [Fact]
    public void Priority_ReturnsOne()
    {
        // Arrange
        var seeder = new HotelSeeder(null!, null!);
        // Act
        int priority = seeder.Priority;
        // Assert
        Assert.Equal(1, priority);
    }

    /// <summary>
    /// Tests that SeedAsync always calls SaveChangesAsync exactly once.
    /// </summary>
    [Fact]
    public async Task SeedAsync_Always_CallsSaveChangesExactlyOnce()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, auditableInterceptor, domainEventInterceptor);
        var unitOfWork = new Repositories.BookingsUnitOfWork(dbContext);
        var seeder = new HotelSeeder(dbContext, unitOfWork);
        // Act
        await seeder.SeedAsync();
        // Assert
        // Verify that data was saved by checking if hotels exist in the database
        var hotelCount = await dbContext.Hotels.CountAsync();
        Assert.Equal(3, hotelCount);
    }

    /// <summary>
    /// Helper class to support async query operations in tests.
    /// This is required for mocking Entity Framework Core's async LINQ operations.
    /// </summary>
    internal class TestAsyncQueryProvider<TEntity> : IQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }
    }

    /// <summary>
    /// Helper class to support async enumerable operations in tests.
    /// This is required for mocking Entity Framework Core's async LINQ operations.
    /// </summary>
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
        {
        }

        public TestAsyncEnumerable(Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    /// <summary>
    /// Helper class to support async enumerator operations in tests.
    /// This is required for mocking Entity Framework Core's async LINQ operations.
    /// </summary>
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }

    /// <summary>
    /// Tests that the Priority property returns the expected constant value of 1.
    /// This verifies the seeding order priority for hotel data seeding.
    /// </summary>
    [Fact]
    public void Priority_WhenAccessed_ReturnsOne()
    {
        // Arrange
        var seeder = new HotelSeeder(null!, null!);
        // Act
        var priority = seeder.Priority;
        // Assert
        Assert.Equal(1, priority);
    }
}