using Shared.Domain;

namespace Bookings.Domain.Events;

public record BookingCompletedDomainEvent(
    Guid BookingId,
    Guid GuestId,
    Guid HotelId) : DomainEvent;
