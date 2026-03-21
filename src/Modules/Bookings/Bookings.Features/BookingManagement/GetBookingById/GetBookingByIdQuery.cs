using MediatR;
using Shared.Application.Behaviors;
using Shared.Domain;
using Shared.Domain.Authorization;
using AuthorizationPermission = Shared.Domain.Authorization.Permission;

namespace Bookings.Features.BookingManagement.GetBookingById;

public record GetBookingByIdResponse(
    Guid BookingId,
    Guid GuestId,
    Guid HotelId,
    Guid RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int Nights,
    decimal TotalAmount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancellationReason);

public record GetBookingByIdQuery(Guid BookingId)
    : IRequest<Result<GetBookingByIdResponse>>, IPermissionRequired, IRequirementRequired
{
    public string Permission => AuthorizationPermission.BookingRead;

    public IAuthorizationRequirement[] Requirements =>
    [
        new GetBookingByIdAuthorizationHandler.Requirement
        {
            BookingId = BookingId
        }
    ];
}
