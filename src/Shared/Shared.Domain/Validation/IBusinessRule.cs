using Shared.Domain;

namespace Shared.Domain.Validation;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
    string ErrorCode { get; }
}

public static class BusinessRuleValidator
{
    public static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
            throw new BusinessRuleViolationException(rule.Message, rule.ErrorCode);
    }
}

public class BusinessRuleViolationException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleViolationException(string message, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
