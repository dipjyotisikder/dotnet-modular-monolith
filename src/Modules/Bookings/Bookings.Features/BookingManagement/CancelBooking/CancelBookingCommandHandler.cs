using Bookings.Domain.Repositories;
using MediatR;
using Shared.Domain;
using Shared.Domain.Repositories;

namespace Bookings.Features.BookingManagement.CancelBooking;

public class CancelBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelBookingCommand, Result>
{
    public async Task<Result> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            return Result.NotFound("Booking not found");

        var cancelResult = booking.Cancel(request.Reason);
        if (cancelResult.IsFailure)
            return cancelResult;

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
