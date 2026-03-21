using Bookings.Domain.Entities;
using Bookings.Domain.Repositories;
using Bookings.Domain.ValueObjects;
using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;

namespace Bookings.Features.BookingManagement.CreateBooking;

public class CreateBookingCommandHandler(
    IHotelRepository hotelRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBookingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var dateRangeResult = DateRange.Create(request.CheckIn, request.CheckOut);
        if (dateRangeResult.IsFailure)
            return Result.Failure<Guid>(dateRangeResult.Error, dateRangeResult.ErrorCode);

        var hotel = await hotelRepository.GetWithRoomsAsync(request.HotelId, cancellationToken);
        if (hotel is null)
            return Result.NotFound<Guid>("Hotel not found");

        if (!hotel.IsActive)
            return Result.BusinessRuleViolation<Guid>("Hotel is not accepting bookings");

        var room = hotel.Rooms.FirstOrDefault(r => r.Id == request.RoomId);
        if (room is null)
            return Result.NotFound<Guid>("Room not found in the specified hotel");

        if (!room.IsAvailable)
            return Result.BusinessRuleViolation<Guid>("Room is not available for booking");

        var hasConflict = await bookingRepository.HasOverlappingBookingAsync(
            room.Id, dateRangeResult.Value.CheckIn, dateRangeResult.Value.CheckOut, cancellationToken);

        if (hasConflict)
            return Result.BusinessRuleViolation<Guid>(
                "Room is already booked for the selected dates");

        var bookingResult = Booking.Create(
            request.GuestId,
            hotel.Id,
            room.Id,
            dateRangeResult.Value,
            room.PricePerNight);

        if (bookingResult.IsFailure)
            return Result.Failure<Guid>(bookingResult.Error, bookingResult.ErrorCode);

        await bookingRepository.AddAsync(bookingResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(bookingResult.Value.Id);
    }
}
