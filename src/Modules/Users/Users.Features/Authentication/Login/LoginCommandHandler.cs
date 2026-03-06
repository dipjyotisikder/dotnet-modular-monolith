using MediatR;
using Shared.Domain;
using Shared.Domain.Services;
using Users.Domain.Entities;
using Users.Domain.Repositories;
using Users.Domain.Services;

namespace Users.Features.Authentication.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IUserDeviceRepository deviceRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ISystemClock clock)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await ValidateCredentials(request, cancellationToken);
            if (user == null)
                return Result.Failure<LoginResponse>("Invalid email or password", ErrorCodes.UNAUTHORIZED);

            var deviceResult = await CreateDeviceSessionAndTokens(user, request, cancellationToken);
            if (deviceResult.IsFailure)
                return Result.Failure<LoginResponse>(deviceResult.Error, deviceResult.ErrorCode);

            var (device, accessToken, refreshToken) = deviceResult.Value;

            user.UpdateLastLogin(clock);

            var response = new LoginResponse(
                accessToken,
                refreshToken,
                user.Id.ToString(),
                user.Email,
                user.Name,
                user.GetRoles().ToArray(),
                device.DeviceId);

            return Result.Success(response);
        }
        catch (Exception)
        {
            return Result.Failure<LoginResponse>("An error occurred during login", ErrorCodes.INTERNAL_ERROR);
        }
    }

    private async Task<User?> ValidateCredentials(LoginCommand request, CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAsync(u => u.Email == request.Email, cancellationToken);
        var user = users.FirstOrDefault();

        if (user == null || !user.IsActive || user.OAuthProvider != null)
            return null;

        if (user.PasswordHash == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        return user;
    }

    private async Task<Result<(UserDevice Device, string AccessToken, string RefreshToken)>> CreateDeviceSessionAndTokens(
        User user,
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var deviceId = Guid.NewGuid().ToString();
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = clock.UtcNow.AddDays(30);
        var accessToken = jwtTokenService.GenerateAccessToken(user, deviceId);

        var deviceResult = UserDevice.Create(
            userId: user.Id,
            deviceId: deviceId,
            deviceName: request.DeviceName ?? "Unknown Device",
            refreshToken: refreshToken,
            refreshTokenExpiresAt: refreshTokenExpiresAt,
            deviceType: request.DeviceType,
            ipAddress: request.IpAddress);

        if (deviceResult.IsFailure)
            return Result.Failure<(UserDevice, string, string)>(deviceResult.Error, deviceResult.ErrorCode);

        await deviceRepository.AddAsync(deviceResult.Value, cancellationToken);

        var setTokenResult = user.SetRefreshToken(refreshToken, refreshTokenExpiresAt);
        if (setTokenResult.IsFailure)
            return Result.Failure<(UserDevice, string, string)>(setTokenResult.Error, setTokenResult.ErrorCode);

        return Result.Success((deviceResult.Value, accessToken, refreshToken));
    }
}
