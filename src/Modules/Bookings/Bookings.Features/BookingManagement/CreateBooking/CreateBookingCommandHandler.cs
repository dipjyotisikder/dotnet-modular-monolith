using Bookings.Domain.Entities;
using Bookings.Domain.Repositories;
using Bookings.Domain.ValueObjects;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.CreateBooking;

public class CreateBookingCommandHandler(
    IHotelRepository hotelRepository,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateBookingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var dateRangeResult = DateRange.Create(request.CheckIn, request.CheckOut);
            if (dateRangeResult.IsFailure)
                return Result.Failure<Guid>(dateRangeResult.Error, dateRangeResult.ErrorCode);

            var hotel = await hotelRepository.GetWithRoomsAsync(request.HotelId, cancellationToken);
            if (hotel is null)
                return Result.Failure<Guid>("Hotel not found", ErrorCodes.RESOURCE_NOT_FOUND);

            if (!hotel.IsActive)
                return Result.Failure<Guid>("Hotel is not accepting bookings", ErrorCodes.BUSINESS_RULE_VIOLATION);

            var room = hotel.Rooms.FirstOrDefault(r => r.Id == request.RoomId);
            if (room is null)
                return Result.Failure<Guid>("Room not found in the specified hotel", ErrorCodes.RESOURCE_NOT_FOUND);

            if (!room.IsAvailable)
                return Result.Failure<Guid>("Room is not available for booking", ErrorCodes.BUSINESS_RULE_VIOLATION);

            var hasConflict = await bookingRepository.HasOverlappingBookingAsync(
                room.Id, dateRangeResult.Value.CheckIn, dateRangeResult.Value.CheckOut, cancellationToken);

            if (hasConflict)
                return Result.Failure<Guid>(
                    "Room is already booked for the selected dates",
                    ErrorCodes.BUSINESS_RULE_VIOLATION);

            var bookingResult = Booking.Create(
                request.GuestId,
                hotel.Id,
                room.Id,
                dateRangeResult.Value,
                room.PricePerNight);

            if (bookingResult.IsFailure)
                return Result.Failure<Guid>(bookingResult.Error, bookingResult.ErrorCode);

            await bookingRepository.AddAsync(bookingResult.Value, cancellationToken);

            return Result.Success(bookingResult.Value.Id);
        }
        catch (Exception)
        {
            return Result.Failure<Guid>("An error occurred while creating the booking", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
