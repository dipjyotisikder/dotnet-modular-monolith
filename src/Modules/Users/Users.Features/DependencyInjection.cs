using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Configuration;

namespace Users.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersFeatures(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.RegisterCqrsHandlers(typeof(DependencyInjection).Assembly);

        return services;
    }
}
