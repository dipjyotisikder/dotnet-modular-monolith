using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.SearchAvailableRooms;

public record AvailableRoomResponse(
    Guid RoomId,
    string RoomNumber,
    string RoomType,
    decimal PricePerNight,
    string Currency,
    int MaxOccupancy,
    string? Description,
    int Nights,
    decimal TotalPrice);

public record SearchAvailableRoomsQuery(
    Guid HotelId,
    DateTime CheckIn,
    DateTime CheckOut) : IRequest<Result<IEnumerable<AvailableRoomResponse>>>;
