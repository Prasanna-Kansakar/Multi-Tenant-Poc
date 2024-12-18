# Multi-Tenant Proof of Concept (PoC)

## Overview
The **Multi-Tenant PoC** is a .NET 8 application designed to showcase how to implement a multi-tenant architecture with support for multiple databases. This project provides a foundational structure for creating scalable, tenant-aware applications while maintaining data isolation and configurability.

## Features
- Multi-database segregation for tenants.
- Console application for automated database migrations across all tenant databases.
- Centralized tenant configuration and resolution.
- Environment-specific settings for flexible deployment.
- Extensible architecture for tenant onboarding and management.

## Project Structure

### Core Application
- **Database Context**:
  - `AppDbContext`: Defines the application's database context.
  - `AppDbContextFactory`: Dynamically creates instances of the database context based on tenant-specific settings.
- **Tenant Management**:
  - `TenantConfig/Tenant.cs`: Represents tenant-specific configurations.
  - `TenantConfig/TenantResolver.cs`: Resolves tenant data dynamically based on the context, such as headers.
- **Models**: Example `User.cs` for tenant-specific data.
- **Migrations**: Contains migration scripts for database schema updates.

### Migrator Console Application
The **Migrator** is a console application designed to apply migrations to all tenant databases. It ensures that each tenant's database schema is up-to-date without manual intervention.

Key files include:
- `MigratorService.cs`: Core service handling the migration process, supporting commands like `migrate` and `rollback`.
- `Program.cs`: Entry point for the console application.

#### Commands
1. **Apply Migrations**: 
   ```bash
   dotnet run --project Migrator-Test migrate
   ```
   Applies the latest migrations to all tenant databases.

2. **Rollback Migrations**: 
   ```bash
   dotnet run --project Migrator-Test rollback <migrationId>
   ```
   Rolls back migrations for all tenants to the specified migration ID. If no ID is provided, rolls back to the previous migration.

#### Key Features of `MigratorService.cs`
- Automatically retrieves tenant information from the configuration.
- Applies migrations to each tenant's database using dynamic tenant resolution.
- Provides error handling for individual tenants to ensure that one failure does not stop the entire migration process.
- Supports rolling back to a specific migration or the previous migration.

## Prerequisites
- **.NET 8 SDK**
- **PostgreSQL** (or a compatible database system)
- **Visual Studio** or another IDE supporting .NET 8 development

## Getting Started

### Build the Solution
Open the solution file in Visual Studio and build the solution:
```bash
dotnet build
```

### Run the Core Application
1. Configure the connection string in `appsettings.json`.
2. Start the application:
   ```bash
   dotnet run --project Multi-Tenant-Poc
   ```

### Run the Migrator Application
1. Ensure tenant-specific connection strings are correctly configured.
2. Apply migrations to all tenant databases:
   ```bash
   dotnet run --project Migrator-Test migrate
   ```
3. Roll back migrations for all tenants:
   ```bash
   dotnet run --project Migrator-Test rollback <migrationId>
   ```

## Configuration

### Tenant Configuration
Tenant details are managed using the `TenantConfig/Tenant.cs` class and dynamically resolved using `TenantResolver.cs`. Update or extend these classes to onboard new tenants or modify tenant behavior.

- **Header Configuration**: Tenant IDs are retrieved from the `X-Tenant-ID` header. This can be customized as needed.
- **Fallback Behavior**: If no tenant is resolved, the application falls back to the default connection string specified in `appsettings.json`.

#### Switching Tenants via HTTP Context
Tenants are switched dynamically by extracting the tenant ID from the HTTP headers (`X-Tenant-ID`). The `TenantResolver` class handles this process:

```csharp
public Tenant GetCurrentTenant()
{
    var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();

    if (!string.IsNullOrEmpty(tenantId))
    {
        var tenantSection = _configuration.GetSection($"Tenants:{tenantId}");
        if (tenantSection.Exists())
        {
            return new Tenant
            {
                ConnectionString = tenantSection["ConnectionString"]!,
                Provider = tenantSection["Provider"] ?? "PostgreSQL"
            };
        }
    }

    var defaultConnectionString = _configuration.GetConnectionString("Default");
    return new Tenant
    {
        ConnectionString = defaultConnectionString!,
        Provider = "PostgreSQL"
    };
}
```

### Connection Strings
Update the `ConnectionStrings` section in `appsettings.json` to include the necessary connection strings for each tenant. Example:
```json
"ConnectionStrings": {
  "Default": "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=testdb1;Pooling=true;MinPoolSize=1;MaxPoolSize=100;Timeout=30"
},
"Tenants": {
  "Tenant1": {
    "ConnectionString": "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=testdb2;Pooling=true;MinPoolSize=1;MaxPoolSize=100;Timeout=30",
    "Provider": "PostgreSQL"
  }
}
```

### DbContext Configuration
The `AppDbContextFactory` creates database contexts dynamically based on the tenant configuration. It supports multiple providers (e.g., PostgreSQL, InMemory):
```csharp
switch (tenantInfo.Provider.ToLower())
{
    case "postgresql":
        optionsBuilder.UseNpgsql(tenantInfo.ConnectionString);
        break;
    case "memory":
        optionsBuilder.UseInMemoryDatabase(tenantInfo.ConnectionString);
        break;
    default:
        throw new NotSupportedException($"The provider {tenantInfo.Provider} is not supported.");
}
```

### Using DbContext with Dependency Injection
A single `DbContext` instance is sufficient for handling multiple tenants. The `TenantResolver` ensures that the correct connection is used based on the current HTTP context.

Example:
```csharp
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public WeatherForecastController(AppDbContext dbContext)
        => _dbContext = dbContext;

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
        => _dbContext.Forecasts.OrderBy(f => f.Date).ToArray();
}
```

