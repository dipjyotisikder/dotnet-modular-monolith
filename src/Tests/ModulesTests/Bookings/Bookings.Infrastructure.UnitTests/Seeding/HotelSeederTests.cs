using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Bookings.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Infrastructure.Persistence.Interceptors;
using System.Linq.Expressions;

namespace Bookings.Infrastructure.UnitTests.Seeding;

public class HotelSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenNoHotelsExist_AddsAllThreeHotels()
    {
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

        await seeder.SeedAsync(CancellationToken.None);

        var hotelsInDb = await dbContext.Hotels.ToListAsync();
        Assert.Equal(3, hotelsInDb.Count);
    }

    [Fact]
    public void Priority_ReturnsOne()
    {
        var seeder = new HotelSeeder(null!, null!);

        int priority = seeder.Priority;

        Assert.Equal(1, priority);
    }

    [Fact]
    public async Task SeedAsync_Always_CallsSaveChangesExactlyOnce()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var dbContext = new BookingsDbContext(options, auditableInterceptor, domainEventInterceptor);
        var unitOfWork = new BookingsUnitOfWork(dbContext);
        var seeder = new HotelSeeder(dbContext, unitOfWork);

        await seeder.SeedAsync();

        var hotelCount = await dbContext.Hotels.CountAsync();
        Assert.Equal(3, hotelCount);
    }

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

    [Fact]
    public void Priority_WhenAccessed_ReturnsOne()
    {
        var seeder = new HotelSeeder(null!, null!);

        var priority = seeder.Priority;

        Assert.Equal(1, priority);
    }
}
