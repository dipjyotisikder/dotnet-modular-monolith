using Moq;


namespace Users.Features.UnitTests.Authentication.RegisterWithPassword;

/// <summary>
/// Unit tests for <see cref="RegisterWithPasswordCommandHandler"/>.
/// </summary>
public class RegisterWithPasswordCommandHandlerTests
{
    /// <summary>
    /// Tests that Handle successfully registers a new user when email does not exist,
    /// user creation succeeds, and all operations complete successfully.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequestWithNoExistingUser_ReturnsSuccessWithUserId()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns("hashed_password");

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
        mockUserRepository.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        mockPasswordHasher.Verify(h => h.HashPassword("password123"), Times.Once);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure with DUPLICATE_RESOURCE error code
    /// when an existing user with the same email already exists.
    /// </summary>
    [Fact]
    public async Task Handle_EmailAlreadyExists_ReturnsFailureWithDuplicateResourceError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("existing@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        var existingUser = User.Create("existing@example.com", "Existing User", "hash").Value;
        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { existingUser });

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email already exists", result.Error);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        mockUserRepository.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        mockPasswordHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure when User.Create fails due to empty email.
    /// </summary>
    [Theory]
    [InlineData("", "Test User", "password123")]
    [InlineData("   ", "Test User", "password123")]
    public async Task Handle_EmptyOrWhitespaceEmail_ReturnsFailureWithValidationError(string email, string name, string password)
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand(email, name, password);
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword(password))
            .Returns("hashed_password");

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure when User.Create fails due to empty name.
    /// </summary>
    [Theory]
    [InlineData("test@example.com", "", "password123")]
    [InlineData("test@example.com", "   ", "password123")]
    public async Task Handle_EmptyOrWhitespaceName_ReturnsFailureWithValidationError(string email, string name, string password)
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand(email, name, password);
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword(password))
            .Returns("hashed_password");

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Name Cannot Be Empty", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure with INTERNAL_ERROR when FindAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_FindAsyncThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ThrowsAsync(new Exception("Database connection failed"));

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during registration", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure with INTERNAL_ERROR when AddAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_AddAsyncThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns("hashed_password");

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during registration", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure with INTERNAL_ERROR when SaveChangesAsync throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_SaveChangesAsyncThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns("hashed_password");

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Exception("Transaction failed"));

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during registration", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure with INTERNAL_ERROR when PasswordHasher throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_PasswordHasherThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Throws(new Exception("Hashing algorithm error"));

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred during registration", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle propagates cancellation token to all async operations.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PropagatesCancellationToAllOperations()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns("hashed_password");

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        mockUserRepository.Verify(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle correctly hashes the password from the request.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_HashesPasswordCorrectly()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "MySecurePassword123!");
        var cancellationToken = CancellationToken.None;
        var expectedHash = "hashed_MySecurePassword123!";

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("MySecurePassword123!"))
            .Returns(expectedHash);

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        mockPasswordHasher.Verify(h => h.HashPassword("MySecurePassword123!"), Times.Once);
    }

    /// <summary>
    /// Tests that Handle adds the user with correct properties to the repository.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_AddsUserWithCorrectProperties()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;
        var hashedPassword = "hashed_password";

        User? capturedUser = null;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns(hashedPassword);

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, ct) => capturedUser = user)
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedUser);
        Assert.Equal("test@example.com", capturedUser.Email);
        Assert.Equal("Test User", capturedUser.Name);
        Assert.Equal(hashedPassword, capturedUser.PasswordHash);
        Assert.NotEqual(Guid.Empty, capturedUser.Id);
    }

    /// <summary>
    /// Tests that Handle returns the correct user ID in the success result.
    /// </summary>
    [Fact]
    public async Task Handle_ValidRequest_ReturnsCorrectUserId()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("test@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        Guid capturedUserId = Guid.Empty;

        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User>());

        mockPasswordHasher
            .Setup(h => h.HashPassword("password123"))
            .Returns("hashed_password");

        mockUserRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, ct) => capturedUserId = user.Id)
            .Returns(Task.CompletedTask);

        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(capturedUserId, result.Value);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    /// <summary>
    /// Tests that Handle processes multiple existing users with same email correctly.
    /// </summary>
    [Fact]
    public async Task Handle_MultipleUsersWithSameEmail_ReturnsFailureWithDuplicateResourceError()
    {
        // Arrange
        var mockUserRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var request = new RegisterWithPasswordCommand("duplicate@example.com", "Test User", "password123");
        var cancellationToken = CancellationToken.None;

        var existingUser1 = User.Create("duplicate@example.com", "User 1", "hash1").Value;
        var existingUser2 = User.Create("duplicate@example.com", "User 2", "hash2").Value;
        mockUserRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { existingUser1, existingUser2 });

        var handler = new RegisterWithPasswordCommandHandler(
            mockUserRepository.Object,
            mockPasswordHasher.Object,
            mockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Email already exists", result.Error);
        Assert.Equal(ErrorCodes.DUPLICATE_RESOURCE, result.ErrorCode);
        mockPasswordHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cancellationToken), Times.Never);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Never);
    }
}