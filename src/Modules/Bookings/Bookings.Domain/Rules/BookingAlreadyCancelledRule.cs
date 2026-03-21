using Bookings.Domain.Enums;
using Shared.Domain;
using Shared.Domain.Validation;

namespace Bookings.Domain.Rules;

public sealed class BookingAlreadyCancelledRule(BookingStatus status) : IBusinessRule
{
    public bool IsBroken() => status == BookingStatus.Cancelled;

    public string Message => "Booking is already cancelled";

    public string ErrorCode => ErrorCodes.BUSINESS_RULE_VIOLATION;
}
