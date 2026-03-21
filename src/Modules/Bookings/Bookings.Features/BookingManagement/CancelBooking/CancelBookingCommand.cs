using MediatR;
using Shared.Application.Behaviors;
using Shared.Domain;
using Shared.Domain.Authorization;
using AuthorizationPermission = Shared.Domain.Authorization.Permission;

namespace Bookings.Features.BookingManagement.CancelBooking;

public record CancelBookingCommand(
    Guid BookingId,
    string Reason) : IRequest<Result>, IPermissionRequired, IRequirementRequired
{
    public string Permission => AuthorizationPermission.BookingCancel;

    public IAuthorizationRequirement[] Requirements =>
    [
        new CancelBookingAuthorizationHandler.Requirement
        {
            BookingId = BookingId
        }
    ];
}
