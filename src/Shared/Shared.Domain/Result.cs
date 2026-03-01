namespace Shared.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string error, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Successful result cannot have an error");

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failed result must have an error");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    public static Result Failure(string error, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
        => new(false, error, errorCode);

    public static Result<T> Failure<T>(string error, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
        => new(default!, false, error, errorCode);
}

public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }
}
