using MediatR;
using Shared.Domain;

namespace Users.Features.Authentication.Login;

public record LoginRequest(string Email, string Password, string? DeviceName = null, string? DeviceType = null, string? IpAddress = null);

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    string Email,
    string Name,
    string[] Roles,
    string? DeviceId = null);

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceName = null,
    string? DeviceType = null,
    string? IpAddress = null) : IRequest<Result<LoginResponse>>;
