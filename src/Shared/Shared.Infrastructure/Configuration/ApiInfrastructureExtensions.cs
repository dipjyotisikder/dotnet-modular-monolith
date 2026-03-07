namespace Shared.Infrastructure.Configuration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Middleware;

public static class ApiInfrastructureExtensions
{
    public static IServiceCollection AddApiInfrastructure(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddRateLimiter(_ => { });

        RegisterSwaggerDocumentation(services);
        RegisterGlobalExceptionHandling(services);
        RegisterHealthCheckServices(services);
        RegisterCorsPolicy(services);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    public static WebApplication UseApiInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseCors(CorsConfiguration.DefaultPolicy);
        MapSwaggerUIEndpoints(app);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHealthChecks("/health").AllowAnonymous();

        return app;
    }

    private static void RegisterSwaggerDocumentation(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "Monolithic API",
                Version = "v1",
                Description = "Clean Architecture + CQRS + DDD + JWT Auth + OAuth",
                Contact = new()
                {
                    Name = "Monolith Admin",
                    Url = new Uri("https://github.com/dipjyotisikder")
                }
            });
        });
    }

    private static void RegisterGlobalExceptionHandling(IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }

    private static void RegisterHealthCheckServices(IServiceCollection services)
    {
        services.AddHealthChecks();
    }

    private static void RegisterCorsPolicy(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsConfiguration.DefaultPolicy, builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });
    }

    private static void MapSwaggerUIEndpoints(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.RoutePrefix = string.Empty;
            x.SwaggerEndpoint("/swagger/v1/swagger.json", "Monolithic API v1");
            x.DocumentTitle = "Monolithic API";
        });
    }
}
