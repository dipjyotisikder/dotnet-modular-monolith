using MediatR;
using Shared.Domain;

namespace Users.Features.UserManagement.CreateUser;

public record CreateUserRequest(string Email, string Name);

public record CreateUserCommand(
    string Email,
    string Name) : IRequest<Result<Guid>>;
