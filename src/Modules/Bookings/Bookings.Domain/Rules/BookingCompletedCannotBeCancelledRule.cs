using Bookings.Domain.Enums;
using Shared.Domain;
using Shared.Domain.Validation;

namespace Bookings.Domain.Rules;

public sealed class BookingCompletedCannotBeCancelledRule(BookingStatus status) : IBusinessRule
{
    public bool IsBroken() => status == BookingStatus.Completed;

    public string Message => "Completed bookings cannot be cancelled";

    public string ErrorCode => ErrorCodes.BUSINESS_RULE_VIOLATION;
}
