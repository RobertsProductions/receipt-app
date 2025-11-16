# Quick Start Guide - Warranty App

## Start the Application (Current Setup)

### Prerequisites Check
```bash
# Verify Node.js and npm
node --version  # Should be 18+
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

### Step 2: Start Backend (Terminal 1)

```bash
# From solution root
cd AppHost
dotnet run
```

**What happens:**
- Aspire dashboard opens at `http://localhost:15000`
- SQL Server container starts
- MyApi starts at `http://localhost:5000`
- Database migrations apply automatically

### Step 3: Start Frontend (Terminal 2)

```bash
# From solution root (new terminal)
cd WarrantyApp.Web
npm start
```

**What happens:**
- Angular dev server starts at `http://localhost:4200`
- API requests automatically proxy to backend
- Hot reload enabled for code changes

### Step 4: Access the Application

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Aspire Dashboard**: http://localhost:15000

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
- Check proxy.conf.json configuration
- Verify API allows localhost:4200 origin

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
2. ✅ Frontend running standalone
3. ⏳ Integrate frontend with Aspire (see docs/29-angular-aspire-integration.md)
4. ⏳ Implement authentication UI
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
