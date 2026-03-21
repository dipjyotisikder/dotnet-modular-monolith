using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Repositories;

namespace Users.Features.DeviceManagement.RevokeDevice;

public class RevokeDeviceCommandHandler(
    IUserDeviceRepository deviceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeDeviceCommand, Result>
{
    public async Task<Result> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await deviceRepository.GetByDeviceIdAsync(request.DeviceId, cancellationToken);
        if (device == null)
            return Result.NotFound("Device not found");

        var revokeResult = device.Revoke(request.Reason);
        if (revokeResult.IsFailure)
            return Result.Failure(revokeResult.Error, revokeResult.ErrorCode);

        await deviceRepository.UpdateAsync(device, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
