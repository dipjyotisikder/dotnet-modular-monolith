using Moq;


namespace Users.Features.UnitTests.UserManagement.CreateUser;

/// <summary>
/// Unit tests for <see cref="CreateUserCommandHandler"/>.
/// </summary>
public class CreateUserCommandHandlerTests
{
    /// <summary>
    /// Tests that Handle returns success with user ID when email is unique and user data is valid.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequestWithUniqueEmail_ReturnsSuccessWithUserId()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure when email already exists in the repository.
    /// </summary>
    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailureWithDuplicateResourceError()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("existing@example.com", "Test User");
        var cancellationToken = CancellationToken.None;
        var existingUser = User.Create("existing@example.com", "Existing User").Value;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new[] { existingUser });

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email already exists", result.Error);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns validation failure when email is invalid according to User.Create validation.
    /// </summary>
    /// <param name="email">The invalid email value to test.</param>
    /// <param name="testDescription">Description of the test case.</param>
    [Theory]
    [InlineData("", "empty email")]
    [InlineData("   ", "whitespace email")]
    public async Task Handle_InvalidEmail_ReturnsValidationFailure(string email, string testDescription)
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand(email, "Test User");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns validation failure when name is invalid according to User.Create validation.
    /// </summary>
    /// <param name="name">The invalid name value to test.</param>
    /// <param name="testDescription">Description of the test case.</param>
    [Theory]
    [InlineData("", "empty name")]
    [InlineData("   ", "whitespace name")]
    public async Task Handle_InvalidName_ReturnsValidationFailure(string name, string testDescription)
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("test@example.com", name);
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<User>());

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns internal error when FindAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_FindAsyncThrowsException_ReturnsInternalError()
    {
        // Arrange
        var userRepositoryMock = new Mock<IUserRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var handler = new CreateUserCommandHandler(userRepositoryMock.Object, unitOfWorkMock.Object);
        var command = new CreateUserCommand("test@example.com", "Test User");
        var cancellationToken = CancellationToken.None;

        userRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while creating user", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns internal error when AddAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_AddAsyncThrowsException_ReturnsInternalError()
    {
        // Arrange
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
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while creating user", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns internal error when SaveChangesAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_SaveChangesAsyncThrowsException_ReturnsInternalError()
    {
        // Arrange
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
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while creating user", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle properly passes cancellation token to all async operations.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToAllAsyncOperations()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        userRepositoryMock.Verify(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle correctly checks for the specific email in FindAsync predicate.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_FindAsyncChecksCorrectEmail()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedPredicate);

        var compiledPredicate = capturedPredicate!.Compile();
        var matchingUser = User.Create(testEmail, "Some Name").Value;
        var nonMatchingUser = User.Create("other@example.com", "Other Name").Value;

        Assert.True(compiledPredicate(matchingUser));
        Assert.False(compiledPredicate(nonMatchingUser));
    }

    /// <summary>
    /// Tests that Handle creates user with correct email and name values.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_CreatesUserWithCorrectData()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedUser);
        Assert.Equal(testEmail, capturedUser!.Email);
        Assert.Equal(testName, capturedUser.Name);
        Assert.Equal(result.Value, capturedUser.Id);
    }

    /// <summary>
    /// Tests that Handle works correctly with special characters in email and name.
    /// </summary>
    [Theory]
    [InlineData("test+tag@example.com", "User With Special-Characters_123")]
    [InlineData("user.name@sub-domain.example.co.uk", "Üser Ñame")]
    public async Task Handle_SpecialCharactersInEmailAndName_ReturnsSuccess(string email, string name)
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    /// <summary>
    /// Tests that Handle works correctly with very long valid email and name strings.
    /// </summary>
    [Fact]
    public async Task Handle_VeryLongEmailAndName_ReturnsSuccess()
    {
        // Arrange
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

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }
}