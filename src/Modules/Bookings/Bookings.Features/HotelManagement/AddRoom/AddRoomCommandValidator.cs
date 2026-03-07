using FluentValidation;

namespace Bookings.Features.HotelManagement.AddRoom;

public class AddRoomCommandValidator : AbstractValidator<AddRoomCommand>
{
    public AddRoomCommandValidator()
    {
        RuleFor(x => x.HotelId)
            .NotEmpty().WithMessage("Hotel ID is required");

        RuleFor(x => x.RoomNumber)
            .NotEmpty().WithMessage("Room number is required")
            .MaximumLength(20).WithMessage("Room number cannot exceed 20 characters");

        RuleFor(x => x.PricePerNight)
            .GreaterThan(0).WithMessage("Price per night must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g. USD)");

        RuleFor(x => x.MaxOccupancy)
            .GreaterThan(0).WithMessage("Max occupancy must be at least 1");
    }
}
