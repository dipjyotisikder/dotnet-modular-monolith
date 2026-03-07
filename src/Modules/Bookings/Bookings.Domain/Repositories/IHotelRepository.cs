using Bookings.Domain.Entities;
using Shared.Domain.Repositories;

namespace Bookings.Domain.Repositories;

public interface IHotelRepository : IRepository<Hotel>
{
    Task<Hotel?> GetWithRoomsAsync(Guid hotelId, CancellationToken cancellationToken = default);
    Task<Room?> GetRoomAsync(Guid roomId, CancellationToken cancellationToken = default);
}
