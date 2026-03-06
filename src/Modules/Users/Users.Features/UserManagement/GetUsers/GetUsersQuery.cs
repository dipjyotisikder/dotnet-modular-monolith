using MediatR;
using Shared.Domain;

namespace Users.Features.UserManagement.GetUsers;

public record GetUsersResponse(
    string UserId,
    string Email,
    string Name,
    string Tier,
    DateTime CreatedAt,
    bool IsActive);

public record GetUsersQuery : IRequest<Result<IEnumerable<GetUsersResponse>>>;
