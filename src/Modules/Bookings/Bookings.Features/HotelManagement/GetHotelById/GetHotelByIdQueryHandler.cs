using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.GetHotelById;

public class GetHotelByIdQueryHandler(IHotelRepository hotelRepository)
    : IRequestHandler<GetHotelByIdQuery, Result<GetHotelByIdResponse>>
{
    public async Task<Result<GetHotelByIdResponse>> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await hotelRepository.GetWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
            return Result.NotFound<GetHotelByIdResponse>("Hotel not found");

        var rooms = hotel.Rooms.Select(r => new RoomSummary(
            r.Id,
            r.RoomNumber,
            r.RoomType.ToString(),
            r.PricePerNight.Amount,
            r.PricePerNight.Currency,
            r.MaxOccupancy,
            r.Description,
            r.IsAvailable));

        var response = new GetHotelByIdResponse(
            hotel.Id,
            hotel.Name,
            hotel.Description,
            hotel.StarRating,
            hotel.Address.Street,
            hotel.Address.City,
            hotel.Address.Country,
            hotel.Address.ZipCode,
            hotel.IsActive,
            hotel.CreatedAt,
            rooms);

        return Result.Success(response);
    }
}
