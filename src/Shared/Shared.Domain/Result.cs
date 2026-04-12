namespace Shared.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string error, string errorCode = ErrorCodes.BUSINESS_RULE_VIOLATION)
    {
        switch (isSuccess)
        {
            case true when !string.IsNullOrEmpty(error):
                throw new InvalidOperationException("Successful result cannot have an error");
            case false when string.IsNullOrEmpty(error):
                throw new InvalidOperationException("Failed result must have an error");
        }

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

    public static Result ValidationError(string error)
        => new(false, error, ErrorCodes.VALIDATION_ERROR);

    public static Result<T> ValidationError<T>(string error)
        => new(default!, false, error, ErrorCodes.VALIDATION_ERROR);

    public static Result NotFound(string error)
        => new(false, error, ErrorCodes.RESOURCE_NOT_FOUND);

    public static Result<T> NotFound<T>(string error)
        => new(default!, false, error, ErrorCodes.RESOURCE_NOT_FOUND);

    public static Result Unauthorized(string error)
        => new(false, error, ErrorCodes.UNAUTHORIZED);

    public static Result<T> Unauthorized<T>(string error)
        => new(default!, false, error, ErrorCodes.UNAUTHORIZED);

    public static Result Forbidden(string error)
        => new(false, error, ErrorCodes.FORBIDDEN);

    public static Result<T> Forbidden<T>(string error)
        => new(default!, false, error, ErrorCodes.FORBIDDEN);

    public static Result PermissionDenied(string error)
        => new(false, error, ErrorCodes.PERMISSION_DENIED);

    public static Result<T> PermissionDenied<T>(string error)
        => new(default!, false, error, ErrorCodes.PERMISSION_DENIED);

    public static Result RequirementFailed(string error)
        => new(false, error, ErrorCodes.REQUIREMENT_FAILED);

    public static Result<T> RequirementFailed<T>(string error)
        => new(default!, false, error, ErrorCodes.REQUIREMENT_FAILED);

    public static Result DuplicateResource(string error)
        => new(false, error, ErrorCodes.DUPLICATE_RESOURCE);

    public static Result<T> DuplicateResource<T>(string error)
        => new(default!, false, error, ErrorCodes.DUPLICATE_RESOURCE);

    public static Result BusinessRuleViolation(string error)
        => new(false, error, ErrorCodes.BUSINESS_RULE_VIOLATION);

    public static Result<T> BusinessRuleViolation<T>(string error)
        => new(default!, false, error, ErrorCodes.BUSINESS_RULE_VIOLATION);

    public static Result Conflict(string error)
        => new(false, error, ErrorCodes.CONFLICT);

    public static Result<T> Conflict<T>(string error)
        => new(default!, false, error, ErrorCodes.CONFLICT);
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
