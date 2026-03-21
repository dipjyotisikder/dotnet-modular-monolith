using Moq;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.UserManagement.CreateUser;


namespace Users.Features.UnitTests.UserManagement.CreateUser;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequestWithUniqueEmail_ReturnsSuccessWithUserId()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("test@example.com", "Test User");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailureWithDuplicateResourceError()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("existing@example.com", "Test User");
        var cancellationToken = CancellationToken.None;
        var existingUser = User.Create("existing@example.com", "Existing User").Value;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new[] { existingUser });

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Email already exists", result.Error);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "empty email")]
    [InlineData("   ", "whitespace email")]
    public async Task Handle_InvalidEmail_ReturnsValidationFailure(string email, string testDescription)
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand(email, "Test User");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "empty name")]
    [InlineData("   ", "whitespace name")]
    public async Task Handle_InvalidName_ReturnsValidationFailure(string name, string testDescription)
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("test@example.com", name);
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToAllAsyncOperations()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("test@example.com", "Test User");
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_FindAsyncChecksCorrectEmail()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var testEmail = "specific@example.com";
        var command = new CreateUserCommand(testEmail, "Test User");
        var cancellationToken = CancellationToken.None;
        System.Linq.Expressions.Expression<Func<User, bool>>? capturedPredicate = null;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .Callback<System.Linq.Expressions.Expression<Func<User, bool>>, CancellationToken>((pred, ct) => capturedPredicate = pred)
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedPredicate);

        var compiledPredicate = capturedPredicate!.Compile();
        var matchingUser = User.Create(testEmail, "Some Name").Value;
        var nonMatchingUser = User.Create("other@example.com", "Other Name").Value;

        Assert.True(compiledPredicate(matchingUser));
        Assert.False(compiledPredicate(nonMatchingUser));
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserWithCorrectData()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var testEmail = "test@example.com";
        var testName = "Test User Name";
        var command = new CreateUserCommand(testEmail, testName);
        var cancellationToken = CancellationToken.None;
        User? capturedUser = null;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedUser);
        Assert.Equal(testEmail, capturedUser!.Email);
        Assert.Equal(testName, capturedUser.Name);
        Assert.Equal(result.Value, capturedUser.Id);
    }

    [Theory]
    [InlineData("test+tag@example.com", "User With Special-Characters_123")]
    [InlineData("user.name@sub-domain.example.co.uk", "Üser Ñame")]
    public async Task Handle_SpecialCharactersInEmailAndName_ReturnsSuccess(string email, string name)
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand(email, name);
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task Handle_VeryLongEmailAndName_ReturnsSuccess()
    {
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var longEmail = new string('a', 240) + "@example.com";
        var longName = new string('b', 500);
        var command = new CreateUserCommand(longEmail, longName);
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());
        userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var result = await handler.Handle(command, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }
}
