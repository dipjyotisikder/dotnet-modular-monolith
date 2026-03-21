using Bookings.Domain.Repositories;
using Shared.Domain.Authorization;

namespace Bookings.Features.BookingManagement.CancelBooking;

public class CancelBookingAuthorizationHandler(IBookingRepository bookingRepository)
    : IAuthorizationRequirementHandler<CancelBookingAuthorizationHandler.Requirement>
{
    public class Requirement : IAuthorizationRequirement
    {
        public Guid BookingId { get; set; }
    }

    public async Task<bool> HandleAsync(Guid userId, Requirement requirement)
    {
        var booking = await bookingRepository.GetByIdAsync(requirement.BookingId);
        if (booking is null)
            return false;

        return booking.GuestId == userId;
    }
}
