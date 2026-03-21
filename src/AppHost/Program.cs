using AppHost.Infrastructure.Persistence;
using AppHost.Infrastructure.Seeding;
using Bookings.Features;
using Bookings.Infrastructure;
using Shared.Application.Configuration;
using Shared.Infrastructure.Configuration;
using Shared.Infrastructure.Seeding;
using Users.Features;
using Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptionsConfiguration(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddPermissionBasedAuthorization();
builder.Services.AddApiInfrastructure();

builder.Services.AddDistributedLocksModule(builder.Configuration);
builder.Services.AddOutboxModule(builder.Configuration);

builder.Services
    .AddUsersFeatures()
    .AddUsersInfrastructure(builder.Configuration);

builder.Services
    .AddBookingsFeatures()
    .AddBookingsInfrastructure(builder.Configuration);

builder.Services.AddScoped<SeederRunner>();
builder.Services.AddApplicationServices();

var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.SeedAsync();

app.UseApiInfrastructure();
app.MapUsersEndpoints();
app.MapBookingsEndpoints();

await app.RunAsync();
