using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Configuration;
using Shared.Domain.Authorization;
using Shared.Domain.Services;
using Shared.Infrastructure.Services;

namespace Shared.Infrastructure.Configuration;

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionBasedAuthorization(
        this IServiceCollection services)
    {
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRequirementEvaluator, RequirementEvaluator>();

        services.RegisterAuthorizationHandlers(typeof(PermissionAuthorizationExtensions).Assembly);

        return services;
    }
}
