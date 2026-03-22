using Shared.Domain.Authorization;
using Shared.Domain.Services;

namespace Shared.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly IUserContext _userContext;
    private IReadOnlySet<string>? _cachedPermissions;

    public PermissionService(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public bool HasPermission(string permission)
    {
        if (!_userContext.IsAuthenticated)
            return false;

        var permissions = GetUserPermissions();
        return permissions.Contains(permission.ToLowerInvariant());
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (!_userContext.IsAuthenticated || permissions.Length == 0)
            return false;

        var userPermissions = GetUserPermissions();
        return permissions.Any(p => userPermissions.Contains(p.ToLowerInvariant()));
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (!_userContext.IsAuthenticated || permissions.Length == 0)
            return false;

        var userPermissions = GetUserPermissions();
        return permissions.All(p => userPermissions.Contains(p.ToLowerInvariant()));
    }

    public IReadOnlySet<string> GetUserPermissions()
    {
        if (_cachedPermissions != null)
            return _cachedPermissions;

        var permissionSet = new HashSet<string>();

        foreach (var permission in _userContext.Permissions)
        {
            permissionSet.Add(permission.ToLowerInvariant());
        }

        if (permissionSet.Count > 0)
        {
            _cachedPermissions = permissionSet.AsReadOnly();
            return _cachedPermissions;
        }

        foreach (var role in _userContext.Roles)
        {
            var rolePermissions = RolePermissions.GetPermissionsForRole(role);
            foreach (var permission in rolePermissions)
            {
                permissionSet.Add(permission.ToLowerInvariant());
            }
        }

        _cachedPermissions = permissionSet.AsReadOnly();
        return _cachedPermissions;
    }

    public bool CanAccessUserResource(Guid resourceOwnerId)
    {
        if (_userContext.Roles.Contains("Admin"))
            return true;

        return _userContext.UserId == resourceOwnerId;
    }
}
