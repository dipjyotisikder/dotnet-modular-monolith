namespace Shared.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.BackgroundJobs;
using Shared.Infrastructure.Configuration.Options;
using Shared.Infrastructure.Messaging;
using Shared.Infrastructure.Persistence;
using Shared.Infrastructure.Repositories;

public static class OutboxModule
{
    public static IServiceCollection AddOutboxModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OutboxDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOutboxRepository, OutboxRepository>();

        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

        var outboxOptions = configuration.GetSection(OutboxOptions.SectionName)
            .Get<OutboxOptions>();

        if (outboxOptions?.Enabled == true)
        {
            services.AddHostedService<OutboxProcessor>();
            services.AddHostedService<OutboxCleanupJob>();
        }

        return services;
    }
}
