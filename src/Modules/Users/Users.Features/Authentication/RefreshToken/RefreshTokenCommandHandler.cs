using MediatR;
using Shared.Domain;
using Shared.Domain.Services;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Features.Authentication.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IUserDeviceRepository deviceRepository,
    IJwtTokenService jwtTokenService,
    ISystemClock clock)
    : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await deviceRepository.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
                return Result.Failure<RefreshTokenResponse>("Invalid device", ErrorCodes.UNAUTHORIZED);

            if (!device.IsRefreshTokenValid())
                return Result.Failure<RefreshTokenResponse>("Refresh token expired or revoked", ErrorCodes.UNAUTHORIZED);

            if (device.RefreshToken != request.RefreshToken)
                return Result.Failure<RefreshTokenResponse>("Invalid refresh token", ErrorCodes.UNAUTHORIZED);

            var users = await userRepository.FindAsync(u => u.Id == device.UserId, cancellationToken);
            var user = users.FirstOrDefault();

            if (user == null || !user.IsActive)
                return Result.Failure<RefreshTokenResponse>("User not found or inactive", ErrorCodes.UNAUTHORIZED);

            if (!user.IsActive || user.TokenRevocationVersion.AccessTokenVersion <= 0)
                return Result.Failure<RefreshTokenResponse>("Tokens have been revoked", ErrorCodes.UNAUTHORIZED);

            var newAccessToken = jwtTokenService.GenerateAccessToken(user, request.DeviceId);
            var newRefreshToken = jwtTokenService.GenerateRefreshToken();
            var newRefreshTokenExpiresAt = clock.UtcNow.AddDays(30);

            var setTokenResult = user.SetRefreshToken(newRefreshToken, newRefreshTokenExpiresAt);
            if (setTokenResult.IsFailure)
                return Result.Failure<RefreshTokenResponse>(setTokenResult.Error, setTokenResult.ErrorCode);

            device.UpdateLastActivity();
            await deviceRepository.UpdateAsync(device, cancellationToken);

            var response = new RefreshTokenResponse(
                newAccessToken,
                newRefreshToken,
                user.Id.ToString(),
                DateTime.UtcNow.AddHours(1));

            return Result.Success(response);
        }
        catch (Exception)
        {
            return Result.Failure<RefreshTokenResponse>("An error occurred while refreshing token", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
