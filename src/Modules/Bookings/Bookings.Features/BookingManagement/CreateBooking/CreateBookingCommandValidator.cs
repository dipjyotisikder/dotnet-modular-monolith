using FluentValidation;

namespace Bookings.Features.BookingManagement.CreateBooking;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.GuestId)
            .NotEmpty().WithMessage("Guest ID is required");

        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required");

        RuleFor(x => x.CheckIn)
            .NotEmpty().WithMessage("Check-in date is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Check-in date cannot be in the past");

        RuleFor(x => x.CheckOut)
            .NotEmpty().WithMessage("Check-out date is required")
            .GreaterThan(x => x.CheckIn).WithMessage("Check-out date must be after check-in date");
    }
}
