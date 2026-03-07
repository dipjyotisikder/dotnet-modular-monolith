using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.GetHotelById;

public record RoomSummary(
    Guid Id,
    string RoomNumber,
    string RoomType,
    decimal PricePerNight,
    string Currency,
    int MaxOccupancy,
    string? Description,
    bool IsAvailable);

public record GetHotelByIdResponse(
    Guid Id,
    string Name,
    string Description,
    int StarRating,
    string Street,
    string City,
    string Country,
    string ZipCode,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<RoomSummary> Rooms);

public record GetHotelByIdQuery(Guid HotelId) : IRequest<Result<GetHotelByIdResponse>>;
