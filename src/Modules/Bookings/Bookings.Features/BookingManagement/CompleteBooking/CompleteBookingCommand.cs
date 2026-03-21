using MediatR;
using Shared.Application.Behaviors;
using Shared.Domain;
using AuthorizationPermission = Shared.Domain.Authorization.Permission;

namespace Bookings.Features.BookingManagement.CompleteBooking;

public record CompleteBookingCommand(Guid BookingId) : IRequest<Result>, IPermissionRequired
{
    public string Permission => AuthorizationPermission.BookingComplete;
}
