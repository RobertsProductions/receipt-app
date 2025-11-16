# Receipt App - Warranty Management System

[![.NET CI/CD Pipeline](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml)

A modern warranty management application built with .NET 8 and .NET Aspire for cloud-native orchestration, featuring OpenAI-powered OCR for automatic receipt data extraction.

## Overview

This application provides a comprehensive warranty tracking system with a REST API backend orchestrated through .NET Aspire for simplified local development and deployment. Features include JWT authentication, receipt image/PDF upload, and AI-powered OCR to automatically extract merchant, amount, date, and product information from receipts.

## Technology Stack

- **.NET 8.0** - Latest LTS version of .NET
- **.NET Aspire 13.0** - Cloud-native orchestration and observability
- **ASP.NET Core Web API** - RESTful API backend
- **Entity Framework Core** - ORM with SQL Server support
- **ASP.NET Core Identity** - User authentication and authorization
- **OpenAI GPT-4o-mini** - AI-powered OCR for receipt processing
- **Swagger/OpenAPI** - API documentation and testing
- **GitHub Actions** - CI/CD pipeline

## Project Structure

```
MyAspireSolution/
├── .github/
│   └── workflows/
│       └── dotnet-ci.yml          # CI/CD pipeline configuration
├── docs/                          # Documentation
│   ├── 01-initial-setup.md        # Initial setup documentation
│   ├── 02-api-registration.md     # API registration with Aspire
│   ├── 03-cicd-setup.md           # GitHub Actions CI/CD pipeline
│   ├── 04-authentication-authorization.md  # JWT authentication
│   ├── 06-docker-database-setup.md     # Docker and database configuration
│   ├── 07-connection-fixes.md     # Database connection troubleshooting
│   ├── 08-receipt-upload-feature.md   # Receipt upload and management
│   └── 09-ocr-openai-integration.md   # OpenAI OCR integration
├── MyApi/                         # ASP.NET Core Web API
│   ├── Controllers/               # API endpoints
│   ├── Services/                  # Business logic & OCR service
│   ├── Models/                    # Data models
│   ├── DTOs/                      # Data transfer objects
│   ├── Data/                      # EF Core DbContext
│   ├── Program.cs
│   └── MyApi.csproj
├── AppHost/                       # Aspire AppHost orchestrator
│   ├── AppHost.cs                 # Service registration
│   ├── MyAspireApp.Host.csproj
│   └── appsettings.json
├── global.json                    # .NET SDK version pinning
├── SetOpenAiKey.ps1               # Helper script for OpenAI API key setup
├── MyAspireSolution.sln
└── README.md
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (version 8.0.302 or higher)
- [PowerShell 7+](https://aka.ms/powershell) (for Windows users)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (required for SQL Server container)
- [Git](https://git-scm.com/downloads)
- [OpenAI API Key](https://platform.openai.com/api-keys) (optional, for OCR features)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/RobertsProductions/receipt-app.git
   cd receipt-app
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Configure OpenAI API Key (Optional, for OCR features)**
   
   Use the provided PowerShell script:
   ```powershell
   .\SetOpenAiKey.ps1
   ```
   
   Or manually via user secrets:
   ```bash
   cd MyApi
   dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
   ```
   
   Or configure via Aspire Dashboard (recommended for testing):
   - Start the application
   - Open Aspire Dashboard
   - Navigate to Parameters
   - Set `openai-apikey` parameter

### Running the Application

#### Option 1: Using Aspire AppHost (Recommended)

```bash
cd AppHost
dotnet run
```

This will:
- Start the Aspire Dashboard
- Launch SQL Server container (persistent)
- Automatically launch the MyApi service
- Apply database migrations automatically
- Provide unified logging and telemetry
- Open the dashboard in your browser

#### Option 2: Run API Standalone

```bash
cd MyApi
dotnet run
```

Access Swagger UI at: `https://localhost:5001/swagger`

## Aspire Dashboard

When running through the AppHost, access the Aspire Dashboard at:
- URL: `https://localhost:17263`
- The login token will be displayed in the console output

The dashboard provides:
- **Resources**: View all running services and their status (API, SQL Server)
- **Console Logs**: Real-time logs from all services
- **Traces**: Distributed tracing information
- **Metrics**: Performance metrics and monitoring
- **Parameters**: Configure secrets like OpenAI API key

## API Documentation

Once the application is running, access the API documentation:

- **Swagger UI**: `https://localhost:{port}/swagger/index.html`
- **OpenAPI JSON**: `https://localhost:{port}/swagger/v1/swagger.json`

The port number will be displayed in the console or available in the Aspire Dashboard.

### Key Features

**Authentication & Authorization**
- JWT-based authentication with ASP.NET Core Identity
- User registration and login endpoints
- Secure password requirements and validation

**Receipt Management**
- Upload receipt images (JPG, PNG) and PDFs (max 10MB)
- Store receipt metadata (merchant, amount, date, warranty info)
- Download original receipt files
- Delete receipts with automatic file cleanup
- User-isolated storage (users only access their own receipts)

**AI-Powered OCR**
- Automatic extraction of merchant, amount, purchase date, and product name
- Optional OCR during upload (`UseOcr=true` parameter)
- Run OCR on existing receipts via dedicated endpoint
- Smart data merging (OCR only fills empty fields)
- Uses OpenAI GPT-4o-mini vision model (~$0.00015 per image)

## Development

### Project Configuration

The solution uses `global.json` to pin the .NET SDK version:
```json
{
  "sdk": {
    "version": "8.0.302"
  }
}
```

### Adding New Services

To add a new service to the Aspire orchestration, edit `AppHost/AppHost.cs`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var myApi = builder.AddProject<Projects.MyApi>("myapi");

// Add your new service here
var myNewService = builder.AddProject<Projects.MyNewService>("mynewservice");

builder.Build().Run();
```

## CI/CD Pipeline

The project includes a GitHub Actions workflow that:

1. **Build and Test**: Compiles the solution and runs tests
2. **Code Quality**: Checks code formatting standards
3. **Security Scan**: Scans for vulnerable dependencies
4. **Artifacts**: Publishes build artifacts for deployment

Pipeline triggers:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`
- Manual workflow dispatch

## Documentation

Detailed documentation is available in the `docs/` folder:

- [01 - Initial Setup](docs/01-initial-setup.md): Environment setup and project creation
- [02 - API Registration](docs/02-api-registration.md): Registering services with Aspire
- [03 - CI/CD Setup](docs/03-cicd-setup.md): GitHub Actions pipeline and repository setup
- [04 - Authentication & Authorization](docs/04-authentication-authorization.md): JWT-based authentication implementation
- [06 - Docker Database Setup](docs/06-docker-database-setup.md): Docker and database configuration guide
- [07 - Connection Fixes](docs/07-connection-fixes.md): Troubleshooting database connection issues
- [08 - Receipt Upload Feature](docs/08-receipt-upload-feature.md): Upload and manage receipt images and PDFs
- [09 - OpenAI OCR Integration](docs/09-ocr-openai-integration.md): AI-powered receipt data extraction setup and usage

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Build Status

| Branch | Status |
|--------|--------|
| main   | [![Build](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg?branch=main)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml) |
| develop | [![Build](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg?branch=develop)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml) |

## Troubleshooting

### Common Issues

**Issue**: SDK version mismatch
```
Solution: Ensure you have .NET 8.0.302 or compatible SDK installed
Check with: dotnet --list-sdks
```

**Issue**: Port already in use
```
Solution: Change the port in launchSettings.json or stop the conflicting process
```

**Issue**: Aspire Dashboard not accessible
```
Solution: Check the console output for the correct URL and token
```

**Issue**: Database connection errors
```
Solution: Ensure Docker Desktop is running and SQL Server container is started
Check Aspire Dashboard for SQL Server resource status
```

**Issue**: OCR not working
```
Solution: Configure OpenAI API key in Aspire Dashboard Parameters or user secrets
Verify key with: cd MyApi && dotnet user-secrets list
See docs/09-ocr-openai-integration.md for detailed setup
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or contributions, please:
- Open an issue on [GitHub Issues](https://github.com/RobertsProductions/receipt-app/issues)
- Check existing [documentation](docs/)
- Review closed issues for solutions

## Roadmap

- [x] Add authentication and authorization (JWT-based with ASP.NET Core Identity)
- [x] Implement database integration (Entity Framework Core with SQL Server)
- [x] Add SQL Server container orchestration with Aspire
- [x] Implement connection resiliency and retry logic
- [x] Configure dual database support (SQL Server & SQLite)
- [x] Add receipt upload and storage functionality
- [x] Implement file storage service for receipt images/PDFs
- [x] Add OCR for automatic receipt data extraction (OpenAI GPT-4o-mini)
- [x] Configure Aspire parameters for secret management
- [x] Automatic database migrations on startup
- [ ] Add warranty expiration notifications
- [ ] Implement PDF OCR support
- [ ] Add batch OCR processing
- [ ] Create frontend UI
- [ ] Add automated deployment
- [ ] Implement monitoring and alerting
- [ ] Add comprehensive test coverage
- [ ] Add refresh token support
- [ ] Implement two-factor authentication (2FA)
- [ ] Add email confirmation

---

**Built with** ❤️ **using .NET 8 and .NET Aspire**
