using Bookings.Domain.ValueObjects;
using Shared.Domain;
using Shared.Domain.Validation;

namespace Bookings.Domain.Rules;

public sealed class CannotCancelAfterCheckInRule(DateRange dateRange) : IBusinessRule
{
    public bool IsBroken() => dateRange.CheckIn <= DateTime.UtcNow.Date;

    public string Message => "Cannot cancel a booking on or after the check-in date";

    public string ErrorCode => ErrorCodes.BUSINESS_RULE_VIOLATION;
}
