# Docker and Database Setup Instructions

## Prerequisites

Before running the Aspire AppHost with SQL Server, you need Docker Desktop running.

### Start Docker Desktop

**Option 1: Via Start Menu**
1. Press Windows key
2. Type "Docker Desktop"
3. Click to launch Docker Desktop
4. Wait for Docker to start (whale icon in system tray will be steady)

**Option 2: Via Command Line**
```powershell
# Start Docker Desktop service
Start-Service com.docker.service

# Or launch the Docker Desktop application
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
```

**Verify Docker is Running:**
```powershell
docker ps
# Should show an empty list or running containers (not an error)
```

## Connection String Configuration

### Aspire-Injected Connection Strings

When running via Aspire AppHost, these environment variables are automatically injected:

1. **SQL Server**: `ConnectionStrings__receiptdb`
   - Format: `Server=localhost,PORT;User ID=sa;Password=GENERATED;Database=receiptdb;TrustServerCertificate=true`
   - Generated dynamically by Aspire
   - Visible in Aspire Dashboard

2. **SQLite**: `ConnectionStrings__sqlitedb`
   - Format: `Data Source=receipts.db`
   - Points to local file

### Viewing Connection Strings

1. Run the AppHost:
   ```powershell
   cd AppHost
   dotnet run
   ```

2. Open Aspire Dashboard (URL shown in console)

3. Click on "receiptdb" resource to see:
   - Full connection string
   - Server address and port
   - SA password
   - Database name

4. Click on "sqlitedb" to see SQLite connection

## Running with SQL Server

### Method 1: Via Aspire AppHost (Recommended)

```powershell
# Ensure Docker Desktop is running
docker ps

# Start AppHost (will pull and start SQL Server container)
cd E:\dev\WarrantyApp\MyAspireSolution\AppHost
dotnet run
```

**First Run:**
- SQL Server image will be downloaded (~700MB)
- Container will be created
- Database "receiptdb" will be created
- Takes 2-5 minutes

**Subsequent Runs:**
- Container starts quickly (persistent)
- Data is preserved
- Takes 10-30 seconds

### Method 2: Standalone with LocalDB

```powershell
cd E:\dev\WarrantyApp\MyAspireSolution\MyApi
$env:DatabaseProvider="SqlServer"
dotnet run
```

Uses LocalDB (no Docker needed)

## Running with SQLite

```powershell
cd E:\dev\WarrantyApp\MyAspireSolution\MyApi
$env:DatabaseProvider="Sqlite"
dotnet run
```

No Docker or SQL Server needed. Creates `receipts.db` file in project directory.

## Verifying the Setup

### Check SQL Server Container

```powershell
# List running containers
docker ps

# Should show something like:
# CONTAINER ID   IMAGE                                        PORTS
# abc123...      mcr.microsoft.com/mssql/server:2022-latest   0.0.0.0:xxxxx->1433/tcp
```

### Check Connection in Aspire Dashboard

1. Navigate to dashboard URL (shown in console)
2. Go to "Resources" tab
3. Verify:
   - ✅ **sqlserver** - Running
   - ✅ **receiptdb** - Healthy
   - ✅ **sqlitedb** - Connection available
   - ✅ **myapi** - Running

### Test API Connection

1. Open Swagger UI (URL shown in Aspire Dashboard for myapi)
2. Check logs for database connection message:
   ```
   Using database provider: SqlServer
   Connection string: Server=localhost,xxxxx;...
   ```

## Troubleshooting

### Issue: Docker command not found

**Solution:**
- Docker Desktop is not installed
- Download from: https://www.docker.com/products/docker-desktop/
- Restart after installation

### Issue: Docker Desktop won't start

**Solutions:**
1. Check Windows features:
   - Hyper-V enabled (Windows Pro/Enterprise)
   - WSL 2 installed (All Windows 10/11)
   
2. Update Docker Desktop to latest version

3. Check system requirements:
   - Windows 10 64-bit: Pro, Enterprise, or Education (Build 19041 or higher)
   - OR Windows 11 64-bit

### Issue: SQL Server container fails to start

**Check Logs:**
```powershell
docker logs <container-id>
```

**Common Solutions:**
1. Port 1433 already in use:
   ```powershell
   # Check what's using port
   netstat -ano | findstr :1433
   
   # Kill process if needed
   taskkill /PID <process-id> /F
   ```

2. Insufficient memory:
   - Docker Desktop Settings → Resources
   - Increase memory to at least 2GB

3. Disk space:
   - SQL Server image needs ~2GB
   - Check available space

### Issue: "Connection string not found"

**Check Environment:**
```powershell
# View all connection strings available
$env:ConnectionStrings__receiptdb
$env:ConnectionStrings__sqlitedb
```

**Solution:**
- Must run via Aspire AppHost for auto-injection
- OR manually set connection string

### Issue: SQLite database file not found

**Solutions:**
1. Check current directory:
   ```powershell
   Get-Location
   # Should be in MyApi folder
   ```

2. Manually create file:
   ```powershell
   cd MyApi
   dotnet ef database update
   ```

3. Check file permissions

## Database Migrations

### Apply Migrations (SQL Server via Aspire)

```powershell
# Terminal 1: Start Aspire (keeps SQL Server running)
cd AppHost
dotnet run

# Terminal 2: Apply migrations
cd MyApi
dotnet ef database update
```

### Apply Migrations (SQLite)

```powershell
cd MyApi
$env:DatabaseProvider="Sqlite"
dotnet ef database update
```

## Switching Database Providers

### Option 1: Environment Variable

```powershell
# Use SQL Server
$env:DatabaseProvider="SqlServer"
dotnet run

# Use SQLite
$env:DatabaseProvider="Sqlite"
dotnet run
```

### Option 2: appsettings.json

Edit `MyApi/appsettings.json`:
```json
{
  "DatabaseProvider": "SqlServer"  // or "Sqlite"
}
```

### Option 3: Launch Settings

Edit `MyApi/Properties/launchSettings.json` and add environment variable.

## Inspecting the Database

### SQL Server (via Docker)

**Using sqlcmd:**
```powershell
# Get connection details from Aspire Dashboard
sqlcmd -S localhost,PORT -U sa -P PASSWORD -d receiptdb
```

**Using Azure Data Studio:**
1. Download: https://aka.ms/azuredatastudio
2. Connect with details from Aspire Dashboard
3. Explore tables and data

### SQLite

**Using DB Browser for SQLite:**
1. Download: https://sqlitebrowser.org/
2. Open `receipts.db` file
3. Browse tables

**Using VS Code Extension:**
1. Install "SQLite" extension
2. Right-click `receipts.db` → Open Database

## Connection String Examples

### SQL Server (Aspire-injected)
```
Server=localhost,52341;User ID=sa;Password=myStr0ngP@ss!;Database=receiptdb;TrustServerCertificate=true
```

### SQL Server (LocalDB)
```
Server=(localdb)\mssqllocaldb;Database=ReceiptAppDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

### SQLite
```
Data Source=receipts.db
```
Or absolute path:
```
Data Source=E:\dev\WarrantyApp\MyAspireSolution\MyApi\receipts.db
```

## Next Steps

1. ✅ Start Docker Desktop
2. ✅ Run AppHost to create SQL Server container
3. ✅ Verify resources in Aspire Dashboard
4. ✅ Apply database migrations
5. ✅ Test authentication endpoints
6. Switch between SQL Server and SQLite as needed
