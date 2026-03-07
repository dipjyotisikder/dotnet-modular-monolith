using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.GetHotels;

public record GetHotelsResponse(
    Guid Id,
    string Name,
    string Description,
    int StarRating,
    string City,
    string Country,
    bool IsActive,
    int RoomCount);

public record GetHotelsQuery : IRequest<Result<IEnumerable<GetHotelsResponse>>>;
