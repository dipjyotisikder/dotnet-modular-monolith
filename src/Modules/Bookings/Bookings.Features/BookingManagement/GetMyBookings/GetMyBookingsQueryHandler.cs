using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;

namespace Bookings.Features.BookingManagement.GetMyBookings;

public class GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    : IRequestHandler<GetMyBookingsQuery, Result<IEnumerable<BookingSummary>>>
{
    public async Task<Result<IEnumerable<BookingSummary>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bookings = await bookingRepository.GetByGuestIdAsync(request.GuestId, cancellationToken);

            var response = bookings.Select(b => new BookingSummary(
                b.Id,
                b.HotelId,
                b.RoomId,
                b.DateRange.CheckIn,
                b.DateRange.CheckOut,
                b.DateRange.Nights,
                b.TotalAmount.Amount,
                b.TotalAmount.Currency,
                b.Status.ToString(),
                b.CreatedAt));

            return Result.Success(response);
        }
        catch (Exception)
        {
            return Result.Failure<IEnumerable<BookingSummary>>(
                "An error occurred while retrieving bookings", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
