using Shared.Domain;

namespace Bookings.Domain.Events;

public record BookingCancelledDomainEvent(
    Guid BookingId,
    Guid GuestId,
    Guid HotelId,
    Guid RoomId,
    string Reason) : DomainEvent;
