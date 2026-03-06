using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Users.Domain.Repositories;

namespace Users.Infrastructure.Services;

public class TokenRevocationJwtBearerEvents(IUserRepository userRepository) : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tokenVersionClaim = context.Principal?.FindFirst("token_version")?.Value;

        if (userIdClaim is null
            || !Guid.TryParse(userIdClaim, out var userId)
            || tokenVersionClaim is null)
        {
            context.Fail("Missing required token claims.");
            return;
        }

        var user = await userRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

        if (user is null)
        {
            context.Fail("User not found.");
            return;
        }

        if (!user.IsActive)
        {
            context.Fail("User account is deactivated.");
            return;
        }

        if (!int.TryParse(tokenVersionClaim, out var tokenVersion)
            || tokenVersion != user.TokenRevocationVersion.AccessTokenVersion)
        {
            context.Fail("Token has been revoked.");
            return;
        }

        await base.TokenValidated(context);
    }
}
