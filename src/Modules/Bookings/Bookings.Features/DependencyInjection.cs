using Bookings.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Configuration;

namespace Bookings.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingsFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.RegisterCqrsHandlers(typeof(DependencyInjection).Assembly);

        services.AddBookingsInfrastructure(configuration);
        return services;
    }
}