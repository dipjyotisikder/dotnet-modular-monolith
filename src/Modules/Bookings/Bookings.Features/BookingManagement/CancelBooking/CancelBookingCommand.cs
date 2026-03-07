using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.CancelBooking;

public record CancelBookingCommand(
    Guid BookingId,
    Guid RequestingUserId,
    string Reason) : IRequest<Result>;
