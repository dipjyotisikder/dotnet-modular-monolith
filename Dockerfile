# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY ["Monolithic.slnx", "."]
COPY ["Directory.Packages.props", "."]
COPY ["nuget.config", "."]
COPY ["src/", "src/"]

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet build -c Release --no-restore

# Publish stage
FROM build AS publish

RUN dotnet publish ./src/AppHost/AppHost.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

# Install necessary runtime dependencies
RUN apk add --no-cache libpq

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD dotnet-healthchecks || exit 1

# Run application
ENTRYPOINT ["dotnet", "AppHost.dll"]
