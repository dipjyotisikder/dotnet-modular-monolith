using AppHost.Infrastructure.Persistence;
using Bookings.Features;
using Shared.Application.Configuration;
using Shared.Infrastructure.Configuration;
using Users.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptionsConfiguration(builder.Configuration);

builder.Services.AddCoreServices();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddApiInfrastructure();

builder.Services.AddDistributedLocksModule(builder.Configuration);
builder.Services.AddOutboxModule(builder.Configuration);

builder.Services.RegisterCqrsHandlers(
    typeof(Users.Features.DependencyInjection).Assembly,
    typeof(Bookings.Features.DependencyInjection).Assembly);

builder.Services.AddUsersFeatures(builder.Configuration);
builder.Services.AddBookingsFeatures(builder.Configuration);

builder.Services.AddScoped<SeederRunner>();

builder.Services.AddApplicationServices();

var app = builder.Build();

await app.ApplyMigrationsAsync();
await app.SeedAsync();

app.UseApiInfrastructure();

app.MapUsersEndpoints();
app.MapBookingsEndpoints();

await app.RunAsync();
