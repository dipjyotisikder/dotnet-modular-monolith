using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;

namespace Bookings.Features.BookingManagement.CompleteBooking;

public class CompleteBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteBookingCommand, Result>
{
    public async Task<Result> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return Result.Failure("Booking not found", ErrorCodes.RESOURCE_NOT_FOUND);

            if (booking.DateRange.CheckOut > DateTime.UtcNow.Date)
                return Result.Failure(
                    "Booking cannot be completed before the check-out date",
                    ErrorCodes.BUSINESS_RULE_VIOLATION);

            var completeResult = booking.Complete();
            if (completeResult.IsFailure)
                return completeResult;

            bookingRepository.Update(booking);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure("An error occurred while completing the booking", ErrorCodes.INTERNAL_ERROR);
        }
    }
}
