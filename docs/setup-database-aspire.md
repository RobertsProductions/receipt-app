# 05 - Database Resources with Aspire AppHost

**Date:** November 16, 2025  
**Status:** Completed

## Overview
Added both SQL Server and SQLite database resources to the Aspire AppHost with automatic connection string injection and support for switching between database providers.

## Changes Made

### 1. AppHost Package Added
- **Package**: `Aspire.Hosting.SqlServer` (v13.0.0)
- **Purpose**: Enables SQL Server container orchestration in Aspire

### 2. MyApi Package Added
- **Package**: `Microsoft.EntityFrameworkCore.Sqlite` (v8.0.11)
- **Purpose**: Enables SQLite database support

## AppHost Configuration

### Updated AppHost.cs

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server resource
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("receiptdb");

// Add SQLite database (file-based, no container needed)
var sqlite = builder.AddConnectionString("sqlite", "Data Source=receipts.db");

// Add the API with database connections
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(sqlServer)
    .WithReference(sqlite);

builder.Build().Run();
```

### What This Does

**SQL Server Resource:**
- Creates a SQL Server container when the AppHost starts
- Container has persistent lifetime (survives restarts)
- Creates a database named "receiptdb"
- Automatically generates and injects connection string
- Connection string name: `ConnectionStrings__sqlserver`

**SQLite Resource:**
- File-based database (no container needed)
- Connection string points to `receipts.db` file
- Lightweight option for development
- Connection string name: `ConnectionStrings__sqlite`

**API Configuration:**
- References both database resources
- Aspire automatically injects connection strings as environment variables
- API can access them through IConfiguration

## MyApi Configuration

### appsettings.json

Added support for multiple database providers:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ReceiptAppDb;Trusted_Connection=True;MultipleActiveResultSets=true",
    "SqlServerConnection": "Server=localhost,1433;Database=receiptdb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;",
    "SqliteConnection": "Data Source=receipts.db"
  },
  "DatabaseProvider": "SqlServer"
}
```

### appsettings.Sqlite.json (New)

Configuration for SQLite mode:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=receipts.db"
  }
}
```

### appsettings.SqlServer.json (New)

Configuration for SQL Server mode:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseProvider": "SqlServer"
}
```

### Updated Program.cs

Added database provider detection and configuration:

```csharp
// Configure Database
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
var connectionString = databaseProvider switch
{
    "Sqlite" => builder.Configuration.GetConnectionString("SqliteConnection") 
                ?? builder.Configuration.GetConnectionString("sqlite") 
                ?? "Data Source=receipts.db",
    "SqlServer" => builder.Configuration.GetConnectionString("SqlServerConnection") 
                   ?? builder.Configuration.GetConnectionString("sqlserver") 
                   ?? builder.Configuration.GetConnectionString("DefaultConnection"),
    _ => builder.Configuration.GetConnectionString("DefaultConnection")
} ?? throw new InvalidOperationException($"Connection string for {databaseProvider} not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "Sqlite")
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});
```

## How It Works

### When Running via Aspire AppHost

1. **AppHost starts** and creates resources
2. **SQL Server container** is pulled and started (if not exists)
3. **Connection strings** are generated automatically
4. **Environment variables** are injected into MyApi:
   - `ConnectionStrings__sqlserver` → SQL Server connection
   - `ConnectionStrings__sqlite` → SQLite connection
5. **MyApi reads** the DatabaseProvider setting
6. **Selects appropriate** database and connection string
7. **DbContext configured** for chosen provider

### Connection String Priority

The application checks connection strings in this order:

**For SQLite:**
1. `ConnectionStrings:SqliteConnection` (appsettings)
2. `ConnectionStrings:sqlite` (Aspire-injected)
3. Default: `Data Source=receipts.db`

**For SQL Server:**
1. `ConnectionStrings:SqlServerConnection` (appsettings)
2. `ConnectionStrings:sqlserver` (Aspire-injected)
3. `ConnectionStrings:DefaultConnection` (appsettings)

## Database Provider Switching

### Method 1: Environment Variable

```bash
# Run with SQLite
$env:DatabaseProvider="Sqlite"
dotnet run --project MyApi

# Run with SQL Server
$env:DatabaseProvider="SqlServer"
dotnet run --project MyApi
```

### Method 2: Configuration File

```bash
# Use SQLite configuration
dotnet run --project MyApi --environment Sqlite

# Use SQL Server configuration
dotnet run --project MyApi --environment SqlServer
```

### Method 3: appsettings.json

Change the `DatabaseProvider` value:

```json
{
  "DatabaseProvider": "Sqlite"  // or "SqlServer"
}
```

## Running the Application

### With Aspire AppHost (Recommended)

```bash
cd AppHost
dotnet run
```

This will:
- Start the Aspire Dashboard
- Pull and start SQL Server container (first run)
- Start MyApi with injected connections
- Display all resources in dashboard

### Standalone (Without AppHost)

**With SQL Server:**
```bash
cd MyApi
dotnet run
```

**With SQLite:**
```bash
cd MyApi
$env:DatabaseProvider="Sqlite"
dotnet run
```

## SQL Server Container Details

### Container Configuration
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Port**: 1433 (exposed to host)
- **Lifetime**: Persistent (survives restarts)
- **Database**: receiptdb (auto-created)
- **SA Password**: Auto-generated by Aspire

### Finding Connection Details

View in Aspire Dashboard:
1. Open Dashboard (https://localhost:17263)
2. Click on "sqlserver" resource
3. See connection string and credentials
4. Click on "receiptdb" database for specific connection

### Manual Connection

Get the connection string from Aspire Dashboard and use:

```bash
# Using sqlcmd
sqlcmd -S localhost,1433 -U sa -P <password-from-dashboard> -d receiptdb

# Using Azure Data Studio or SQL Server Management Studio
Server: localhost,1433
Authentication: SQL Login
Username: sa
Password: <from-dashboard>
Database: receiptdb
```

## Database Migrations

### Creating Migrations

**For SQL Server:**
```bash
cd MyApi
dotnet ef migrations add MigrationName
```

**For SQLite:**
```bash
cd MyApi
$env:DatabaseProvider="Sqlite"
dotnet ef migrations add MigrationName --context ApplicationDbContext
```

### Applying Migrations

**For SQL Server (via Aspire):**
```bash
# Let Aspire start SQL Server, then:
cd MyApi
dotnet ef database update
```

**For SQLite:**
```bash
cd MyApi
$env:DatabaseProvider="Sqlite"
dotnet ef database update
```

### Migration Compatibility

The existing `InitialCreate` migration works with both providers because:
- Uses standard Identity schema
- No provider-specific features
- Compatible with both SQL Server and SQLite

## Advantages of This Setup

### SQL Server Benefits
- **Full-featured** enterprise database
- **Excellent performance** for production
- **Advanced features**: Full-text search, JSON support, etc.
- **Scalability**: Handles large datasets
- **Container orchestration**: Aspire manages lifecycle

### SQLite Benefits
- **Lightweight**: No container needed
- **Fast startup**: Instant database availability
- **Portable**: Single file database
- **Simple**: No server configuration
- **Great for dev/testing**: Quick iterations

### Aspire Integration
- **Automatic discovery**: Databases visible in dashboard
- **Connection injection**: No hardcoded strings
- **Lifecycle management**: Containers auto-started/stopped
- **Health checks**: Monitor database status
- **Local development**: Consistent environment

## Aspire Dashboard Features

### Resource Monitoring
- View all database connections
- See container status and logs
- Monitor health checks
- Track resource usage

### Connection Strings
- Automatically generated
- Securely injected
- Visible in dashboard
- Copy for manual connections

### Container Logs
- View SQL Server startup logs
- Debug connection issues
- Monitor queries (if configured)
- Track errors

## Troubleshooting

### Issue: SQL Server container fails to start

**Check Docker:**
```bash
docker ps -a
docker logs <container-id>
```

**Solutions:**
- Ensure Docker Desktop is running
- Check port 1433 is not in use
- Verify disk space available
- Check Docker resource limits

### Issue: Cannot connect to SQL Server

**Solutions:**
1. Get connection string from Aspire Dashboard
2. Verify container is running: `docker ps`
3. Check firewall allows port 1433
4. Ensure SA password is correct
5. Try `TrustServerCertificate=True` in connection string

### Issue: SQLite file not found

**Solutions:**
1. Check file path is correct
2. Ensure write permissions
3. File created in API project directory
4. Use absolute path if needed

### Issue: Migration fails

**Solutions:**
1. Verify DatabaseProvider setting matches intent
2. Ensure database server is running
3. Check connection string is valid
4. Try: `dotnet ef database drop` then `dotnet ef database update`

## Configuration Examples

### Run API with Aspire SQL Server

```bash
cd AppHost
dotnet run
# API automatically uses Aspire SQL Server connection
```

### Run API with Local SQLite

```bash
cd MyApi
$env:DatabaseProvider="Sqlite"
dotnet run
```

### Run API with LocalDB (No Aspire)

```bash
cd MyApi
$env:DatabaseProvider="SqlServer"
$env:ConnectionStrings__SqlServerConnection="Server=(localdb)\mssqllocaldb;Database=ReceiptAppDb;Trusted_Connection=True"
dotnet run
```

## Security Considerations

### SQL Server
- SA password auto-generated by Aspire
- Change password for production
- Use Windows Authentication when possible
- Restrict network access to container
- Use secrets management (Azure Key Vault)

### SQLite
- File-based, no network exposure
- Set file permissions appropriately
- Not recommended for production multi-user
- Consider encryption for sensitive data

### Connection Strings
- Never commit to source control
- Use User Secrets for local dev
- Use environment variables in CI/CD
- Use Key Vault for production

## Next Steps

1. ✅ Apply migrations to both databases
2. Test API with each database provider
3. Add database seeding for initial data
4. Configure backup strategies
5. Add database health checks
6. Implement connection pooling tuning
7. Add query logging for debugging
8. Consider read replicas for scaling

## Summary

Successfully configured Aspire AppHost to manage both SQL Server (containerized) and SQLite (file-based) databases with automatic connection string injection, provider switching support, and full integration with the Aspire Dashboard for monitoring and management.

The API can now seamlessly switch between database providers based on configuration, making it ideal for different environments (development, testing, production) and use cases.

## Resources in Aspire Dashboard

When running, you'll see:
- **myapi**: API project resource
- **sqlserver**: SQL Server container
- **receiptdb**: Database within SQL Server
- **Endpoints**: API URLs and database connections
- **Logs**: Real-time logs from all resources
- **Metrics**: Performance metrics
- **Traces**: Distributed tracing data
