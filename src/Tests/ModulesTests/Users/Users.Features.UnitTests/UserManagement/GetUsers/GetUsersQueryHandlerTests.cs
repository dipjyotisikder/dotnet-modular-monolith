using Moq;

namespace Users.Features.UnitTests.UserManagement.GetUsers;
/// <summary>
/// Unit tests for <see cref = "GetUsersQueryHandler"/>.
/// </summary>
public class GetUsersQueryHandlerTests
{
    /// <summary>
    /// Tests that Handle returns success with empty collection when repository returns no users.
    /// Input: Empty user list from repository.
    /// Expected: Result.Success with empty collection.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyUserList_ReturnsSuccessWithEmptyCollection()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    /// <summary>
    /// Tests that Handle returns failure result when repository throws an exception.
    /// Input: Repository throws exception.
    /// Expected: Result.Failure with error message and INTERNAL_ERROR code.
    /// </summary>
    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ReturnsFailureWithErrorMessage()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Database error"));
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while retrieving users", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle passes cancellation token to repository.
    /// Input: Specific cancellation token.
    /// Expected: Repository receives the same cancellation token.
    /// </summary>
    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        mockRepository.Setup(r => r.GetAllAsync(cancellationToken)).ReturnsAsync(new List<User>());
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        // Act
        await handler.Handle(query, cancellationToken);
        // Assert
        mockRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that Handle correctly maps user properties with special characters and edge values.
    /// Input: User with special characters in email and name, DateTime.MinValue.
    /// Expected: Properties correctly mapped including special characters.
    /// </summary>
    [Fact]
    public async Task Handle_WithSpecialCharactersAndEdgeDateValues_MapsCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var minDate = DateTime.MinValue;
        var users = new List<User>
        {
            CreateUser(userId, "test+alias@example.com", "User With Special !@#$% Chars", "free", minDate, false)
        };
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal("test+alias@example.com", responseList[0].Email);
        Assert.Equal("User With Special !@#$% Chars", responseList[0].Name);
        Assert.Equal(minDate, responseList[0].CreatedAt);
        Assert.False(responseList[0].IsActive);
    }

    /// <summary>
    /// Tests that Handle returns failure when repository throws different exception types.
    /// Input: Various exception types (ArgumentException, NullReferenceException, Exception).
    /// Expected: All exceptions result in Result.Failure with same error message.
    /// </summary>
    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(NullReferenceException))]
    [InlineData(typeof(Exception))]
    public async Task Handle_WhenRepositoryThrowsDifferentExceptions_ReturnsFailureResult(Type exceptionType)
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Error message")!;
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ThrowsAsync(exception);
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("An error occurred while retrieving users", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    /// <summary>
    /// Tests that Handle correctly maps users with empty string values.
    /// Input: User with empty strings for name and tier.
    /// Expected: Empty strings are preserved in mapping.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyStringProperties_MapsCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var users = new List<User>
        {
            CreateUser(userId, "test@example.com", string.Empty, string.Empty, createdAt, true)
        };
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal(string.Empty, responseList[0].Name);
        Assert.Equal(string.Empty, responseList[0].Tier);
    }

    /// <summary>
    /// Tests that Handle correctly maps users with DateTime.MaxValue.
    /// Input: User with DateTime.MaxValue as CreatedAt.
    /// Expected: DateTime.MaxValue is correctly mapped.
    /// </summary>
    [Fact]
    public async Task Handle_WithMaxDateTimeValue_MapsCorrectly()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var maxDate = DateTime.MaxValue;
        var users = new List<User>
        {
            CreateUser(userId, "test@example.com", "Test User", "free", maxDate, true)
        };
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;
        // Act
        var result = await handler.Handle(query, cancellationToken);
        // Assert
        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal(maxDate, responseList[0].CreatedAt);
    }

    /// <summary>
    /// Helper method to create a User entity using reflection since constructor is not accessible.
    /// </summary>
    private static User CreateUser(Guid id, string email, string name, string tier, DateTime createdAt, bool isActive)
    {
        var user = (User)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty(nameof(User.Email))!.SetValue(user, email);
        typeof(User).GetProperty(nameof(User.Name))!.SetValue(user, name);
        typeof(User).GetProperty(nameof(User.Tier))!.SetValue(user, tier);
        typeof(User).GetProperty(nameof(User.CreatedAt))!.SetValue(user, createdAt);
        typeof(User).GetProperty(nameof(User.IsActive))!.SetValue(user, isActive);
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(user, id);
        return user;
    }
}