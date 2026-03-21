namespace Shared.Domain.Authorization;

public class AuthorizationException : Exception
{
    public string ErrorCode { get; }
    public string? UserId { get; }
    public string? Permission { get; }
    public string? RequirementType { get; }

    public AuthorizationException(
        string message,
        string errorCode,
        string? userId = null,
        string? permission = null,
        string? requirementType = null)
        : base(message)
    {
        ErrorCode = errorCode;
        UserId = userId;
        Permission = permission;
        RequirementType = requirementType;
    }
}

public class PermissionDeniedException : AuthorizationException
{
    public PermissionDeniedException(string permission, string userId)
        : base(
            $"User does not have permission: {permission}",
            "permission_denied",
            userId,
            permission)
    {
    }
}

public class RequirementFailedException : AuthorizationException
{
    public RequirementFailedException(string requirementType, string userId, string reason = "Authorization requirement failed")
        : base(
            reason,
            "requirement_failed",
            userId,
            requirementType: requirementType)
    {
    }
}

public class AuthenticationRequiredException : AuthorizationException
{
    public AuthenticationRequiredException()
        : base(
            "Authentication is required",
            "authentication_required")
    {
    }
}
