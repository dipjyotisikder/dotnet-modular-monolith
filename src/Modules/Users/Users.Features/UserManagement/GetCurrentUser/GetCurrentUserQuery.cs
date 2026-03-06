using MediatR;
using Shared.Domain;

namespace Users.Features.UserManagement.GetCurrentUser;

public record GetCurrentUserResponse(
    string UserId,
    string Email,
    string Name,
    string Tier,
    string[] Roles,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record GetCurrentUserQuery(string UserId) : IRequest<Result<GetCurrentUserResponse>>;
