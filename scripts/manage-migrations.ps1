<#
.SYNOPSIS
    Manages EF Core migrations across all DbContexts in the solution
.DESCRIPTION
    Provides unified interface to add, update, or remove migrations for all modules
.EXAMPLE
    .\manage-migrations.ps1 -Action add -MigrationName "InitialCreate"
    .\manage-migrations.ps1 -Action update -Context users
#>

param(
    [ValidateSet("add", "update", "remove")]
    [string]$Action = "add",
    
    [string]$MigrationName = "Update",
    
    [ValidateSet("all", "users", "bookings", "outbox", "locks")]
    [string]$Context = "all"
)

$rootDir = Split-Path -Parent -Path $PSScriptRoot
$startupProject = "src/AppHost"

$migrations = @(
    @{
        Name      = "users"
        Project   = "src/Modules/Users/Users.Infrastructure"
        DbContext = "UsersDbContext"
        OutputDir = "Persistence/Migrations"
    },
    @{
        Name      = "bookings"
        Project   = "src/Modules/Bookings/Bookings.Infrastructure"
        DbContext = "BookingsDbContext"
        OutputDir = "Persistence/Migrations"
    },
    @{
        Name      = "outbox"
        Project   = "src/Shared/Shared.Infrastructure"
        DbContext = "OutboxDbContext"
        OutputDir = "Persistence/Migrations/Outbox"
    },
    @{
        Name      = "locks"
        Project   = "src/Shared/Shared.Infrastructure"
        DbContext = "DistributedLocksDbContext"
        OutputDir = "Persistence/Migrations/Locks"
    }
)

function Invoke-Migration {
    param(
        [hashtable]$Migration,
        [string]$Action,
        [string]$MigrationName
    )
    
    $params = @(
        "ef", "migrations", $Action,
        "--project", $Migration.Project,
        "--startup-project", $startupProject,
        "--context", $Migration.DbContext,
        "--output-dir", $Migration.OutputDir
    )
    
    if ($Action -eq "add") {
        $params += $MigrationName
    }
    
    Write-Host "$($Migration.Name)" -ForegroundColor Cyan
    & dotnet $params
    Write-Host ""
    
    return $LASTEXITCODE -eq 0
}

function Update-Database {
    param([hashtable]$Migration)
    
    $params = @(
        "ef", "database", "update",
        "--project", $Migration.Project,
        "--startup-project", $startupProject,
        "--context", $Migration.DbContext
    )
    
    Write-Host "$($Migration.Name)" -ForegroundColor Cyan
    & dotnet $params
    Write-Host ""
    
    return $LASTEXITCODE -eq 0
}

Set-Location $rootDir

$toProcess = if ($Context -eq "all") { $migrations } else { $migrations | Where-Object { $_.Name -eq $Context } }

if ($toProcess.Count -eq 0 -and $toProcess -ne $null) {
    $toProcess = @($toProcess)
}

foreach ($migration in $toProcess) {
    if ($Action -eq "add" -or $Action -eq "remove") {
        Invoke-Migration $migration $Action $MigrationName
    }
    elseif ($Action -eq "update") {
        Update-Database $migration
    }
}
