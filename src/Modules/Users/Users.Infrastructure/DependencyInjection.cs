using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Services;
using Users.Domain.Repositories;
using Users.Domain.Services;
using Users.Infrastructure.Options;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<TokenRevocationJwtBearerEvents>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, HttpUserContext>();

        services.Configure<JwtBearerOptions>(
            JwtBearerDefaults.AuthenticationScheme,
            options => options.EventsType = typeof(TokenRevocationJwtBearerEvents));

        return services;
    }
}
