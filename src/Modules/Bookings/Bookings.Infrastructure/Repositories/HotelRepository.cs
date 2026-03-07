using Bookings.Domain.Entities;
using Bookings.Domain.Repositories;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace Bookings.Infrastructure.Repositories;

public class HotelRepository(BookingsDbContext context) : Repository<Hotel>(context), IHotelRepository
{
    private readonly BookingsDbContext _context = context;

    public async Task<Hotel?> GetWithRoomsAsync(Guid hotelId, CancellationToken cancellationToken = default)
        => await _context.Hotels
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == hotelId, cancellationToken);

    public async Task<Room?> GetRoomAsync(Guid roomId, CancellationToken cancellationToken = default)
        => await _context.Rooms.FindAsync([roomId], cancellationToken);
}
