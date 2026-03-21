using MediatR;
using Shared.Domain;
using Users.Domain.Repositories;

namespace Users.Features.DeviceManagement.GetUserDevices;

public class GetUserDevicesQueryHandler(
    IUserDeviceRepository deviceRepository)
    : IRequestHandler<GetUserDevicesQuery, Result<IEnumerable<GetUserDevicesResponse>>>
{
    public async Task<Result<IEnumerable<GetUserDevicesResponse>>> Handle(GetUserDevicesQuery request, CancellationToken cancellationToken)
    {
        var devices = await deviceRepository.GetUserDevicesAsync(request.UserId, cancellationToken);

        var response = devices.Select(d => new GetUserDevicesResponse(
            d.DeviceId,
            d.DeviceName,
            d.DeviceType,
            d.IpAddress,
            d.IssuedAt,
            d.LastActivityAt,
            d.IsRevoked,
            d.RevokeReason)).ToList();

        return Result.Success(response.AsEnumerable());
    }
}
