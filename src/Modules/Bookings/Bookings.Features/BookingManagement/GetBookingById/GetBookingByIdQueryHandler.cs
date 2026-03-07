using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.GetBookingById;

public class GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingByIdQuery, Result<GetBookingByIdResponse>>
{
    public async Task<Result<GetBookingByIdResponse>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return Result.Failure<GetBookingByIdResponse>("Booking not found", ErrorCodes.RESOURCE_NOT_FOUND);

            if (!booking.BelongsTo(request.RequestingUserId))
                return Result.Failure<GetBookingByIdResponse>(
                    "You are not authorized to view this booking", ErrorCodes.FORBIDDEN);

            var response = new GetBookingByIdResponse(
                booking.Id,
                booking.GuestId,
                booking.HotelId,
                booking.RoomId,
                booking.DateRange.CheckIn,
                booking.DateRange.CheckOut,
                booking.DateRange.Nights,
                booking.TotalAmount.Amount,
                booking.TotalAmount.Currency,
                booking.Status.ToString(),
                booking.CreatedAt,
                booking.CancelledAt,
                booking.CancellationReason);

            return Result.Success(response);
        }
        catch (Exception)
        {
            return Result.Failure<GetBookingByIdResponse>(
                "An error occurred while retrieving the booking", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
