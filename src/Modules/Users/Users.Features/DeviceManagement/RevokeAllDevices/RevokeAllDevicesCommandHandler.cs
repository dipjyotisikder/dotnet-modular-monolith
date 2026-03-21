using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;
using Users.Domain.Repositories;

namespace Users.Features.DeviceManagement.RevokeAllDevices;

public class RevokeAllDevicesCommandHandler(
    IUserRepository userRepository,
    IUserDeviceRepository deviceRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeAllDevicesCommand, Result>
{
    public async Task<Result> Handle(RevokeAllDevicesCommand request, CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAsync(u => u.Id == request.UserId, cancellationToken);
        var user = users.FirstOrDefault();
        if (user == null)
            return Result.NotFound("User not found");

        var revokeResult = user.RevokeAllTokens(request.Reason);
        if (revokeResult.IsFailure)
            return Result.Failure(revokeResult.Error, revokeResult.ErrorCode);

        await deviceRepository.RevokeUserDevicesAsync(request.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
