namespace Shared.Domain;

public static class ErrorCodes
{
    public const string VALIDATION_ERROR = "validation_error";
    public const string BUSINESS_RULE_VIOLATION = "business_rule_violation";
    public const string RESOURCE_NOT_FOUND = "resource_not_found";
    public const string DUPLICATE_RESOURCE = "duplicate_resource";
    public const string CONFLICT = "conflict";
    public const string UNAUTHORIZED = "unauthorized";
    public const string FORBIDDEN = "forbidden";
    public const string PERMISSION_DENIED = "permission_denied";
    public const string AUTHENTICATION_REQUIRED = "authentication_required";
    public const string REQUIREMENT_FAILED = "requirement_failed";
    public const string EXPIRED = "expired";
    public const string INVALID_STATE = "invalid_state";
    public const string CONCURRENCY_CONFLICT = "concurrency_conflict";
    public const string SERVICE_UNAVAILABLE = "service_unavailable";
    public const string RATE_LIMITED = "rate_limited";
    public const string DATABASE_ERROR = "database_error";
    public const string EXTERNAL_SERVICE_ERROR = "external_service_error";
    public const string TIMEOUT = "timeout";
    public const string RESOURCE_EXHAUSTED = "resource_exhausted";
    public const string INTERNAL_ERROR = "internal_error";
    public const string SYSTEM_ERROR = "system_error";
}
