using Microsoft.AspNetCore.Http;
using Shared.Domain.Services;
using System.Security.Claims;

namespace Users.Infrastructure.Services;

public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public string? UserEmail => _httpContextAccessor.HttpContext?.User
        .FindFirst(ClaimTypes.Email)?.Value;

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?.User
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? [];

            return roles.AsReadOnly();
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string? Tier => _httpContextAccessor.HttpContext?.User
        .FindFirst("tier")?.Value ?? "standard";

    public IReadOnlyList<string> Permissions
    {
        get
        {
            var permissions = _httpContextAccessor.HttpContext?.User
                .FindAll("permission")
                .Select(c => c.Value)
                .ToList() ?? [];

            return permissions.AsReadOnly();
        }
    }
}
