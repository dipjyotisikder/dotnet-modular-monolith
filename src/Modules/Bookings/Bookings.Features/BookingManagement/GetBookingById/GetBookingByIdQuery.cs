using MediatR;
using Shared.Domain;

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

public record GetBookingByIdQuery(Guid BookingId, Guid RequestingUserId)
    : IRequest<Result<GetBookingByIdResponse>>;
