using MediatR;
using Shared.Domain;
using Users.Domain.Repositories;

namespace Users.Features.DeviceManagement.RevokeDevice;

public class RevokeDeviceCommandHandler(
    IUserDeviceRepository deviceRepository)
    : IRequestHandler<RevokeDeviceCommand, Result>
{
    public async Task<Result> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await deviceRepository.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
                return Result.Failure("Device not found", ErrorCodes.VALIDATION_ERROR);

            var revokeResult = device.Revoke(request.Reason);
            if (revokeResult.IsFailure)
                return Result.Failure(revokeResult.Error, revokeResult.ErrorCode);

            await deviceRepository.UpdateAsync(device, cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("Error revoking device", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
