using Moq;
using Shared.Domain;
using System.Linq.Expressions;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.UserManagement.GetCurrentUser;


namespace Users.Features.UnitTests.UserManagement.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidUserIdAndUserExists_ReturnsSuccessWithUserData()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(user.Id.ToString(), result.Value.UserId);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(name, result.Value.Name);
        Assert.Equal(tier, result.Value.Tier);
        Assert.NotNull(result.Value.Roles);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

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
        var mockRepository = new Mock<IUserRepository>();
        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(invalidUserId);
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("Invalid user ID", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidUserIdButUserNotFound_ReturnsFailureWithValidationError()
    {
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureWithInternalError()
    {
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("An error occurred while retrieving user", result.Error);
        Assert.Equal(ErrorCodes.INTERNAL_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_EmptyGuidUserIdAndUserNotFound_ReturnsFailureWithUserNotFound()
    {
        var userId = Guid.Empty;
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesCancellationTokenToRepository()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_UserWithNullLastLoginAt_ReturnsSuccessWithNullLastLoginAt()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Value.LastLoginAt);
    }

    [Fact]
    public async Task Handle_RepositoryReturnsEmpty_ReturnsFailureWithUserNotFound()
    {
        var userId = Guid.NewGuid();
        var mockRepository = new Mock<IUserRepository>();
        mockRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<User>());

        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(userId.ToString());
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectPredicate_FiltersById()
    {
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

        await handler.Handle(query, cancellationToken);

        Assert.NotNull(capturedPredicate);
        mockRepository.Verify(r => r.FindAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUser_MapsAllPropertiesCorrectly()
    {
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

        var result = await handler.Handle(query, cancellationToken);

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

    [Theory]
    [InlineData("D")]
    [InlineData("N")]
    [InlineData("B")]
    [InlineData("P")]
    public async Task Handle_ValidGuidFormats_ReturnsSuccessWhenUserExists(string guidFormat)
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Theory]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("  \t  ")]
    [InlineData("null")]
    [InlineData("undefined")]
    public async Task Handle_SpecialStringEdgeCases_ReturnsFailureWithValidationError(string edgeCaseUserId)
    {
        var mockRepository = new Mock<IUserRepository>();
        var handler = new GetCurrentUserQueryHandler(mockRepository.Object);
        var query = new GetCurrentUserQuery(edgeCaseUserId);
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid user ID", result.Error);
        Assert.Equal(ErrorCodes.VALIDATION_ERROR, result.ErrorCode);
    }
}
