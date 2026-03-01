namespace Shared.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Services;
using Shared.Infrastructure.Persistence.Interceptors;
using Shared.Infrastructure.Services;

public static class CoreServicesExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        AddTimeAbstraction(services);
        AddUserContextServices(services);
        AddAuditingServices(services);

        return services;
    }

    private static void AddTimeAbstraction(IServiceCollection services)
    {
        services.AddSingleton<ISystemClock, SystemClock>();
    }

    private static void AddUserContextServices(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();
    }

    private static void AddAuditingServices(IServiceCollection services)
    {
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventOutboxInterceptor>();
    }
}
