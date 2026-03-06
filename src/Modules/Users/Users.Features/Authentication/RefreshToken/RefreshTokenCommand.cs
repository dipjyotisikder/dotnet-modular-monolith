using MediatR;
using Shared.Domain;

namespace Users.Features.Authentication.RefreshToken;

public record RefreshTokenRequest(string RefreshToken, string DeviceId);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    string UserId,
    DateTime ExpiresAt);

public record RefreshTokenCommand(
    string RefreshToken,
    string DeviceId) : IRequest<Result<RefreshTokenResponse>>;
