# Angular Frontend Integration with .NET Aspire

**Created**: November 16, 2025  
**Status**: Pending  
**Version**: 1.0

## Overview

This document outlines the task to integrate the Angular frontend application (`WarrantyApp.Web`) with the .NET Aspire AppHost for local development orchestration. The integration will allow the Aspire dashboard to manage and monitor both the backend API and frontend application together.

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

### 3. Add Package.json Scripts (if needed)

Ensure the Angular project has appropriate npm scripts for Aspire:

```json
{
  "scripts": {
    "start": "ng serve --host 0.0.0.0 --port 4200",
    "start:aspire": "ng serve --host 0.0.0.0 --disable-host-check",
    "build": "ng build",
    "build:prod": "ng build --configuration production"
  }
}
```

### 4. Update AppHost Configuration

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
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web")
    .WithNpmCommand("start")
    .WithHttpEndpoint(port: 4200, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(myApi); // Optional: creates service binding

builder.Build().Run();
```

## Acceptance Criteria

- [ ] Angular app starts automatically when running Aspire AppHost
- [ ] Frontend appears in Aspire dashboard with proper health status
- [ ] Angular dev server is accessible at `http://localhost:4200`
- [ ] API endpoint is accessible from Angular app (CORS configured)
- [ ] Logs from Angular dev server appear in Aspire dashboard
- [ ] Hot reload works for Angular code changes
- [ ] Aspire can stop/restart the Angular app
- [ ] Environment variables can be passed to Angular from Aspire

## Testing Steps

1. **Start Aspire AppHost**
   ```bash
   cd MyAspireSolution/AppHost
   dotnet run
   ```

2. **Verify Aspire Dashboard**
   - Open Aspire dashboard (usually `http://localhost:15000`)
   - Confirm "frontend" resource appears
   - Check health status is green
   - View logs for npm/Angular CLI output

3. **Test Angular App**
   - Navigate to `http://localhost:4200`
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

1. Implement Option A (NPM project resource) in AppHost.cs
2. Configure CORS in MyApi for Angular dev server
3. Test integration with Aspire dashboard
4. Update project README with new startup instructions
5. Document any environment variable configuration needed

---

**Priority**: High  
**Estimated Effort**: 2-3 hours  
**Dependencies**: None (Angular project and Aspire already set up)  
**Assignee**: TBD
