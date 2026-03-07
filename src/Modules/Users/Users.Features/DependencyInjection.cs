using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Configuration;
using Users.Infrastructure;

namespace Users.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersFeatures(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.RegisterCqrsHandlers(typeof(DependencyInjection).Assembly);

        services.AddUsersInfrastructure(configuration);
        return services;
    }
}
