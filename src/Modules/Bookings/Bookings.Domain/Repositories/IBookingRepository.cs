using Bookings.Domain.Entities;
using Shared.Domain.Repositories;

namespace Bookings.Domain.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingBookingAsync(Guid roomId, DateTime checkIn, DateTime checkOut, CancellationToken cancellationToken = default);
}
