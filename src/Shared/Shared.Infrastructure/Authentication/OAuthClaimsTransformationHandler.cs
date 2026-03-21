using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Text.Json;

namespace Shared.Infrastructure.Authentication;

public class OAuthClaimsTransformationHandler
{
    private readonly Shared.Infrastructure.Configuration.Options.ClaimsMappingOptions _mapping;

    public OAuthClaimsTransformationHandler(Shared.Infrastructure.Configuration.Options.OAuthOptions oauthOptions)
    {
        _mapping = oauthOptions.Google.ClaimsMapping;
    }

    public Task MapGoogleClaims(OAuthCreatingTicketContext context)
    {
        try
        {
            var json = context.User.GetRawText();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var identity = context.Identity;

            if (identity == null)
                return Task.CompletedTask;

            AddUserIdClaim(identity, root);
            AddEmailClaim(identity, root);
            AddRoleClaim(identity, root);
            AddTierClaim(identity, root);
            AddPermissionsClaim(identity, root);
        }
        catch
        {
            context.Identity?.AddClaim(new Claim(ClaimTypes.Role, _mapping.DefaultRole));
        }

        return Task.CompletedTask;
    }

    private void AddUserIdClaim(ClaimsIdentity identity, JsonElement root)
    {
        if (root.TryGetProperty(_mapping.UserIdClaim, out var userIdElement))
        {
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userIdElement.GetString() ?? ""));
        }
    }

    private void AddEmailClaim(ClaimsIdentity identity, JsonElement root)
    {
        if (root.TryGetProperty(_mapping.EmailClaim, out var emailElement))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, emailElement.GetString() ?? ""));
        }
    }

    private void AddRoleClaim(ClaimsIdentity identity, JsonElement root)
    {
        var roleAdded = false;

        if (root.TryGetProperty(_mapping.RoleClaim, out var roleElement))
        {
            var providerRole = roleElement.GetString()?.ToLower();
            if (providerRole != null && _mapping.RoleMapping.TryGetValue(providerRole, out var mappedRole))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, mappedRole));
                roleAdded = true;
            }
        }

        if (!roleAdded)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, _mapping.DefaultRole));
        }
    }

    private void AddTierClaim(ClaimsIdentity identity, JsonElement root)
    {
        if (root.TryGetProperty(_mapping.TierClaim, out var tierElement))
        {
            identity.AddClaim(new Claim("tier", tierElement.GetString() ?? "standard"));
        }
        else
        {
            identity.AddClaim(new Claim("tier", "standard"));
        }
    }

    private void AddPermissionsClaim(ClaimsIdentity identity, JsonElement root)
    {
        if (root.TryGetProperty("permissions", out var permsElement) && permsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var perm in permsElement.EnumerateArray())
            {
                identity.AddClaim(new Claim("permission", perm.GetString() ?? ""));
            }
        }
    }
}
