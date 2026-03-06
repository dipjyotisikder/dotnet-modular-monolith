using MediatR;
using Shared.Domain;

namespace Users.Features.Authentication.Logout;

public record LogoutCommand(string DeviceId) : IRequest<Result>;
