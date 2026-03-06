using MediatR;
using Shared.Domain;

namespace Users.Features.DeviceManagement.RevokeDevice;

public record RevokeDeviceCommand(string DeviceId, string Reason = "User initiated") : IRequest<Result>;
