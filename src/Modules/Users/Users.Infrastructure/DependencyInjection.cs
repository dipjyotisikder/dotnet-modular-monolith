using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Services;
using Shared.Infrastructure.Configuration;
using Shared.Infrastructure.Seeding;
using Users.Domain.Services;
using Users.Infrastructure.Options;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Services;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<PasswordHasherOptions>(
            configuration.GetSection("PasswordHasher"));

        services.AddRepositoriesFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();

        services.AddScoped<TokenRevocationJwtBearerEvents>();
        services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options => options.EventsType = typeof(TokenRevocationJwtBearerEvents));

        services.AddSeeding(typeof(DependencyInjection).Assembly);

        return services;
    }
}
