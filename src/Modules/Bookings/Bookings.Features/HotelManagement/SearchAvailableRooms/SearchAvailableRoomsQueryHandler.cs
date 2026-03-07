using Bookings.Domain.Repositories;
using Bookings.Domain.ValueObjects;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.HotelManagement.SearchAvailableRooms;

public class SearchAvailableRoomsQueryHandler(
    IHotelRepository hotelRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<SearchAvailableRoomsQuery, Result<IEnumerable<AvailableRoomResponse>>>
{
    public async Task<Result<IEnumerable<AvailableRoomResponse>>> Handle(
        SearchAvailableRoomsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dateRangeResult = DateRange.Create(request.CheckIn, request.CheckOut);
            if (dateRangeResult.IsFailure)
                return Result.Failure<IEnumerable<AvailableRoomResponse>>(
                    dateRangeResult.Error, dateRangeResult.ErrorCode);

            var hotel = await hotelRepository.GetWithRoomsAsync(request.HotelId, cancellationToken);
            if (hotel is null)
                return Result.Failure<IEnumerable<AvailableRoomResponse>>(
                    "Hotel not found", ErrorCodes.RESOURCE_NOT_FOUND);

            if (!hotel.IsActive)
                return Result.Failure<IEnumerable<AvailableRoomResponse>>(
                    "Hotel is not accepting bookings", ErrorCodes.BUSINESS_RULE_VIOLATION);

            var dateRange = dateRangeResult.Value;
            var availableRooms = new List<AvailableRoomResponse>();

            foreach (var room in hotel.Rooms.Where(r => r.IsAvailable))
            {
                var hasConflict = await bookingRepository.HasOverlappingBookingAsync(
                    room.Id, dateRange.CheckIn, dateRange.CheckOut, cancellationToken);

                if (!hasConflict)
                {
                    availableRooms.Add(new AvailableRoomResponse(
                        room.Id,
                        room.RoomNumber,
                        room.RoomType.ToString(),
                        room.PricePerNight.Amount,
                        room.PricePerNight.Currency,
                        room.MaxOccupancy,
                        room.Description,
                        dateRange.Nights,
                        room.PricePerNight.Amount * dateRange.Nights));
                }
            }

            return Result.Success(availableRooms.AsEnumerable());
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<AvailableRoomResponse>>(
                "An error occurred while searching available rooms", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
