using MediatR;
using Shared.Domain;

namespace Users.Features.Authentication.RegisterWithPassword;

public record RegisterWithPasswordRequest(string Email, string Name, string Password);

public record RegisterWithPasswordCommand(
    string Email,
    string Name,
    string Password) : IRequest<Result<Guid>>;
