using System.Security.Claims;
using Users.Domain.Entities;

namespace Users.Domain.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string deviceId);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
