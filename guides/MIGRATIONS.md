# Migration Management Guide

## DbContexts

| Module | DbContext | Migrations Folder |
|--------|-----------|-------------------|
| Users | UsersDbContext | `src/Modules/Users/Users.Infrastructure/Persistence/Migrations` |
| Bookings | BookingsDbContext | `src/Modules/Bookings/Bookings.Infrastructure/Persistence/Migrations` |
| Shared (Outbox) | OutboxDbContext | `src/Shared/Shared.Infrastructure/Persistence/Migrations/Outbox` |
| Shared (Locks) | DistributedLocksDbContext | `src/Shared/Shared.Infrastructure/Persistence/Migrations/Locks` |

## Setup

Install EF Core CLI tools (one-time):

```powershell
dotnet tool install --global dotnet-ef
```

## Quick Start

Add migration to all contexts:
```powershell
cd d:\Source\Maintained\monolithic
.\scripts\manage-migrations.ps1 -Action add -MigrationName "InitialCreate"
```

Add migration to specific context:
```powershell
.\scripts\manage-migrations.ps1 -Action add -MigrationName "AddUserFeature" -Context users
.\scripts\manage-migrations.ps1 -Action add -MigrationName "AddUserFeature" -Context bookings
.\scripts\manage-migrations.ps1 -Action add -MigrationName "AddUserFeature" -Context outbox
.\scripts\manage-migrations.ps1 -Action add -MigrationName "AddUserFeature" -Context locks
```

Update all databases:
```powershell
.\scripts\manage-migrations.ps1 -Action update
```

Update specific database:
```powershell
.\scripts\manage-migrations.ps1 -Action update -Context users
```

Remove last migration:
```powershell
.\scripts\manage-migrations.ps1 -Action remove -Context bookings
```

## Automatic Migration Application

Migrations are automatically applied at application startup via `MigrationExtensions.cs`. When you run:

```powershell
dotnet run --project src/AppHost
```

All DbContext migrations are applied in order:
1. OutboxDbContext
2. DistributedLocksDbContext
3. UsersDbContext
4. BookingsDbContext

No manual database updates needed in development.

## Manual EF Core Commands

Direct EF Core commands (alternative to script):

**Users Module:**
```powershell
dotnet ef migrations add InitialCreate `
  --project src/Modules/Users/Users.Infrastructure `
  --startup-project src/AppHost `
  --context UsersDbContext `
  --output-dir Persistence/Migrations
```

**Bookings Module:**
```powershell
dotnet ef migrations add InitialCreate `
  --project src/Modules/Bookings/Bookings.Infrastructure `
  --startup-project src/AppHost `
  --context BookingsDbContext `
  --output-dir Persistence/Migrations
```

**Outbox:**
```powershell
dotnet ef migrations add InitialCreate `
  --project src/Shared/Shared.Infrastructure `
  --startup-project src/AppHost `
  --context OutboxDbContext `
  --output-dir Persistence/Migrations/Outbox
```

**Distributed Locks:**
```powershell
dotnet ef migrations add InitialCreate `
  --project src/Shared/Shared.Infrastructure `
  --startup-project src/AppHost `
  --context DistributedLocksDbContext `
  --output-dir Persistence/Migrations/Locks
```

## Best Practices

- Name migrations descriptively: `AddUserPhoneNumber` not `Update`
- Commit ALL migration files (`.cs` and `ModelSnapshot.cs`)
- Review generated migrations before committing
- Keep migrations small and focused
- Test locally before committing: `dotnet run`
- Don't edit migrations after creation, create a new one to fix issues

## Disabling Auto-Migration

To disable automatic migrations (e.g., for production), update `src/AppHost/Program.cs`:

```csharp
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
}
```

## Files

- Migration Extension: `src/AppHost/Infrastructure/Persistence/MigrationExtensions.cs`
- Startup Configuration: `src/AppHost/Program.cs`  
- Management Script: `scripts/manage-migrations.ps1`
