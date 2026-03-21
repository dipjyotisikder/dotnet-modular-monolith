using MediatR;
using Shared.Application.Behaviors;
using Shared.Domain;
using AuthorizationPermission = Shared.Domain.Authorization.Permission;

namespace Bookings.Features.BookingManagement.CreateBooking;

public record CreateBookingRequest(
    Guid HotelId,
    Guid RoomId,
    DateTime CheckIn,
    DateTime CheckOut);

public record CreateBookingCommand(
    Guid GuestId,
    Guid HotelId,
    Guid RoomId,
    DateTime CheckIn,
    DateTime CheckOut) : IRequest<Result<Guid>>, IPermissionRequired
{
    public string Permission => AuthorizationPermission.BookingCreate;
}
