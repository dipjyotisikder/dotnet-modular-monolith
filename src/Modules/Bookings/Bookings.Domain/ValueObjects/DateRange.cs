using Shared.Domain;

namespace Bookings.Domain.ValueObjects;

public class DateRange
{
    public DateTime CheckIn { get; private set; }
    public DateTime CheckOut { get; private set; }

    private DateRange() { }

    public int Nights => (CheckOut.Date - CheckIn.Date).Days;

    public static Result<DateRange> Create(DateTime checkIn, DateTime checkOut)
    {
        if (checkIn.Date >= checkOut.Date)
            return Result.Failure<DateRange>(
                "Check-out date must be after check-in date",
                ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new DateRange
        {
            CheckIn = checkIn.Date,
            CheckOut = checkOut.Date
        });
    }

    public bool OverlapsWith(DateRange other) =>
        CheckIn < other.CheckOut && CheckOut > other.CheckIn;
}
