using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.GetHotels;

public class GetHotelsQueryHandler(IHotelRepository hotelRepository)
    : IRequestHandler<GetHotelsQuery, Result<IEnumerable<GetHotelsResponse>>>
{
    public async Task<Result<IEnumerable<GetHotelsResponse>>> Handle(GetHotelsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var hotels = await hotelRepository.GetAllAsync(cancellationToken);

            var response = hotels
                .Where(h => h.IsActive)
                .Select(h => new GetHotelsResponse(
                    h.Id,
                    h.Name,
                    h.Description,
                    h.StarRating,
                    h.Address.City,
                    h.Address.Country,
                    h.IsActive,
                    h.Rooms.Count))
                .ToList();

            return Result.Success(response.AsEnumerable());
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<GetHotelsResponse>>("An error occurred while retrieving hotels", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
