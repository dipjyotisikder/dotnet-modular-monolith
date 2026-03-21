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
            return Result.Failure("Booking not found", ErrorCodes.RESOURCE_NOT_FOUND);

        if (!booking.BelongsTo(request.RequestingUserId))
            return Result.Failure("You are not authorized to cancel this booking", ErrorCodes.FORBIDDEN);

        var cancelResult = booking.Cancel(request.Reason);
        if (cancelResult.IsFailure)
            return cancelResult;

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
