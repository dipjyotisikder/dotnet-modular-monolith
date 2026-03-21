using Moq;
using Shared.Domain;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Features.UserManagement.GetUsers;

namespace Users.Features.UnitTests.UserManagement.GetUsers;

public class GetUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithEmptyUserList_ReturnsSuccessWithEmptyCollection()
    {
        var mockRepository = new Mock<IUserRepository>();
        mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();
        var cancellationToken = CancellationToken.None;

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        var mockRepository = new Mock<IUserRepository>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        mockRepository.Setup(r => r.GetAllAsync(cancellationToken)).ReturnsAsync(new List<User>());
        var handler = new GetUsersQueryHandler(mockRepository.Object);
        var query = new GetUsersQuery();

        await handler.Handle(query, cancellationToken);

        mockRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSpecialCharactersAndEdgeDateValues_MapsCorrectly()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal("test+alias@example.com", responseList[0].Email);
        Assert.Equal("User With Special !@#$% Chars", responseList[0].Name);
        Assert.Equal(minDate, responseList[0].CreatedAt);
        Assert.False(responseList[0].IsActive);
    }

    [Fact]
    public async Task Handle_WithEmptyStringProperties_MapsCorrectly()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal(string.Empty, responseList[0].Name);
        Assert.Equal(string.Empty, responseList[0].Tier);
    }

    [Fact]
    public async Task Handle_WithMaxDateTimeValue_MapsCorrectly()
    {
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

        var result = await handler.Handle(query, cancellationToken);

        Assert.True(result.IsSuccess);
        var responseList = result.Value.ToList();
        Assert.Single(responseList);
        Assert.Equal(maxDate, responseList[0].CreatedAt);
    }

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
