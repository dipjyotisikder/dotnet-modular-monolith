using MediatR;
using Shared.Domain;

namespace Users.Features.DeviceManagement.GetUserDevices;

public record GetUserDevicesQuery(Guid UserId) : IRequest<Result<IEnumerable<GetUserDevicesResponse>>>;
