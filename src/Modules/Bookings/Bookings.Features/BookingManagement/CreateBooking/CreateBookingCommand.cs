using MediatR;
using Shared.Domain;

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
    DateTime CheckOut) : IRequest<Result<Guid>>;
