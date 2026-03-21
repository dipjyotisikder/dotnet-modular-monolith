using Bookings.Domain.Enums;
using MediatR;
using Shared.Application.Behaviors;
using Shared.Domain;
using AuthorizationPermission = Shared.Domain.Authorization.Permission;

namespace Bookings.Features.HotelManagement.AddRoom;

public record AddRoomRequest(
    string RoomNumber,
    RoomType RoomType,
    decimal PricePerNight,
    string Currency,
    int MaxOccupancy,
    string? Description = null);

public record AddRoomCommand(
    Guid HotelId,
    string RoomNumber,
    RoomType RoomType,
    decimal PricePerNight,
    string Currency,
    int MaxOccupancy,
    string? Description) : IRequest<Result<Guid>>, IPermissionRequired
{
    public string Permission => AuthorizationPermission.HotelAddRoom;
}
