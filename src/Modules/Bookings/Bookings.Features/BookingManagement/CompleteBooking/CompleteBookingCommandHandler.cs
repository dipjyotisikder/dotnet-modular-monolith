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
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            return Result.NotFound("Booking not found");

        if (booking.DateRange.CheckOut > DateTime.UtcNow.Date)
            return Result.BusinessRuleViolation(
                "Booking cannot be completed before the check-out date");

        var completeResult = booking.Complete();
        if (completeResult.IsFailure)
            return completeResult;

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
