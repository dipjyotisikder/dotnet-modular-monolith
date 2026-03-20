using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Repositories;

public class BookingsUnitOfWorkTests
{
    [Fact]
    public async Task BeginTransactionAsync_WithDefaultCancellationToken_CallsDatabaseBeginTransactionAndReturnsTransaction()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        var mockDatabase = new Mock<DatabaseFacade>(Mock.Of<DbContext>());
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockContext = new Mock<BookingsDbContext>(
            options,
            auditableInterceptor,
            domainEventInterceptor);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var result = await unitOfWork.BeginTransactionAsync();

        Assert.NotNull(result);
        Assert.Same(mockTransaction.Object, result);
        mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Once);
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenDatabaseThrowsException_PropagatesException()
    {
        var expectedException = new InvalidOperationException("Database error");
        var mockDatabase = new Mock<DatabaseFacade>(Mock.Of<DbContext>());
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockContext = new Mock<BookingsDbContext>(
            options,
            auditableInterceptor,
            domainEventInterceptor);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitOfWork.BeginTransactionAsync());
        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task BeginTransactionAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, domainEventInterceptor);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(cancellationToken)).ThrowsAsync(new OperationCanceledException(cancellationToken));
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await unitOfWork.BeginTransactionAsync(cancellationToken));
    }

    [Fact]
    public async Task BeginTransactionAsync_CalledMultipleTimes_ReturnsNewTransactionEachTime()
    {
        var mockTransaction1 = new Mock<IDbContextTransaction>();
        var mockTransaction2 = new Mock<IDbContextTransaction>();
        var optionsBuilder = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
        var auditableInterceptor = new AuditableEntityInterceptor(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var outboxInterceptor = new DomainEventOutboxInterceptor(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var context = new BookingsDbContext(optionsBuilder.Options, auditableInterceptor, outboxInterceptor);
        var mockDatabase = new Mock<DatabaseFacade>(context);
        mockDatabase.SetupSequence(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction1.Object).ReturnsAsync(mockTransaction2.Object);
        var mockContext = new Mock<BookingsDbContext>(optionsBuilder.Options, auditableInterceptor, outboxInterceptor);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var result1 = await unitOfWork.BeginTransactionAsync();
        var result2 = await unitOfWork.BeginTransactionAsync();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Same(mockTransaction1.Object, result1);
        Assert.Same(mockTransaction2.Object, result2);
        Assert.NotSame(result1, result2);
        mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Exactly(2));
    }

    [Fact]
    public async Task CommitTransactionAsync_NoActiveTransaction_SavesChangesSuccessfully()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;

        await unitOfWork.CommitTransactionAsync(cancellationToken);

        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CommitTransactionAsync_WithCancellationToken_PassesTokenToSaveChanges()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, outboxInterceptor);
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        mockContext.Setup(c => c.SaveChangesAsync(cancellationToken)).ReturnsAsync(1);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await unitOfWork.CommitTransactionAsync(cancellationToken);

        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CommitTransactionAsync_WithActiveTransaction_CommitsTransactionSuccessfully()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, outboxInterceptor);
        var mockDatabase = new Mock<DatabaseFacade>(Mock.Of<DbContext>());
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task CommitTransactionAsync_SaveChangesThrows_RollsBackAndRethrows()
    {
        var expectedException = new InvalidOperationException("Save failed");
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, outboxInterceptor);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CommitTransactionAsync_CommitAsyncThrows_RollsBackDisposesAndRethrows()
    {
        var expectedException = new InvalidOperationException("Commit failed");
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var mockDatabase = new Mock<DatabaseFacade>(Mock.Of<DbContext>());
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, outboxInterceptor);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task CommitTransactionAsync_SaveChangesThrowsWithTransaction_DisposesTransactionInFinally()
    {
        var expectedException = new DbUpdateException("Database error");
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockTransaction.Setup(t => t.DisposeAsync()).Returns(ValueTask.CompletedTask);
        var mockDatabase = new Mock<DatabaseFacade>(Mock.Of<DbContext>());
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, outboxInterceptor);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetExceptionTestData))]
    public async Task CommitTransactionAsync_VariousExceptions_RollsBackAndRethrows(Exception exception)
    {
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;

        var thrownException = await Assert.ThrowsAsync(exception.GetType(), () => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(exception, thrownException);
    }

    [Fact]
    public async Task CommitTransactionAsync_DefaultCancellationToken_UsesDefaultToken()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await unitOfWork.CommitTransactionAsync();

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    public static TheoryData<Exception> GetExceptionTestData()
    {
        return new TheoryData<Exception>
        {
            new InvalidOperationException("Invalid operation"),
            new DbUpdateException("Database update failed"),
            new OperationCanceledException("Operation cancelled"),
            new TimeoutException("Operation timed out"),
            new ArgumentException("Invalid argument")
        };
    }

    private static Mock<BookingsDbContext> CreateMockContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        return new Mock<BookingsDbContext>(options, auditableInterceptor, domainEventInterceptor);
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenTransactionIsNull_CompletesWithoutError()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var context = new BookingsDbContext(
            options,
            auditableInterceptor,
            domainEventInterceptor);
        var unitOfWork = new BookingsUnitOfWork(context);

        await unitOfWork.RollbackTransactionAsync();
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenTransactionExists_CallsRollbackAndDispose()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new Shared.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new Shared.Infrastructure.Persistence.Interceptors.DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();
        var context = new BookingsDbContext(options, auditableInterceptor, domainEventInterceptor);
        var unitOfWork = new BookingsUnitOfWork(context);
        var transactionField = typeof(BookingsUnitOfWork).GetField("_transaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        transactionField?.SetValue(unitOfWork, mockTransaction.Object);

        await unitOfWork.RollbackTransactionAsync();

        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenRollbackThrowsException_StillDisposesTransaction()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();
        var expectedException = new InvalidOperationException("Rollback failed");
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitOfWork.RollbackTransactionAsync());
        Assert.Equal(expectedException.Message, exception.Message);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackTransactionAsync_PassesCancellationToken_ToRollbackAsync()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = Mock.Of<ISystemClock>();
        var mockUserContext = Mock.Of<IUserContext>();
        var mockServiceScopeFactory = Mock.Of<IServiceScopeFactory>();
        var mockAuditableEntityInterceptor = new AuditableEntityInterceptor(mockClock, mockUserContext);
        var mockDomainEventOutboxInterceptor = new DomainEventOutboxInterceptor(mockClock, mockServiceScopeFactory);
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableEntityInterceptor, mockDomainEventOutboxInterceptor);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();
        var cancellationToken = new CancellationToken(false);

        await unitOfWork.RollbackTransactionAsync(cancellationToken);

        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task RollbackTransactionAsync_AfterCompletion_SetsTransactionToNull()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockDomainEventInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var context = new BookingsDbContext(options, mockAuditableInterceptor.Object, mockDomainEventInterceptor.Object);
        var mockDatabase = new Mock<DatabaseFacade>(context);
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockDomainEventInterceptor.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        await unitOfWork.RollbackTransactionAsync();
        await unitOfWork.RollbackTransactionAsync();

        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenCancellationTokenCancelled_ThrowsOperationCanceledException()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancelledToken = cancellationTokenSource.Token;
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new OperationCanceledException(cancelledToken));
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await unitOfWork.RollbackTransactionAsync(cancelledToken));
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task RollbackTransactionAsync_WithDefaultCancellationToken_UsesDefaultToken()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>().Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var mockAuditInterceptor = new Mock<AuditableEntityInterceptor>(mockClock.Object, mockUserContext.Object);
        var mockDomainEventInterceptor = new Mock<DomainEventOutboxInterceptor>(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditInterceptor.Object, mockDomainEventInterceptor.Object);
        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockTransaction.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        await unitOfWork.RollbackTransactionAsync();

        mockTransaction.Verify(t => t.RollbackAsync(It.Is<CancellationToken>(ct => ct == default)), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WhenTransactionIsNull_OnlyDisposesContext()
    {
        var mockContext = CreateMockContext();
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await ((IAsyncDisposable)unitOfWork).DisposeAsync();

        mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_WhenTransactionIsNotNull_DisposesTransactionAndContext()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockContext = new Mock<BookingsDbContext>(
            options,
            null!,
            null!)
        {
            CallBase = false
        };
        var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        await ((IAsyncDisposable)unitOfWork).DisposeAsync();

        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisposeAsync_CalledMultipleTimes_IsIdempotent()
    {
        var mockOptions = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var outboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(mockOptions, auditableInterceptor, outboxInterceptor);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();

        mockContext.Verify(c => c.DisposeAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task DisposeAsync_CalledMultipleTimesWithTransaction_DisposesTransactionOnce()
    {
        var mockTransaction = new Mock<IDbContextTransaction>();
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: "TestDb").Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockServiceScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var eventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(options, auditableInterceptor, eventInterceptor);
        var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();

        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.DisposeAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task DisposeAsync_WhenContextDisposalThrows_PropagatesException()
    {
        var expectedException = new InvalidOperationException("Context disposal failed");
        var mockOptions = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockServiceScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(mockOptions, auditableInterceptor, domainEventInterceptor);
        mockContext.Setup(c => c.DisposeAsync()).ThrowsAsync(expectedException);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ((IAsyncDisposable)unitOfWork).DisposeAsync());
        Assert.Same(expectedException, exception);
    }

    [Fact]
    public async Task DisposeAsync_WhenTransactionDisposalThrows_PropagatesException()
    {
        var expectedException = new InvalidOperationException("Transaction disposal failed");
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.DisposeAsync()).ThrowsAsync(expectedException);
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(
            options,
            auditableInterceptor,
            domainEventInterceptor);
        mockContext.CallBase = false;
        var mockDatabase = new Mock<Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.BeginTransactionAsync(default)).ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        await unitOfWork.BeginTransactionAsync();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ((IAsyncDisposable)unitOfWork).DisposeAsync());
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.DisposeAsync(), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_DefaultCancellationToken_CallsContextAndReturnsResult()
    {
        const int expectedChanges = 5;
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedChanges);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var result = await unitOfWork.SaveChangesAsync();

        Assert.Equal(expectedChanges, result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ExplicitCancellationToken_PassesTokenToContext()
    {
        const int expectedChanges = 3;
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var mockClock = new Mock<ISystemClock>();
        var mockUserContext = new Mock<IUserContext>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var auditInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var eventInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(
            new DbContextOptionsBuilder<BookingsDbContext>().Options,
            auditInterceptor,
            eventInterceptor);
        mockContext.Setup(c => c.SaveChangesAsync(cancellationToken)).ReturnsAsync(expectedChanges);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        Assert.Equal(expectedChanges, result);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_NoChangesSaved_ReturnsZero()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var mockAuditInterceptor = new Mock<AuditableEntityInterceptor>(
            Mock.Of<ISystemClock>(),
            Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(
            Mock.Of<ISystemClock>(),
            Mock.Of<IServiceScopeFactory>());
        var context = new BookingsDbContext(
            options,
            mockAuditInterceptor.Object,
            mockOutboxInterceptor.Object);
        var unitOfWork = new BookingsUnitOfWork(context);

        var result = await unitOfWork.SaveChangesAsync();

        Assert.Equal(0, result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task SaveChangesAsync_MultipleEntitiesSaved_ReturnsCorrectCount(int savedCount)
    {
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(savedCount);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        var result = await unitOfWork.SaveChangesAsync();

        Assert.Equal(savedCount, result);
    }

    [Fact]
    public async Task SaveChangesAsync_CancelledToken_PropagatesOperationCanceledException()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;
        var mockClock = new Mock<Shared.Domain.Services.ISystemClock>();
        var mockUserContext = new Mock<Shared.Domain.Services.IUserContext>();
        var mockScopeFactory = new Mock<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>();
        var auditableEntityInterceptor = new AuditableEntityInterceptor(mockClock.Object, mockUserContext.Object);
        var domainEventOutboxInterceptor = new DomainEventOutboxInterceptor(mockClock.Object, mockScopeFactory.Object);
        var mockContext = new Mock<BookingsDbContext>(
            new DbContextOptionsBuilder<BookingsDbContext>().Options,
            auditableEntityInterceptor,
            domainEventOutboxInterceptor);
        mockContext.Setup(c => c.SaveChangesAsync(cancellationToken)).ThrowsAsync(new OperationCanceledException());
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() => unitOfWork.SaveChangesAsync(cancellationToken));
    }
}
