using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure.Configuration.Options;
using System.Text;

namespace Shared.Infrastructure.Configuration;

public static class AuthenticationConfigurationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT CONFIGURATION IS MISSING");

        var oauthOptions = configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>()
            ?? throw new InvalidOperationException("OAUTH CONFIGURATION IS MISSING");

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        if (oauthOptions.Google.Enabled && !string.IsNullOrEmpty(oauthOptions.Google.ClientId))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = oauthOptions.Google.ClientId;
                options.ClientSecret = oauthOptions.Google.ClientSecret;
            });
        }

        if (oauthOptions.Microsoft.Enabled && !string.IsNullOrEmpty(oauthOptions.Microsoft.ClientId))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = oauthOptions.Microsoft.ClientId;
                options.ClientSecret = oauthOptions.Microsoft.ClientSecret;
            });
        }

        _ = services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Admin"));
            options.AddPolicy("ProTierPolicy", policy => policy.RequireClaim("tier", "pro", "enterprise"));
            options.AddPolicy("EnterpriseTierPolicy", policy => policy.RequireClaim("tier", "enterprise"));
        });

        return services;
    }
}
