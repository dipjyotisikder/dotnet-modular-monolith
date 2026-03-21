using Bookings.Domain.Enums;
using Bookings.Domain.Events;
using Bookings.Domain.Rules;
using Bookings.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Validation;

namespace Bookings.Domain.Entities;

public class Booking : AuditableEntity
{
    public Guid GuestId { get; private set; }
    public Guid HotelId { get; private set; }
    public Guid RoomId { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public BookingStatus Status { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private Booking() { }

    public static Result<Booking> Create(
        Guid guestId,
        Guid hotelId,
        Guid roomId,
        DateRange dateRange,
        Money pricePerNight)
    {
        if (guestId == Guid.Empty)
            return Result.Failure<Booking>("Guest ID cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (hotelId == Guid.Empty)
            return Result.Failure<Booking>("Hotel ID cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (roomId == Guid.Empty)
            return Result.Failure<Booking>("Room ID cannot be empty", ErrorCodes.VALIDATION_ERROR);

        var totalAmount = pricePerNight.Multiply(dateRange.Nights);

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            GuestId = guestId,
            HotelId = hotelId,
            RoomId = roomId,
            DateRange = dateRange,
            TotalAmount = totalAmount,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        booking.AddDomainEvent(new BookingCreatedDomainEvent(
            booking.Id,
            guestId,
            hotelId,
            roomId,
            dateRange.CheckIn,
            dateRange.CheckOut,
            totalAmount.Amount));

        return Result.Success(booking);
    }

    public Result Cancel(string reason = "Cancelled by guest")
    {
        try
        {
            BusinessRuleValidator.CheckRule(new BookingAlreadyCancelledRule(Status));
            BusinessRuleValidator.CheckRule(new BookingCompletedCannotBeCancelledRule(Status));
            BusinessRuleValidator.CheckRule(new CannotCancelAfterCheckInRule(DateRange));
        }
        catch (BusinessRuleViolationException ex)
        {
            return Result.Failure(ex.Message, ex.ErrorCode);
        }

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new BookingCancelledDomainEvent(Id, GuestId, HotelId, RoomId, reason));
        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != BookingStatus.Confirmed)
            return Result.Failure("Only confirmed bookings can be completed", ErrorCodes.BUSINESS_RULE_VIOLATION);

        Status = BookingStatus.Completed;
        AddDomainEvent(new BookingCompletedDomainEvent(Id, GuestId, HotelId));
        return Result.Success();
    }

    public bool BelongsTo(Guid userId) => GuestId == userId;
}
