namespace Shared.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Services;
using Authentication;
using Persistence.Interceptors;
using Services;

public static class CoreServicesExtensions
{
    public static void AddCoreServices(this IServiceCollection services)
    {
        AddTimeAbstraction(services);
        AddAuditingServices(services);
        AddAuthenticationServices(services);
    }

    private static void AddTimeAbstraction(IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
    }

    private static void AddAuditingServices(IServiceCollection services)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventOutboxInterceptor>();
    }

    private static void AddAuthenticationServices(IServiceCollection services)
    {
        services.AddScoped<OAuthClaimsTransformationHandler>();
    }
}
