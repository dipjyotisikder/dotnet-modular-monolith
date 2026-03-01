namespace Shared.Domain;

public static class ErrorCodes
{
    public const string VALIDATION_ERROR = "validation_error";
    public const string BUSINESS_RULE_VIOLATION = "business_rule_violation";
    public const string RESOURCE_NOT_FOUND = "resource_not_found";
    public const string DUPLICATE_RESOURCE = "duplicate_resource";
    public const string INTERNAL_ERROR = "internal_error";
    public const string UNAUTHORIZED = "unauthorized";
    public const string FORBIDDEN = "forbidden";
}
