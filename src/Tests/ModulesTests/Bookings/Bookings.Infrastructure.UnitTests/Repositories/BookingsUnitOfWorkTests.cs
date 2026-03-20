using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Bookings.Infrastructure.Persistence;
using Bookings.Infrastructure.Repositories;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;

namespace Bookings.Infrastructure.UnitTests.Repositories;

public class BookingsUnitOfWorkTests
{
    /// <summary>
    /// Tests that BeginTransactionAsync calls the database's BeginTransactionAsync method with default cancellation token
    /// and returns the transaction.
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_WithDefaultCancellationToken_CallsDatabaseBeginTransactionAndReturnsTransaction()
    {
        // Arrange
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
        // Act
        var result = await unitOfWork.BeginTransactionAsync();
        // Assert
        Assert.NotNull(result);
        Assert.Same(mockTransaction.Object, result);
        mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Once);
    }

    /// <summary>
    /// Tests that BeginTransactionAsync propagates exceptions thrown by the database's BeginTransactionAsync method.
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_WhenDatabaseThrowsException_PropagatesException()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitOfWork.BeginTransactionAsync());
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that BeginTransactionAsync honors cancellation by propagating OperationCanceledException
    /// when the cancellation token is cancelled.
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
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
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await unitOfWork.BeginTransactionAsync(cancellationToken));
    }

    /// <summary>
    /// Tests that multiple calls to BeginTransactionAsync each return a new transaction from the database.
    /// </summary>
    [Fact]
    public async Task BeginTransactionAsync_CalledMultipleTimes_ReturnsNewTransactionEachTime()
    {
        // Arrange
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
        // Act
        var result1 = await unitOfWork.BeginTransactionAsync();
        var result2 = await unitOfWork.BeginTransactionAsync();
        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Same(mockTransaction1.Object, result1);
        Assert.Same(mockTransaction2.Object, result2);
        Assert.NotSame(result1, result2);
        mockDatabase.Verify(d => d.BeginTransactionAsync(default), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that CommitTransactionAsync successfully saves changes when no transaction is active.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_NoActiveTransaction_SavesChangesSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;
        // Act
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        // Assert
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync passes the correct cancellation token to SaveChangesAsync.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_WithCancellationToken_PassesTokenToSaveChanges()
    {
        // Arrange
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
        // Act
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        // Assert
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync with active transaction calls both SaveChangesAsync and CommitAsync.
    /// This test requires BeginTransactionAsync to be called first to set up the transaction.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_WithActiveTransaction_CommitsTransactionSuccessfully()
    {
        // Arrange
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
        // First begin a transaction to set the internal _transaction field
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        // Act
        await unitOfWork.CommitTransactionAsync(cancellationToken);
        // Assert
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync calls RollbackTransactionAsync and rethrows when SaveChangesAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_SaveChangesThrows_RollsBackAndRethrows()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync with active transaction calls RollbackAsync and DisposeAsync when CommitAsync throws.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_CommitAsyncThrows_RollsBackDisposesAndRethrows()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync disposes transaction in finally block even when SaveChangesAsync throws.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_SaveChangesThrowsWithTransaction_DisposesTransactionInFinally()
    {
        // Arrange
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
        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.CommitTransactionAsync(cancellationToken));
        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync handles different exception types correctly.
    /// </summary>
    /// <param name = "exception">The exception to be thrown during SaveChangesAsync.</param>
    [Theory]
    [MemberData(nameof(GetExceptionTestData))]
    public async Task CommitTransactionAsync_VariousExceptions_RollsBackAndRethrows(Exception exception)
    {
        // Arrange
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        var cancellationToken = CancellationToken.None;
        // Act & Assert
        var thrownException = await Assert.ThrowsAsync(exception.GetType(), () => unitOfWork.CommitTransactionAsync(cancellationToken));
        Assert.Same(exception, thrownException);
    }

    /// <summary>
    /// Tests that CommitTransactionAsync uses default cancellation token when none is provided.
    /// </summary>
    [Fact]
    public async Task CommitTransactionAsync_DefaultCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BookingsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var mockAuditableInterceptor = new Mock<AuditableEntityInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IUserContext>());
        var mockOutboxInterceptor = new Mock<DomainEventOutboxInterceptor>(Mock.Of<ISystemClock>(), Mock.Of<IServiceScopeFactory>());
        var mockContext = new Mock<BookingsDbContext>(options, mockAuditableInterceptor.Object, mockOutboxInterceptor.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        // Act
        await unitOfWork.CommitTransactionAsync();
        // Assert
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Provides test data for various exception scenarios.
    /// </summary>
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

    /// <summary>
    /// Creates a mock BookingsDbContext with DbContextOptions and interceptors.
    /// </summary>
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

    /// <summary>
    /// Tests that RollbackTransactionAsync completes successfully without any action
    /// when no transaction has been started (_transaction is null).
    /// Expected: Method completes without calling RollbackAsync or DisposeAsync.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_WhenTransactionIsNull_CompletesWithoutError()
    {
        // Arrange
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
        // Act
        await unitOfWork.RollbackTransactionAsync();
        // Assert
        // No exception should be thrown, method completes successfully
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync properly rolls back and disposes the transaction
    /// when a transaction exists.
    /// Expected: RollbackAsync and DisposeAsync are called on the transaction.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_WhenTransactionExists_CallsRollbackAndDispose()
    {
        // Arrange
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
        // Inject the mock transaction using reflection
        var transactionField = typeof(BookingsUnitOfWork).GetField("_transaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        transactionField?.SetValue(unitOfWork, mockTransaction.Object);
        // Act
        await unitOfWork.RollbackTransactionAsync();
        // Assert
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync still disposes the transaction even when
    /// RollbackAsync throws an exception.
    /// Expected: DisposeAsync is called despite the exception in RollbackAsync.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_WhenRollbackThrowsException_StillDisposesTransaction()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await unitOfWork.RollbackTransactionAsync());
        Assert.Equal(expectedException.Message, exception.Message);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync correctly passes the CancellationToken
    /// to the RollbackAsync method.
    /// Expected: RollbackAsync receives the same CancellationToken that was passed to RollbackTransactionAsync.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_PassesCancellationToken_ToRollbackAsync()
    {
        // Arrange
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
        // Act
        await unitOfWork.RollbackTransactionAsync(cancellationToken);
        // Assert
        mockTransaction.Verify(t => t.RollbackAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync sets the internal transaction to null after completion,
    /// ensuring subsequent calls do not attempt to rollback or dispose again.
    /// Expected: Second call to RollbackTransactionAsync does not invoke RollbackAsync or DisposeAsync.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_AfterCompletion_SetsTransactionToNull()
    {
        // Arrange
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
        // Act
        await unitOfWork.RollbackTransactionAsync();
        await unitOfWork.RollbackTransactionAsync();
        // Assert
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync handles a cancelled CancellationToken appropriately
    /// by propagating the cancellation to RollbackAsync.
    /// Expected: OperationCanceledException is thrown when using a cancelled token.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_WhenCancellationTokenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange
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
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await unitOfWork.RollbackTransactionAsync(cancelledToken));
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RollbackTransactionAsync with default CancellationToken uses CancellationToken.None.
    /// Expected: RollbackAsync is called with the default CancellationToken.
    /// </summary>
    [Fact]
    public async Task RollbackTransactionAsync_WithDefaultCancellationToken_UsesDefaultToken()
    {
        // Arrange
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
        // Act
        await unitOfWork.RollbackTransactionAsync();
        // Assert
        mockTransaction.Verify(t => t.RollbackAsync(It.Is<CancellationToken>(ct => ct == default)), Times.Once);
    }

    /// <summary>
    /// Tests that DisposeAsync only disposes the context when no transaction is active.
    /// Input: _transaction field is null.
    /// Expected: Only context.DisposeAsync() is called.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WhenTransactionIsNull_OnlyDisposesContext()
    {
        // Arrange
        var mockContext = CreateMockContext();
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        // Act
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        // Assert
        mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that DisposeAsync disposes both transaction and context when a transaction is active.
    /// Input: _transaction field is not null (set via BeginTransactionAsync).
    /// Expected: Both transaction.DisposeAsync() and context.DisposeAsync() are called.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WhenTransactionIsNotNull_DisposesTransactionAndContext()
    {
        // Arrange
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
        // Act
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        // Assert
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.DisposeAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that DisposeAsync can be called multiple times safely (idempotency).
    /// Input: DisposeAsync called twice on the same instance.
    /// Expected: No exception is thrown, and context disposal is attempted on each call.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
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
        // Act
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        // Assert
        mockContext.Verify(c => c.DisposeAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that DisposeAsync handles multiple calls when a transaction was active.
    /// Input: Transaction is set, then DisposeAsync is called twice.
    /// Expected: Transaction is disposed only once, context is disposed twice, no exception thrown.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_CalledMultipleTimesWithTransaction_DisposesTransactionOnce()
    {
        // Arrange
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
        // Act
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        await ((IAsyncDisposable)unitOfWork).DisposeAsync();
        // Assert
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.DisposeAsync(), Times.Exactly(2));
    }

    /// <summary>
    /// Tests that DisposeAsync properly handles when context disposal throws an exception.
    /// Input: Context.DisposeAsync() throws an exception.
    /// Expected: Exception is propagated to the caller.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WhenContextDisposalThrows_PropagatesException()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ((IAsyncDisposable)unitOfWork).DisposeAsync());
        Assert.Same(expectedException, exception);
    }

    /// <summary>
    /// Tests that DisposeAsync properly handles when transaction disposal throws an exception.
    /// Input: Transaction.DisposeAsync() throws an exception.
    /// Expected: Exception is propagated and context disposal is not reached.
    /// </summary>
    [Fact]
    public async Task DisposeAsync_WhenTransactionDisposalThrows_PropagatesException()
    {
        // Arrange
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
        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ((IAsyncDisposable)unitOfWork).DisposeAsync());
        Assert.Same(expectedException, exception);
        mockContext.Verify(c => c.DisposeAsync(), Times.Never);
    }

    /// <summary>
    /// Tests that SaveChangesAsync calls the underlying context's SaveChangesAsync method
    /// with the default cancellation token and returns the correct result.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_DefaultCancellationToken_CallsContextAndReturnsResult()
    {
        // Arrange
        const int expectedChanges = 5;
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedChanges);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        // Act
        var result = await unitOfWork.SaveChangesAsync();
        // Assert
        Assert.Equal(expectedChanges, result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SaveChangesAsync correctly passes an explicit cancellation token
    /// to the underlying context's SaveChangesAsync method and returns the result.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_ExplicitCancellationToken_PassesTokenToContext()
    {
        // Arrange
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
        // Act
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);
        // Assert
        Assert.Equal(expectedChanges, result);
        mockContext.Verify(c => c.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that SaveChangesAsync returns zero when no changes are saved to the database.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_NoChangesSaved_ReturnsZero()
    {
        // Arrange
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
        // Act
        var result = await unitOfWork.SaveChangesAsync();
        // Assert
        Assert.Equal(0, result);
    }

    /// <summary>
    /// Tests that SaveChangesAsync returns the correct count when multiple entities are saved.
    /// </summary>
    /// <param name = "savedCount">The number of entities saved to the database.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task SaveChangesAsync_MultipleEntitiesSaved_ReturnsCorrectCount(int savedCount)
    {
        // Arrange
        var mockContext = CreateMockContext();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(savedCount);
        var unitOfWork = new BookingsUnitOfWork(mockContext.Object);
        // Act
        var result = await unitOfWork.SaveChangesAsync();
        // Assert
        Assert.Equal(savedCount, result);
    }

    /// <summary>
    /// Tests that SaveChangesAsync propagates OperationCanceledException when
    /// the cancellation token is cancelled.
    /// </summary>
    [Fact]
    public async Task SaveChangesAsync_CancelledToken_PropagatesOperationCanceledException()
    {
        // Arrange
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
        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => unitOfWork.SaveChangesAsync(cancellationToken));
    }

}