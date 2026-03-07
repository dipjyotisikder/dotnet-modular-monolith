using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.CompleteBooking;

public record CompleteBookingCommand(Guid BookingId) : IRequest<Result>;
