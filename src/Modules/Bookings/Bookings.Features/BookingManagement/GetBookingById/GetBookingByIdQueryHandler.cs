using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.GetBookingById;

public class GetBookingByIdQueryHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingByIdQuery, Result<GetBookingByIdResponse>>
{
    public async Task<Result<GetBookingByIdResponse>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            return Result.NotFound<GetBookingByIdResponse>("Booking not found");

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
}
