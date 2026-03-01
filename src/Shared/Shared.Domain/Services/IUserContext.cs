namespace Shared.Domain.Services;

public interface IUserContext
{
    Guid UserId { get; }
    string? UserEmail { get; }
    IReadOnlyList<string> Roles { get; }
    bool IsAuthenticated { get; }
}
