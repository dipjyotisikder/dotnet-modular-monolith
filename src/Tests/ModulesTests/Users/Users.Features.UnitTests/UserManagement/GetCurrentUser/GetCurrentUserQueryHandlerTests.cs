using Moq;
using System.Linq.Expressions;


namespace Users.Features.UnitTests.UserManagement.GetCurrentUser;

/// <summary>
/// Unit tests for <see cref="GetCurrentUserQueryHandler"/>.
/// </summary>
public class GetCurrentUserQueryHandlerTests
{
    /// <summary>
    /// Tests that Handle returns success result with correct user data when valid user ID is provided and user exists.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUserIdAndUserExists_ReturnsSuccessWithUserData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var name = "Test User";
        var tier = "premium";
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var lastLoginAt = DateTime.UtcNow.AddHours(-1);

        var userResult = User.Create(email, name, "hashedPassword", tier);
        var user = userResult.Value;

        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(tier, result.Value.Tier);
        Assert.NotNull(result.Value.Roles);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure result when user ID string is invalid (not a valid Guid).
    /// </summary>
    /// <param name="invalidUserId">The invalid user ID string to test.</param>
    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid-at-all")]
    [InlineData("12345")]
    [InlineData("xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx")]
    [InlineData("g82c74c3-721d-4f34-80e5-57657b6cbc27")]
    public async Task Handle_InvalidUserIdFormat_ReturnsFailureWithValidationError(string invalidUserId)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(invalidUserId);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid user ID", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that Handle returns failure result when valid user ID is provided but user does not exist.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUserIdButUserNotFound_ReturnsFailureWithValidationError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns failure result when repository throws an exception.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureWithInternalError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while retrieving user", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle returns failure result when Guid.Empty is provided as user ID and user not found.
    /// </summary>
    [Fact]
    public async Task Handle_EmptyGuidUserIdAndUserNotFound_ReturnsFailureWithUserNotFound()
    {
        // Arrange
        var userId = Guid.Empty;
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle respects cancellation token when provided.
    /// </summary>
    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle returns success with null LastLoginAt when user has never logged in.
    /// </summary>
    [Fact]
    public async Task Handle_UserWithNullLastLoginAt_ReturnsSuccessWithNullLastLoginAt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;

        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.LastLoginAt);
    }

    /// <summary>
    /// Tests that Handle returns failure when repository returns null enumerable.
    /// </summary>
    [Fact]
    public async Task Handle_RepositoryReturnsNull_ReturnsFailureWithUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle uses correct predicate to filter users by ID in repository call.
    /// </summary>
    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectPredicate_FiltersById()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;

        Expression<Func<User, bool>>? capturedPredicate = null;
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Expression<Func<User, bool>>, CancellationToken>((predicate, ct) => capturedPredicate = predicate)
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        await handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(capturedPredicate);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle correctly maps all user properties to response.
    /// </summary>
    [Fact]
    public async Task Handle_ValidUser_MapsAllPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var name = "Test User";
        var tier = "premium";

        var userResult = User.Create(email, name, "hashedPassword", tier);
        var user = userResult.Value;

        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(tier, result.Value.Tier);
        Assert.NotNull(result.Value.Roles);
        Assert.NotEmpty(result.Value.Roles);
        Assert.Equal(user.CreatedAt, result.Value.CreatedAt);
        Assert.Equal(user.LastLoginAt, result.Value.LastLoginAt);
    }

    /// <summary>
    /// Tests that Handle with various valid Guid formats returns success when user exists.
    /// </summary>
    /// <param name="guidFormat">The Guid string format to test.</param>
    [Theory]
    [InlineData("D")] // xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    [InlineData("N")] // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    [InlineData("B")] // {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
    [InlineData("P")] // (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
    public async Task Handle_ValidGuidFormats_ReturnsSuccessWhenUserExists(string guidFormat)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userIdString = userId.ToString(guidFormat);
        var userResult = User.Create("test@example.com", "Test User");
        var user = userResult.Value;

        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userIdString);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    /// <summary>
    /// Tests that Handle returns failure for special string edge cases that are not valid Guids.
    /// </summary>
    /// <param name="edgeCaseUserId">The edge case user ID string to test.</param>
    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t  ")]
    [InlineData("null")]
    [InlineData("undefined")]
    public async Task Handle_SpecialStringEdgeCases_ReturnsFailureWithValidationError(string edgeCaseUserId)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(edgeCaseUserId);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid user ID", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }
}