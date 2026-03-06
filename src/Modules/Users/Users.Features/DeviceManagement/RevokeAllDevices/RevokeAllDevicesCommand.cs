using MediatR;
using Shared.Domain;

namespace Users.Features.DeviceManagement.RevokeAllDevices;

public record RevokeAllDevicesCommand(Guid UserId, string Reason = "User initiated") : IRequest<Result>;
