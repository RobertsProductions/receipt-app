# Receipt App - Warranty Management System

[![.NET CI/CD Pipeline](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml)

A modern warranty management application built with .NET 8 and .NET Aspire for cloud-native orchestration, featuring OpenAI-powered OCR for automatic receipt data extraction, proactive warranty expiration notifications via email and SMS, secure phone number verification, and comprehensive user management.

## Overview

This application provides a comprehensive warranty tracking system with a REST API backend orchestrated through .NET Aspire for simplified local development and deployment. Features include JWT authentication, receipt image/PDF upload, AI-powered OCR to automatically extract merchant, amount, date, and product information from receipts, a background service that monitors and notifies users about expiring warranties via email and optional SMS, SMS-based phone number verification with 6-digit codes, and full user profile management leveraging ASP.NET Core Identity's built-in fields for seamless integration.

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
│   ├── 05-database-resources-aspire.md     # Aspire database resources
│   ├── 06-docker-database-setup.md     # Docker and database configuration
│   ├── 07-connection-fixes.md     # Database connection troubleshooting
│   ├── 08-receipt-upload-feature.md   # Receipt upload and management
│   ├── 09-ocr-openai-integration.md   # OpenAI OCR integration
│   ├── 10-warranty-expiration-notifications.md   # Background notification service
│   ├── 11-email-sms-notifications.md  # Email and SMS notification configuration
│   ├── 12-user-profile-management.md   # User profile API
│   ├── 13-pdf-ocr-support.md      # PDF receipt OCR processing
│   ├── 14-phone-verification.md   # SMS phone number verification
│   └── 15-batch-ocr-processing.md # Batch OCR for multiple receipts
├── MyApi/                         # ASP.NET Core Web API
│   ├── Controllers/               # API endpoints
│   │   ├── AuthController.cs      # Authentication (register, login)
│   │   ├── ReceiptsController.cs  # Receipt management & OCR
│   │   ├── UserProfileController.cs    # User profile & preferences
│   │   └── WarrantyNotificationsController.cs  # Warranty monitoring
│   ├── Services/                  # Business logic services
│   │   ├── CompositeNotificationService.cs     # Multi-channel notifications
│   │   ├── EmailNotificationService.cs         # SMTP email service
│   │   ├── SmsNotificationService.cs           # Twilio SMS service
│   │   ├── LogNotificationService.cs           # Logging fallback
│   │   ├── PhoneVerificationService.cs         # SMS phone verification
│   │   ├── OpenAiOcrService.cs    # AI-powered OCR (image & PDF)
│   │   ├── LocalFileStorageService.cs  # File storage management
│   │   ├── TokenService.cs        # JWT token generation
│   │   ├── WarrantyExpirationService.cs  # Background warranty monitoring
│   │   ├── IPhoneVerificationService.cs        # Phone verification interface
│   │   ├── INotificationService.cs       # Notification interface
│   │   ├── IOcrService.cs         # OCR interface
│   │   ├── IFileStorageService.cs # Storage interface
│   │   ├── ITokenService.cs       # Token interface
│   │   └── JwtSettings.cs         # JWT configuration
│   ├── Models/                    # Data models
│   │   ├── ApplicationUser.cs     # User entity (Identity + preferences)
│   │   └── Receipt.cs             # Receipt entity
│   ├── DTOs/                      # Data transfer objects
│   │   ├── AuthResponseDto.cs     # Login/register response
│   │   ├── LoginDto.cs            # Login request
│   │   ├── RegisterDto.cs         # Registration request
│   │   ├── RefreshTokenRequestDto.cs  # Refresh token request
│   │   ├── ReceiptResponseDto.cs  # Receipt response
│   │   ├── UploadReceiptDto.cs    # Receipt upload request
│   │   ├── BatchOcrRequestDto.cs  # Batch OCR request
│   │   ├── BatchOcrResultDto.cs   # Batch OCR response
│   │   └── UserProfileDto.cs      # User profile, preferences, phone verification
│   ├── Data/                      # EF Core DbContext
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/            # Database migrations
│   ├── uploads/receipts/          # Local file storage
│   ├── Program.cs                 # Application entry point
│   ├── MyApi.csproj
│   └── MyApi.http                 # HTTP request samples
├── AppHost/                       # Aspire AppHost orchestrator
│   ├── AppHost.cs                 # Service registration
│   ├── MyAspireApp.Host.csproj
│   └── appsettings.json
├── global.json                    # .NET SDK version pinning
├── SetOpenAiKey.ps1               # Helper script for OpenAI API key setup
├── ConfigureEmail.ps1             # Helper script for email notification setup
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
- **Refresh token support** for seamless token renewal (7-day expiry)
- Token revocation for secure logout
- User profile management with preferences

**User Profile Management**
- Get and update profile information (FirstName, LastName)
- Manage phone number for SMS notifications
- SMS-based phone number verification with 6-digit codes
- Secure verification code generation and validation
- 5-minute code expiration with max 3 attempts
- Configure notification preferences (channel, threshold, opt-out)
- User-specific notification thresholds (1-90 days)
- Flexible notification channels (None, Email, SMS, Both)
- Complete opt-out functionality
- Phone number validation and privacy protection (masking)

**Receipt Management**
- Upload receipt images (JPG, PNG) and PDFs (max 10MB)
- Store receipt metadata (merchant, amount, date, warranty info)
- Download original receipt files
- Delete receipts with automatic file cleanup
- User-isolated storage (users only access their own receipts)

**AI-Powered OCR**
- Automatic extraction of merchant, amount, purchase date, and product name
- Support for both images (JPG, PNG) and PDF documents
- PDF text extraction with PdfPig library
- Optional OCR during upload (`UseOcr=true` parameter)
- Run OCR on existing receipts via dedicated endpoint
- **Batch OCR processing** for multiple receipts at once
- Smart data merging (OCR only fills empty fields)
- Image OCR uses OpenAI GPT-4o-mini vision model (~$0.00015 per image)
- PDF OCR uses text extraction + GPT-4o-mini (~$0.00005 per PDF, more cost-effective)
- Detailed batch results with success/failure tracking per receipt

**Warranty Expiration Notifications**
- Background service monitors warranties 24/7
- Configurable notification threshold (default: 7 days before expiration)
- Email notifications with HTML templates and urgency color coding
- Optional SMS notifications via Twilio (if user has phone number)
- Uses Identity's built-in Email and PhoneNumber fields
- Composite notification service (sends both email and SMS)
- In-memory caching for fast API access
- RESTful endpoints to query expiring warranties
- Prevents duplicate notifications
- Graceful degradation when SMS not configured
- Interactive configuration scripts (ConfigureEmail.ps1)
- Support for Gmail, Outlook, SendGrid, custom SMTP
- Production-ready with secure credential storage

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
- [05 - Database Resources in Aspire](docs/05-database-resources-aspire.md): Aspire SQL Server container configuration
- [06 - Docker Database Setup](docs/06-docker-database-setup.md): Docker and database configuration guide
- [07 - Connection Fixes](docs/07-connection-fixes.md): Troubleshooting database connection issues
- [08 - Receipt Upload Feature](docs/08-receipt-upload-feature.md): Upload and manage receipt images and PDFs
- [09 - OpenAI OCR Integration](docs/09-ocr-openai-integration.md): AI-powered receipt data extraction setup and usage
- [10 - Warranty Expiration Notifications](docs/10-warranty-expiration-notifications.md): Background service for warranty monitoring and notifications
- [11 - Email and SMS Notifications](docs/11-email-sms-notifications.md): Email and SMS notification setup, configuration, and testing
- [12 - User Profile Management](docs/12-user-profile-management.md): User profile API with notification preferences and phone number management
- [13 - PDF OCR Support](docs/13-pdf-ocr-support.md): PDF receipt processing with text extraction and AI-powered data extraction
- [14 - Phone Verification](docs/14-phone-verification.md): SMS-based phone number verification with secure 6-digit codes
- [15 - Batch OCR Processing](docs/15-batch-ocr-processing.md): Batch OCR functionality for processing multiple receipts at once
- [16 - Refresh Token Support](docs/16-refresh-token-support.md): JWT refresh tokens for seamless authentication renewal

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

### Completed Features ✅
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
- [x] Add warranty expiration notifications (background service with caching)
- [x] Implement email/SMS notifications for warranty expirations (SMTP, Twilio)
- [x] Add user profile management API (update phone number, preferences)
- [x] Implement PDF OCR support (PdfPig + OpenAI text extraction)
- [x] Add phone number verification (SMS confirmation code with expiration)
- [x] Add batch OCR processing (multiple receipts at once)
- [x] Add refresh token support (JWT token renewal with 7-day expiry)

### Backend Tasks (No UI Required) 🔧
- [ ] Implement two-factor authentication (2FA)
- [ ] Add email confirmation
- [ ] Implement monitoring and alerting
- [ ] Add comprehensive test coverage
- [ ] Add automated deployment

### Frontend/UI Tasks 🎨
- [ ] Create frontend UI
- [ ] Implement notification preferences UI/documentation improvements

---

**Built with** ❤️ **using .NET 8 and .NET Aspire**
