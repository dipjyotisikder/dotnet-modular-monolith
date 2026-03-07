using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.GetMyBookings;

public record BookingSummary(
    Guid BookingId,
    Guid HotelId,
    Guid RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    int Nights,
    decimal TotalAmount,
    string Currency,
    string Status,
    DateTime CreatedAt);

public record GetMyBookingsQuery(Guid GuestId) : IRequest<Result<IEnumerable<BookingSummary>>>;
