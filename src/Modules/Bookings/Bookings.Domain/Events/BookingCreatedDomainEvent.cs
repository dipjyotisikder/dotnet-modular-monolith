using Shared.Domain;

namespace Bookings.Domain.Events;

public record BookingCreatedDomainEvent(
    Guid BookingId,
    Guid GuestId,
    Guid HotelId,
    Guid RoomId,
    DateTime CheckIn,
    DateTime CheckOut,
    decimal TotalAmount) : DomainEvent;
