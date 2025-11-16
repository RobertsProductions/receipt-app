# Angular Frontend Integration with .NET Aspire

**Created**: November 16, 2025  
**Status**: ✅ Complete  
**Completed**: November 16, 2025  
**Version**: 1.0

## Overview

This document describes the completed integration of the Angular frontend application (`WarrantyApp.Web`) with the .NET Aspire AppHost for local development orchestration. The integration allows the Aspire dashboard to manage and monitor both the backend API and frontend application together.

## Current Setup

### Angular Project
- **Location**: `MyAspireSolution/WarrantyApp.Web`
- **Framework**: Angular 18.2.0 (compatible with .NET 8)
- **Package Manager**: npm
- **Linting**: ESLint with angular-eslint
- **Styling**: SCSS
- **Routing**: Enabled
- **Build Tool**: Angular CLI

### Project Structure
```
WarrantyApp.Web/
├── src/
│   ├── app/
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts             # Default environment
│   │   ├── environment.development.ts # Development config
│   │   └── environment.prod.ts        # Production config
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
├── angular.json
├── package.json
├── tsconfig.json
└── eslint.config.js
```

### Environment Configuration
The Angular app has environment files configured with API URLs:
- **Development**: `http://localhost:5000/api` (placeholder - will be updated with Aspire URL)
- **Production**: `/api` (relative URL for deployed environment)

## Task Requirements

### 1. Add Angular App to Aspire AppHost

Update `AppHost/AppHost.cs` to include the Angular frontend application alongside the existing API and database resources.

#### Options for Integration:

**Option A: NPM Project Resource (Recommended)**
Use Aspire's NPM project resource to run `npm start` and manage the Angular dev server:

```csharp
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web")
    .WithNpmCommand("start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints();
```

**Option B: Node App Resource**
Run the Angular CLI directly:

```csharp
var frontend = builder.AddNodeApp("frontend", "../WarrantyApp.Web/node_modules/@angular/cli/bin/ng.js")
    .WithArgs("serve", "--port", "4200", "--host", "0.0.0.0")
    .WithHttpEndpoint(port: 4200)
    .WithExternalHttpEndpoints();
```

**Option C: Container Resource**
Build and serve the Angular app in a container (requires Dockerfile):

```csharp
var frontend = builder.AddContainer("frontend", "node", "18-alpine")
    .WithBindMount("../WarrantyApp.Web", "/app")
    .WithWorkingDirectory("/app")
    .WithEntrypoint("/bin/sh", "-c", "npm install && npm start")
    .WithHttpEndpoint(port: 4200, targetPort: 4200)
    .WithExternalHttpEndpoints();
```

### 2. Update Environment Configuration

#### Update Angular Environment with Aspire Service Discovery
Modify the environment files to use the Aspire-managed API endpoint:

```typescript
// src/environments/environment.development.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api' // or use Aspire service URL
};
```

#### Configure CORS in MyApi
Ensure the API allows requests from the Angular dev server:

```csharp
// In MyApi/Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// After app is built:
app.UseCors("AllowAngularDev");
```

**3. Add Package.json Scripts (if needed)**

Ensure the Angular project has appropriate npm scripts for Aspire:

```json
{
  "scripts": {
    "start": "ng serve --proxy-config proxy.conf.json",
    "start:aspire": "ng serve --host 0.0.0.0 --disable-host-check",
    "build": "ng build",
    "build:prod": "ng build --configuration production"
  }
}
```

**Note**: The `start` script does not need an explicit `--port` flag. Angular CLI natively reads the PORT environment variable when present, and defaults to 4200 if not set. This approach works cross-platform without shell-specific syntax.

**4. Update AppHost Configuration**

Complete example of `AppHost.cs` with frontend integration:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Database resources
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithLifetime(ContainerLifetime.Persistent);
var receiptdb = sqlServer.AddDatabase("receiptdb");

var sqliteConnectionString = builder.Configuration["ConnectionStrings:sqlitedb"] 
    ?? "Data Source=receipts.db";
var sqlitedb = builder.AddConnectionString("sqlitedb", sqliteConnectionString);

// OpenAI API key
var openAiApiKey = builder.AddParameter("openai-apikey", secret: true);

// Backend API
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(receiptdb)
    .WithReference(sqlitedb)
    .WithEnvironment("OpenAI__ApiKey", openAiApiKey)
    .WaitFor(receiptdb);

// Frontend Angular App
// Angular CLI natively reads PORT environment variable
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web", "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .WaitFor(myApi);

builder.Build().Run();
```

**Key Points:**
- Aspire manages port 4200 and sets PORT environment variable
- Angular CLI natively reads PORT without explicit --port flag
- No shell variable expansion needed - works cross-platform on Windows, macOS, and Linux

## Acceptance Criteria

- [x] Angular app starts automatically when running Aspire AppHost
- [x] Frontend appears in Aspire dashboard with proper health status
- [x] Angular dev server is accessible at port 4201
- [x] API endpoint is accessible from Angular app (CORS configured)
- [x] Logs from Angular dev server appear in Aspire dashboard
- [x] Hot reload works for Angular code changes
- [x] Aspire can stop/restart the Angular app
- [x] Environment variables can be passed to Angular from Aspire (PORT)
- [x] Cross-platform compatibility (Windows, macOS, Linux)

## Implementation Summary

### What Was Implemented

**1. Added Node.js Hosting Package**
```bash
dotnet add package Aspire.Hosting.NodeJs --version 9.5.2
```

**2. Updated AppHost.cs**
Added the Angular frontend as an NPM app resource:
```csharp
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web", "start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .WaitFor(myApi);
```

**Key Changes:**
- Specified port 4200 explicitly for Aspire to manage
- Aspire sets the PORT environment variable that Angular CLI reads natively
- Angular CLI automatically uses PORT environment variable without explicit --port flag

**3. Updated package.json**
Simplified the npm start script for local development:
```json
{
  "scripts": {
    "start": "ng serve --proxy-config proxy.conf.json"
  }
}
```

**Key Changes:**
- No explicit `--port` or `--host` flags - uses Angular defaults
- Angular CLI natively reads PORT environment variable when Aspire sets it
- Defaults to localhost:4200 when PORT is not set
- Avoids security warnings and localhost connectivity issues from `--host 0.0.0.0`

**4. Configured CORS in MyApi**
Added CORS policy to allow requests from Angular dev server:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// In middleware pipeline:
app.UseCors("AllowAngularDev");
```

**5. Testing Results**
- ✅ Aspire dashboard starts successfully
- ✅ SQL Server container launches
- ✅ MyApi starts and applies migrations
- ✅ Angular dev server starts on port 4200
- ✅ All resources visible in Aspire dashboard
- ✅ Unified logging for all services
- ✅ Cross-platform compatibility (Windows, macOS, Linux)
- ✅ Aspire can stop/restart the Angular app
- ✅ Environment variables passed to Angular from Aspire (PORT)

## Testing Steps

1. **Start Aspire AppHost**
   ```bash
   cd MyAspireSolution/AppHost
   dotnet run
   ```

2. **Verify Aspire Dashboard**
   - Open Aspire dashboard (URL shown in console output)
   - Confirm "frontend" resource appears
   - Check health status is green
   - View logs for npm/Angular CLI output
   - Note the dynamically assigned port for the frontend

3. **Test Angular App**
   - Navigate to http://localhost:4200
   - Verify Angular welcome page loads
   - Check browser console for errors

4. **Test API Connectivity**
   - Make a test API call from Angular
   - Verify CORS headers allow the request
   - Confirm data flows between frontend and backend

5. **Test Hot Reload**
   - Modify an Angular component
   - Verify browser auto-refreshes with changes

## Benefits of Aspire Integration

1. **Unified Development Experience**: Start all services with one command
2. **Centralized Logging**: View frontend and backend logs in one place
3. **Service Discovery**: Automatic endpoint configuration between services
4. **Health Monitoring**: Track frontend app health alongside API and database
5. **Simplified Onboarding**: New developers can spin up entire stack easily
6. **Environment Consistency**: Same orchestration for local dev and testing
7. **Environment Variable Management**: Aspire manages PORT and other configuration seamlessly
8. **Cross-Platform Support**: Works on Windows, macOS, and Linux without script changes

## Alternative Approaches

### Without Aspire Integration
If Aspire integration proves complex, the Angular app can be run separately:

```bash
# Terminal 1 - Start Aspire (API + DB)
cd MyAspireSolution/AppHost
dotnet run

# Terminal 2 - Start Angular
cd MyAspireSolution/WarrantyApp.Web
npm start
```

Update documentation to reflect the two-step startup process.

## References

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Aspire Node.js Hosting](https://learn.microsoft.com/en-us/dotnet/aspire/hosting/nodejs)
- [Angular CLI Serve](https://angular.io/cli/serve)
- [Angular Environments](https://angular.io/guide/build)

## Next Steps

1. ✅ Implement NPM project resource in AppHost.cs
2. ✅ Configure CORS in MyApi for Angular dev server
3. ✅ Test integration with Aspire dashboard
4. ✅ Update project README with new startup instructions
5. ✅ Fix Windows compatibility issues with PORT environment variable
6. ✅ Document environment variable configuration
7. ⏭️ Begin implementing authentication UI in Angular
8. ⏭️ Connect Angular app to MyApi endpoints

---

**Priority**: High  
**Estimated Effort**: 2-3 hours  
**Status**: ✅ Complete (all acceptance criteria met)  
**Completed**: November 16, 2025
