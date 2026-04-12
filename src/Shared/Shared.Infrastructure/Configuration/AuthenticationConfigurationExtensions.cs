using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure.Authentication;
using Shared.Infrastructure.Configuration.Options;
using System.Text;

namespace Shared.Infrastructure.Configuration;

public static class AuthenticationConfigurationExtensions
{
    public static void AddAuthenticationConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT CONFIGURATION IS MISSING");

        var oauthOptions = configuration.GetSection(OAuthOptions.SectionName).Get<OAuthOptions>()
            ?? throw new InvalidOperationException("OAUTH CONFIGURATION IS MISSING");

        services.AddSingleton(oauthOptions);

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
            var claimsHandler = new OAuthClaimsTransformationHandler(oauthOptions);
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = oauthOptions.Google.ClientId;
                options.ClientSecret = oauthOptions.Google.ClientSecret;
                options.Events.OnCreatingTicket += claimsHandler.MapGoogleClaims;
            });
        }

        services.AddAuthorization();
    }
}
