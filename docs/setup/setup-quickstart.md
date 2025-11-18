# Quick Start Guide - Warranty App

## Start the Application

### Prerequisites Check
```bash
# Verify Node.js and npm
node --version  # Should be 18+ (22 LTS recommended)
npm --version   # Should be 10+

# Verify .NET SDK
dotnet --version  # Should be 8.0.302+

# Verify Docker is running
docker --version
```

### Step 1: Install Dependencies (First Time Only)

```bash
# From solution root
cd MyAspireSolution

# Install Angular dependencies
cd WarrantyApp.Web
npm install
cd ..

# Restore .NET dependencies
dotnet restore
```

### Step 2: Start Everything with Aspire (Single Command! 🎉)

```bash
# From solution root
cd AppHost
dotnet run
```

**What happens:**
- Aspire dashboard opens at `https://localhost:17263`
- SQL Server container starts
- MyApi starts on a dynamic port (e.g., `http://localhost:54525`)
- **Angular frontend starts on a dynamic port (e.g., `http://localhost:62809`)** ✨
- Database migrations apply automatically
- All logs visible in Aspire dashboard

**Note**: Aspire assigns dynamic ports to avoid conflicts. Check the Aspire dashboard for the actual URLs.

### Step 3: Access the Application

**Option A - Via Aspire Dashboard (Recommended):**
1. Open the Aspire Dashboard at `https://localhost:17263`
2. Click the link for "frontend" resource to access the Angular app
3. Click the link for "myapi" resource to access Swagger UI

**Option B - Direct URLs (if running standalone):**
- **Frontend**: http://localhost:4200 (when not using Aspire)
- **Backend API**: http://localhost:5000 (when not using Aspire)
- **Swagger UI**: http://localhost:5000/swagger

## Alternative: Run Frontend Separately

If you prefer to run the frontend separately for better control:

**Terminal 1 - Backend via Aspire:**
```bash
cd AppHost
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd WarrantyApp.Web
npm start
```

## Development Workflow

### Frontend Development
```bash
cd WarrantyApp.Web

# Start dev server
npm start

# Run linter
npm run lint

# Run tests
npm test

# Build for production
npm run build:prod
```

### Backend Development
```bash
cd MyApi

# Run standalone (without Aspire)
dotnet run

# Run tests
dotnet test ../MyApi.Tests

# Watch mode
dotnet watch run
```

### Full Stack with Aspire
```bash
cd AppHost
dotnet run
# Then in another terminal:
cd WarrantyApp.Web
npm start
```

## Common Tasks

### Add OpenAI API Key (for OCR)
```powershell
# Option 1: PowerShell script
.\SetOpenAiKey.ps1

# Option 2: User secrets
cd MyApi
dotnet user-secrets set "OpenAI:ApiKey" "your-key-here"

# Option 3: Aspire Dashboard
# Start app, go to dashboard, set openai-apikey parameter
```

### Reset Database
```bash
# Stop all containers
docker stop sqlserver

# Remove container
docker rm sqlserver

# Restart Aspire (will recreate)
cd AppHost
dotnet run
```

### Generate Angular Component
```bash
cd WarrantyApp.Web
npx ng generate component components/my-component
```

### Add NuGet Package
```bash
cd MyApi
dotnet add package PackageName
```

### Add NPM Package
```bash
cd WarrantyApp.Web
npm install package-name
```

## Troubleshooting

### Port Already in Use
```bash
# Find process using port
netstat -ano | findstr :5000
netstat -ano | findstr :4200

# Kill process (Windows)
taskkill /PID <PID> /F
```

### Angular Build Errors
```bash
cd WarrantyApp.Web
# Clear cache
rm -rf node_modules package-lock.json
npm install
```

### Database Connection Issues
- Ensure Docker Desktop is running
- Check Aspire dashboard for SQL Server status
- Verify connection string in Aspire dashboard

### CORS Issues
- Ensure both backend and frontend are running
- Check proxy.conf.mjs configuration (dynamic proxy)
- Verify API allows localhost origins (CORS configured for any localhost port)

## Environment Variables

### Backend (MyApi)
Set via user secrets or Aspire dashboard:
- `OpenAI:ApiKey` - OpenAI API key for OCR
- `ConnectionStrings:receiptdb` - SQL Server connection
- `ConnectionStrings:sqlitedb` - SQLite connection
- `Smtp:Host`, `Smtp:Port`, `Smtp:User`, `Smtp:Password` - Email config
- `Twilio:AccountSid`, `Twilio:AuthToken`, `Twilio:PhoneNumber` - SMS config

### Frontend (Angular)
Set in `src/environments/`:
- `environment.development.ts` - Development config
- `environment.prod.ts` - Production config

## Next Steps

1. ✅ Backend running with Aspire
2. ✅ Frontend running with Aspire (integrated!)
3. ✅ Full stack orchestration with single command
4. ⏳ Implement authentication UI
5. ⏳ Implement receipt management UI
5. ⏳ Implement receipt management UI

## Useful Commands

```bash
# Check all running Docker containers
docker ps

# View Aspire logs
cd AppHost
dotnet run --verbose

# Angular production build
cd WarrantyApp.Web
npm run build:prod

# Run all .NET tests
dotnet test

# Format Angular code (if prettier installed)
cd WarrantyApp.Web
npx prettier --write "src/**/*.{ts,html,scss}"
```

## Documentation Quick Links

- [Main README](README.md) - Full project documentation
- [Design Reference](docs/27-design-reference.md) - UI/UX guidelines
- [Frontend Workflows](docs/28-frontend-workflows.md) - Implementation plan
- [Aspire Integration](docs/29-angular-aspire-integration.md) - Integration task
- [Setup Complete](docs/30-frontend-setup-complete.md) - What's been done

---

**Need Help?** Check the main README or individual component README files.
