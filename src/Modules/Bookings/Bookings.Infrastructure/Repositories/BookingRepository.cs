using Bookings.Domain.Entities;
using Bookings.Domain.Enums;
using Bookings.Domain.Repositories;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace Bookings.Infrastructure.Repositories;

public class BookingRepository(BookingsDbContext context) : Repository<Booking>(context), IBookingRepository
{
    private readonly BookingsDbContext _context = context;

    public async Task<IEnumerable<Booking>> GetByGuestIdAsync(Guid guestId, CancellationToken cancellationToken = default)
        => await _context.Bookings
            .Where(b => b.GuestId == guestId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<bool> HasOverlappingBookingAsync(
        Guid roomId,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken cancellationToken = default)
        => await _context.Bookings.AnyAsync(
            b => b.RoomId == roomId
                 && b.Status != BookingStatus.Cancelled
                 && b.DateRange.CheckIn < checkOut
                 && b.DateRange.CheckOut > checkIn,
            cancellationToken);
}
