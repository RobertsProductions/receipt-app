# Aspire Angular Frontend Integration Fix

## Date
2025-11-16

## Problem
When running the application through the Aspire AppHost, the Angular frontend was stuck perpetually loading and couldn't reach the backend API. Additionally, the Aspire dashboard showed incorrect URLs that didn't match the actual ports the services were running on.

### Symptoms
1. Angular app link in Aspire dashboard showed one port (e.g., http://localhost:62809/)
2. Angular console output showed a different port (e.g., http://localhost:53301/)
3. API requests from Angular would fail or timeout
4. Only the Angular port returned HTML; API link didn't work

### Root Causes
1. **Port Mismatch**: Angular was using `--port 0` (random port selection), but Aspire was assigning a different port via the PORT environment variable
2. **Missing API Endpoint Exposure**: The API wasn't configured to expose external endpoints in Aspire
3. **Hardcoded Proxy Configuration**: Angular's proxy was hardcoded to `http://localhost:5000` instead of using Aspire's dynamic service URLs
4. **Service Discovery**: Angular app wasn't properly referencing the API for service discovery

## Solution

### 1. AppHost Configuration (`AppHost/AppHost.cs`)
Added external endpoint exposure and service reference for proper orchestration:

```csharp
// API - Added .WithExternalHttpEndpoints() to expose the API endpoint
var myApi = builder.AddProject<Projects.MyApi>("myapi")
    .WithReference(receiptdb)
    .WithReference(sqlitedb)
    .WithEnvironment("OpenAI__ApiKey", openAiApiKey)
    .WithExternalHttpEndpoints()  // NEW: Exposes API endpoint in dashboard
    .WaitFor(receiptdb);

// Frontend - Added service reference and API_URL environment variable
var frontend = builder.AddNpmApp("frontend", "../WarrantyApp.Web", "start")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(myApi)  // NEW: Enables service discovery
    .WithEnvironment("API_URL", myApi.GetEndpoint("http"))  // NEW: Passes dynamic API URL
    .WaitFor(myApi);
```

### 2. Dynamic Proxy Configuration (`WarrantyApp.Web/proxy.conf.mjs`)
Replaced static JSON config with dynamic JavaScript that reads environment variables:

**Before** (`proxy.conf.json`):
```json
{
  "/api": {
    "target": "http://localhost:5000",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

**After** (`proxy.conf.mjs`):
```javascript
export default function() {
  // Aspire injects service URLs in the format: services__<servicename>__http__0
  const apiUrl = process.env.API_URL || 
                 process.env.services__myapi__http__0 || 
                 'http://localhost:5000';
  
  console.log('Proxy configuration:');
  console.log('  API_URL:', process.env.API_URL);
  console.log('  services__myapi__http__0:', process.env.services__myapi__http__0);
  console.log('  Using target:', apiUrl);
  
  return {
    '/api': {
      target: apiUrl,
      secure: false,
      changeOrigin: true,
      logLevel: 'debug'
    }
  };
}
```

**Benefits**:
- Reads `API_URL` from Aspire's environment injection
- Falls back to Aspire's service discovery format (`services__myapi__http__0`)
- Falls back to `localhost:5000` for standalone development
- Logs configuration for debugging

### 3. Port Management (`WarrantyApp.Web/start-server.js`)
Created a Node.js launcher to properly handle Aspire's PORT environment variable:

```javascript
#!/usr/bin/env node

const { spawn } = require('child_process');

const port = process.env.PORT || '4200';

console.log(`Starting Angular dev server on port ${port}`);

const ngServe = spawn('ng', ['serve', '--proxy-config', 'proxy.conf.mjs', '--port', port], {
  stdio: 'inherit',
  shell: true
});

ngServe.on('error', (error) => {
  console.error(`Error starting Angular dev server: ${error.message}`);
  process.exit(1);
});

ngServe.on('close', (code) => {
  process.exit(code);
});
```

**Why this is needed**:
- Angular CLI doesn't natively read the PORT environment variable
- Aspire passes PORT via environment, but `ng serve` needs it as a CLI argument
- This script bridges the gap between Aspire's environment variables and Angular's CLI

### 4. Package.json Update (`WarrantyApp.Web/package.json`)
Updated the start script to use the new launcher:

```json
"scripts": {
  "start": "node start-server.js",
  ...
}
```

## How It Works Now

1. **Aspire Startup**:
   - AppHost allocates ports for both API and Frontend
   - API gets a port (e.g., 54525) and starts
   - Frontend gets a port (e.g., 62809)

2. **Environment Injection**:
   - Aspire sets `PORT=62809` for the frontend
   - Aspire sets `API_URL=http://localhost:54525` for the frontend
   - Aspire also sets `services__myapi__http__0=http://localhost:54525`

3. **Frontend Startup**:
   - `npm start` runs `start-server.js`
   - Script reads PORT from environment
   - Script starts Angular with `ng serve --port 62809`
   - Angular starts on the correct port

4. **Proxy Configuration**:
   - Angular loads `proxy.conf.mjs`
   - Script reads `API_URL` or `services__myapi__http__0`
   - Proxy targets the correct API endpoint
   - API requests from Angular work correctly

5. **Dashboard Links**:
   - Aspire dashboard shows both services with correct URLs
   - Frontend link: http://localhost:62809/ → Works ✓
   - API link: http://localhost:54525/ → Shows Swagger ✓

## Files Changed

### New Files
- `WarrantyApp.Web/proxy.conf.mjs` - Dynamic proxy configuration
- `WarrantyApp.Web/start-server.js` - Port management launcher script
- `ASPIRE_ANGULAR_FIX.md` - This documentation

### Modified Files
- `AppHost/AppHost.cs` - Added external endpoints and service references
- `WarrantyApp.Web/package.json` - Updated start script

### Deprecated Files
- `WarrantyApp.Web/proxy.conf.json` - Replaced by proxy.conf.mjs (can be deleted)

## Testing

### To verify the fix works:
1. Stop any running Aspire instances
2. Run: `dotnet run --project AppHost`
3. Open the Aspire dashboard
4. Verify both "myapi" and "frontend" show in the resources list
5. Click the "frontend" link → Should load Angular app
6. Click the "myapi" link → Should load Swagger UI
7. In Angular app, check browser console for proxy configuration logs
8. Test API calls from Angular (should work without CORS or connection errors)

### Expected Console Output
```
Proxy configuration:
  API_URL: http://localhost:54525
  services__myapi__http__0: http://localhost:54525
  Using target: http://localhost:54525
```

## Benefits

1. **No More Port Conflicts**: Aspire controls all ports, no random selection
2. **Correct Dashboard Links**: URLs in Aspire match actual service ports
3. **Dynamic Service Discovery**: Frontend automatically finds API regardless of port
4. **Better Development Experience**: Click links in Aspire dashboard and they just work
5. **Production Ready**: Configuration works both in Aspire and standalone modes
6. **Debugging Support**: Proxy logs show which API URL is being used

## Compatibility

- ✅ Works with Aspire orchestration
- ✅ Works with standalone `npm start` (falls back to port 4200)
- ✅ Works with standalone API development
- ✅ Cross-platform (Windows, macOS, Linux)
- ✅ No breaking changes to existing development workflows

## Future Improvements

1. Consider using Aspire's built-in service-to-service communication instead of proxy
2. Add health checks for the frontend to verify API connectivity
3. Consider containerizing the Angular app for production deployments
4. Add environment-specific configurations (dev, staging, prod)
