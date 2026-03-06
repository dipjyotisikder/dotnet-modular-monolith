using MediatR;
using Shared.Domain;
using Users.Domain.Repositories;

namespace Users.Features.Authentication.Logout;

public class LogoutCommandHandler(
    IUserDeviceRepository deviceRepository)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await deviceRepository.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
                return Result.Failure("Device not found", ErrorCodes.VALIDATION_ERROR);

            var revokeResult = device.Revoke("User initiated logout");
            if (revokeResult.IsFailure)
                return Result.Failure(revokeResult.Error, revokeResult.ErrorCode);

            await deviceRepository.UpdateAsync(device, cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("An error occurred during logout", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
