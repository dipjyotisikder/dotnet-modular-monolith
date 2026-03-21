namespace Shared.Domain.Services;

public interface IPermissionService
{
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool HasAllPermissions(params string[] permissions);
    IReadOnlySet<string> GetUserPermissions();
    bool CanAccessUserResource(Guid resourceOwnerId);
}
