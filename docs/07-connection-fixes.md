# Database Connection Fixes

## Issues Resolved

### 1. SQL Server Connection Timeout
**Problem:** Connection timeout errors when API tried to connect to SQL Server container:
```
Microsoft.Data.SqlClient.SqlException: Connection Timeout Expired.
The timeout period elapsed while attempting to consume the pre-login handshake acknowledgement.
```

**Root Causes:**
- SQL Server container takes time to fully initialize
- API was starting before SQL Server was ready
- Default connection timeout was too short for container startup

**Solutions Applied:**

#### A. AppHost.cs - Added WaitFor Dependency
```csharp
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(receiptdb)
    .WithReference(sqlitedb)
    .WaitFor(receiptdb);  // ✅ Wait for SQL Server database to be ready
```

This ensures:
- Aspire waits for the SQL Server container to be fully ready
- The database is accepting connections before starting the API
- Proper dependency orchestration

#### B. Program.cs - Added Connection Resiliency
```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
    sqlOptions.CommandTimeout(60); // Increased to 60 seconds
});
```

Benefits:
- ✅ Automatic retry on transient failures (up to 5 attempts)
- ✅ Exponential backoff between retries (max 30 seconds)
- ✅ Increased command timeout to 60 seconds
- ✅ Handles container startup delays gracefully

### 2. SQLite Connection Configuration
**Problem:** Hardcoded SQLite connection string wasn't flexible for demo/testing scenarios.

**Solution: User Secrets Integration**

#### AppHost.cs - Read from Configuration
```csharp
var sqliteConnectionString = builder.Configuration["ConnectionStrings:sqlitedb"] 
    ?? "Data Source=receipts.db";  // Fallback to default
var sqlitedb = builder.AddConnectionString("sqlitedb", sqliteConnectionString);
```

#### Set User Secret
```powershell
cd AppHost
dotnet user-secrets set "ConnectionStrings:sqlitedb" "Data Source=../MyApi/receipts.db"
```

Benefits:
- ✅ Connection string can be customized per developer/environment
- ✅ Stored securely in user secrets (not in source control)
- ✅ Sensible default for quick start
- ✅ Flexible for demos and testing

## Verification Steps

### 1. Build Solution
```powershell
cd E:\dev\WarrantyApp\MyAspireSolution
dotnet build
```
✅ Build succeeds with no warnings or errors

### 2. Check User Secrets
```powershell
cd AppHost
dotnet user-secrets list
```
Should show:
```
ConnectionStrings:sqlitedb = Data Source=../MyApi/receipts.db
```

### 3. Run AppHost
```powershell
cd AppHost
dotnet run
```

Expected behavior:
1. SQL Server container starts (if not already running)
2. Aspire waits for SQL Server to be ready (WaitFor)
3. API starts only after database is ready
4. Connection retries handle any remaining transient issues
5. No timeout errors in logs

### 4. Verify in Aspire Dashboard
- Navigate to dashboard URL (shown in console)
- Check Resources tab:
  - ✅ sqlserver: Running, Healthy
  - ✅ receiptdb: Healthy
  - ✅ sqlitedb: Connection available
  - ✅ myapi: Running

## Additional Configuration

### Override SQLite Path
For different environments, update user secret:

```powershell
# Absolute path
dotnet user-secrets set "ConnectionStrings:sqlitedb" "Data Source=C:\Data\receipts.db"

# Shared network location
dotnet user-secrets set "ConnectionStrings:sqlitedb" "Data Source=\\server\share\receipts.db"

# In-memory (testing)
dotnet user-secrets set "ConnectionStrings:sqlitedb" "Data Source=:memory:"
```

### Adjust SQL Server Retry Policy
Edit `MyApi/Program.cs` to customize:

```csharp
sqlOptions.EnableRetryOnFailure(
    maxRetryCount: 10,        // More retries
    maxRetryDelay: TimeSpan.FromSeconds(60),  // Longer delay
    errorNumbersToAdd: null);
```

## Troubleshooting

### SQL Server Still Times Out
1. Check Docker:
   ```powershell
   docker ps -a
   docker logs <container-name>
   ```

2. Verify SQL Server is running:
   ```powershell
   docker exec <container-name> /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P <password> -Q "SELECT @@VERSION" -C
   ```

3. Increase retry count/timeout in Program.cs

### SQLite File Not Found
1. Check current directory when running:
   ```powershell
   Get-Location
   ```

2. Verify path in user secrets:
   ```powershell
   dotnet user-secrets list
   ```

3. Use absolute path if relative path issues persist

## Summary of Changes

| File | Changes | Purpose |
|------|---------|---------|
| AppHost/AppHost.cs | Added `.WaitFor(receiptdb)` | Ensures SQL Server is ready before starting API |
| AppHost/AppHost.cs | Read SQLite from config | Support user secrets for flexible connection strings |
| MyApi/Program.cs | Added retry logic & timeout | Handle transient SQL Server connection failures |
| MyApi/MyApi.csproj | Updated EF Core packages | Latest stable versions for .NET 8 |
| AppHost (user secrets) | Set SQLite connection | Demo-ready configuration |

## Next Steps

- [ ] Test with fresh Docker container (remove old one)
- [ ] Apply database migrations
- [ ] Verify authentication endpoints work
- [ ] Add integration tests for database connections
- [ ] Document connection string patterns for production
