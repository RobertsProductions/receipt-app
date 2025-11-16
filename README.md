# Receipt App - Warranty Management System

[![.NET CI/CD Pipeline](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml)

A modern warranty management application built with .NET 8 and .NET Aspire for cloud-native orchestration, featuring OpenAI-powered OCR for automatic receipt data extraction, proactive warranty expiration notifications via email and SMS, secure phone number verification, and comprehensive user management.

## Overview

This application provides a comprehensive warranty tracking system with a REST API backend orchestrated through .NET Aspire for simplified local development and deployment. Features include JWT authentication, receipt image/PDF upload, AI-powered OCR to automatically extract merchant, amount, date, and product information from receipts, a background service that monitors and notifies users about expiring warranties via email and optional SMS, SMS-based phone number verification with 6-digit codes, and full user profile management leveraging ASP.NET Core Identity's built-in fields for seamless integration.

## Technology Stack

### Backend
- **.NET 8.0** - Latest LTS version of .NET
- **.NET Aspire 13.0** - Cloud-native orchestration and observability
- **ASP.NET Core Web API** - RESTful API backend
- **Entity Framework Core** - ORM with SQL Server support
- **ASP.NET Core Identity** - User authentication and authorization
- **OpenAI GPT-4o-mini** - AI-powered OCR for receipt processing
- **Swagger/OpenAPI** - API documentation and testing
- **GitHub Actions** - CI/CD pipeline

### Frontend
- **Angular** - Modern TypeScript-based framework for building web applications
- **ESLint** - Pluggable linting utility for code quality and consistency

## Project Structure

```
MyAspireSolution/
├── .github/
│   └── workflows/
│       └── dotnet-ci.yml                           # CI/CD pipeline configuration
├── docs/                                           # Documentation
│   ├── 01-initial-setup.md                         # Initial setup documentation
│   ├── 02-api-registration.md                      # API registration with Aspire
│   ├── 03-cicd-setup.md                            # GitHub Actions CI/CD pipeline
│   ├── 04-authentication-authorization.md          # JWT authentication
│   ├── 05-database-resources-aspire.md             # Aspire database resources
│   ├── 06-docker-database-setup.md                 # Docker and database configuration
│   ├── 07-connection-fixes.md                      # Database connection troubleshooting
│   ├── 08-receipt-upload-feature.md                # Receipt upload and management
│   ├── 09-ocr-openai-integration.md                # OpenAI OCR integration
│   ├── 10-warranty-expiration-notifications.md     # Background notification service
│   ├── 11-email-sms-notifications.md               # Email and SMS notification configuration
│   ├── 12-user-profile-management.md               # User profile API
│   ├── 13-pdf-ocr-support.md                       # PDF receipt OCR processing
│   ├── 14-phone-verification.md                    # SMS phone number verification
│   ├── 15-batch-ocr-processing.md                  # Batch OCR for multiple receipts
│   ├── 16-refresh-token-support.md                 # JWT refresh tokens
│   ├── 17-two-factor-authentication.md             # 2FA with TOTP
│   ├── 18-email-confirmation.md                    # Email address verification
│   ├── 19-monitoring-and-alerting.md               # Health checks and monitoring
│   ├── 20-testing-strategy.md                      # Comprehensive testing strategy
│   ├── 21-automated-deployment.md                  # Azure Container Apps deployment
│   ├── 22-code-quality-improvements.md             # XML documentation and error responses
│   ├── 23-receipt-sharing.md                       # Receipt sharing with read-only access
│   ├── 24-ai-chatbot-receipt-queries.md            # AI chatbot for natural language queries
│   ├── 25-performance-optimization.md              # Response caching, indexes, rate limiting, compression
│   ├── 26-user-data-caching.md                     # Automatic user data caching on login
│   └── 27-design-reference.md                      # UI/UX design guidelines and style reference
├── MyApi/                                          # ASP.NET Core Web API
│   ├── Controllers/                                # API endpoints
│   │   ├── AuthController.cs                       # Authentication (register, login)
│   │   ├── ReceiptsController.cs                   # Receipt management & OCR
│   │   ├── UserProfileController.cs                # User profile & preferences
│   │   ├── WarrantyNotificationsController.cs      # Warranty monitoring
│   │   ├── ReceiptSharingController.cs             # Receipt sharing
│   │   └── ChatbotController.cs                    # AI-powered receipt queries
│   ├── HealthChecks/                               # Health check implementations
│   │   ├── OpenAiHealthCheck.cs                    # OpenAI API connectivity check
│   │   ├── SmtpHealthCheck.cs                      # SMTP server connectivity check
│   │   ├── TwilioHealthCheck.cs                    # Twilio API connectivity check
│   │   └── FileStorageHealthCheck.cs               # File storage and disk space check
│   ├── Services/                                   # Business logic services
│   │   ├── CompositeNotificationService.cs         # Multi-channel notifications
│   │   ├── EmailNotificationService.cs             # SMTP email service
│   │   ├── SmsNotificationService.cs               # Twilio SMS service
│   │   ├── LogNotificationService.cs               # Logging fallback
│   │   ├── PhoneVerificationService.cs             # SMS phone verification
│   │   ├── OpenAiOcrService.cs                     # AI-powered OCR (image & PDF)
│   │   ├── ChatbotService.cs                       # AI chatbot for receipt queries
│   │   ├── LocalFileStorageService.cs              # File storage management
│   │   ├── TokenService.cs                         # JWT token generation
│   │   ├── WarrantyExpirationService.cs            # Background warranty monitoring
│   │   ├── UserCacheService.cs                     # User data caching service
│   │   ├── IPhoneVerificationService.cs            # Phone verification interface
│   │   ├── IChatbotService.cs                      # Chatbot interface
│   │   ├── INotificationService.cs                 # Notification interface
│   │   ├── IOcrService.cs                          # OCR interface
│   │   ├── IFileStorageService.cs                  # Storage interface
│   │   ├── ITokenService.cs                        # Token interface
│   │   ├── IUserCacheService.cs                    # User cache interface
│   │   └── JwtSettings.cs                          # JWT configuration
│   ├── Models/                                     # Data models
│   │   ├── ApplicationUser.cs                      # User entity (Identity + preferences)
│   │   ├── Receipt.cs                              # Receipt entity
│   │   ├── ReceiptShare.cs                         # Receipt sharing entity
│   │   └── ChatMessage.cs                          # Chatbot conversation history
│   ├── DTOs/                                       # Data transfer objects
│   │   ├── AuthResponseDto.cs                      # Login/register response
│   │   ├── LoginDto.cs                             # Login request
│   │   ├── RegisterDto.cs                          # Registration request
│   │   ├── RefreshTokenRequestDto.cs               # Refresh token request
│   │   ├── ReceiptResponseDto.cs                   # Receipt response
│   │   ├── UploadReceiptDto.cs                     # Receipt upload request
│   │   ├── BatchOcrRequestDto.cs                   # Batch OCR request
│   │   ├── BatchOcrResultDto.cs                    # Batch OCR response
│   │   └── UserProfileDto.cs                       # User profile, preferences, phone verification
│   ├── Data/                                       # EF Core DbContext
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/                             # Database migrations
│   ├── uploads/receipts/                           # Local file storage
│   ├── Program.cs                                  # Application entry point
│   ├── MyApi.csproj
│   └── MyApi.http                                  # HTTP request samples
├── AppHost/                                        # Aspire AppHost orchestrator
│   ├── AppHost.cs                                  # Service registration
│   ├── MyAspireApp.Host.csproj
│   └── appsettings.json
├── global.json                                     # .NET SDK version pinning
├── SetOpenAiKey.ps1                                # Helper script for OpenAI API key setup
├── ConfigureEmail.ps1                              # Helper script for email notification setup
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
- **Email confirmation** with secure tokens and HTML templates
- Automatic confirmation email on registration
- Resend confirmation email functionality
- **Refresh token support** for seamless token renewal (7-day expiry)
- Token revocation for secure logout
- **Two-factor authentication (2FA)** with TOTP (Time-based One-Time Password)
- QR code setup for authenticator apps (Google Authenticator, Microsoft Authenticator, Authy)
- 10 recovery codes for account recovery
- Secure 2FA enable/disable with code verification
- User profile management with preferences

**Health Checks & Monitoring**
- Comprehensive health check endpoints (`/health`, `/health/ready`, `/health/live`)
- Database connectivity and query execution monitoring
- External service health checks (OpenAI, SMTP, Twilio)
- File storage health and disk space monitoring
- Detailed health status with response times and metrics
- Integration with Aspire Dashboard for real-time monitoring
- Kubernetes-ready liveness and readiness probes
- JSON health check responses with detailed component status

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

**Receipt Sharing**
- Share receipts with other users (read-only access)
- Share by email or username
- List receipts shared with you
- List receipts you've shared with others
- Revoke sharing access at any time
- Shared receipts included in warranty expiration monitoring
- Audit logging for shared receipt access
- Email notifications when receipts are shared with you

**AI Chatbot for Receipt Queries**
- Natural language queries about your receipts
- Search by merchant, date range, amount, product name
- Get spending statistics (total, average, by merchant)
- Query warranty status and expiring warranties
- Conversation history with context tracking
- Suggested questions for common queries
- Powered by OpenAI GPT-4o-mini for intelligent responses
- Rate limited to prevent abuse
- Chat messages persisted to database

## Testing

The application includes comprehensive test coverage with **119 passing tests** across all critical components:

### Test Suite Overview
- **Total Tests**: 119 (100% pass rate ✅)
- **Execution Time**: ~42 seconds
- **Coverage**: Service layer and models

### Test Categories

**Service Tests** (100 tests):
- TokenService (12): JWT generation, validation, refresh tokens
- LocalFileStorageService (11): File operations, user isolation
- EmailNotificationService (14): Email delivery, templates
- WarrantyExpirationService (17): Expiration detection, notifications
- CompositeNotificationService (8): Multi-channel routing
- LogNotificationService (12): Structured logging
- PhoneVerificationService (10): SMS codes, verification
- OpenAiOcrService (16): OCR processing, file handling

**Model Tests** (19 tests):
- ApplicationUser and Receipt validation
- Entity relationships and data annotations

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~TokenServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true

# Check vulnerable packages
dotnet list package --vulnerable
```

### Testing Strategy

The application follows a pragmatic testing approach:
- ✅ **Service Layer**: Complete with 100 tests covering all business logic
- ✅ **Models**: Complete with 19 tests for validation and relationships
- 🔮 **Controllers**: Deferred to E2E tests with Playwright (after frontend development)
- 🔮 **E2E Tests**: Planned with Playwright for integration and UI workflows

Controller tests are intentionally skipped in favor of comprehensive E2E testing that will validate the entire request-response flow including authentication, authorization, and real HTTP interactions. This approach provides better coverage of real-world scenarios while avoiding the complexity of mocking authentication and HTTP contexts.

See [docs/20-testing-strategy.md](docs/20-testing-strategy.md) for detailed testing strategy and rationale.

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

1. **Build and Test**: Compiles the solution and runs all 119 tests (100% pass rate)
2. **Code Quality**: Checks code formatting standards
3. **Security Scan**: Scans for vulnerable dependencies
4. **Artifacts**: Publishes build artifacts for deployment

**Test Coverage Status**: ✅ 119 tests passing
- Service Layer: 100 tests (TokenService, FileStorage, Notifications, Warranty, OCR, PhoneVerification)
- Model Layer: 19 tests (ApplicationUser, Receipt validation)
- Execution Time: ~42 seconds

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
- [17 - Two-Factor Authentication](docs/17-two-factor-authentication.md): TOTP-based 2FA with authenticator apps and recovery codes
- [18 - Email Confirmation](docs/18-email-confirmation.md): Email address verification with secure tokens and professional HTML templates
- [19 - Monitoring and Alerting](docs/19-monitoring-and-alerting.md): Comprehensive health checks for all system components and dependencies
- [20 - Testing Strategy](docs/20-testing-strategy.md): Comprehensive testing strategy with 119 passing tests (100% pass rate) covering services and models
- [21 - Automated Deployment](docs/21-automated-deployment.md): Multi-platform deployment strategies with Azure Container Apps, Docker Compose, and CI/CD workflows
- [22 - Code Quality Improvements](docs/22-code-quality-improvements.md): XML documentation for all API endpoints and standardized error response patterns
- [23 - Receipt Sharing](docs/23-receipt-sharing.md): Share receipts with other users (read-only access) with warranty monitoring integration
- [24 - AI Chatbot Receipt Queries](docs/24-ai-chatbot-receipt-queries.md): Natural language receipt queries with AI-powered chatbot, conversation history, and suggested questions
- [25 - Performance Optimization](docs/25-performance-optimization.md): Response caching, database indexes, rate limiting, and compression for improved performance and scalability
- [26 - User Data Caching](docs/26-user-data-caching.md): Automatic user data preloading into cache on login for 10-30x faster response times
- [27 - Design Reference](docs/27-design-reference.md): Comprehensive UI/UX design guidelines, color palette, typography, component specifications, and responsive design patterns

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
- [x] Implement two-factor authentication (2FA with TOTP, QR codes, recovery codes)
- [x] Add email confirmation (secure token verification with HTML templates)
- [x] Implement monitoring and alerting (health checks for all components)
- [x] Add receipt sharing (share receipts with read-only access and warranty monitoring)
- [x] Implement AI chatbot for receipt queries (natural language queries, conversation history)
- [x] Add performance optimizations (response caching, database indexes, rate limiting, compression)
- [x] Add user data caching on login (automatic preloading for 10-30x faster response times)

### Backend Tasks (No UI Required) 🔧

**Security & Dependencies**
- [x] Fix SixLabors.ImageSharp vulnerability (upgraded to 3.1.12)
- [x] Fix async warning in SmsNotificationService

**Test Coverage Expansion**
- [x] Phase 1: Model tests (100% coverage - ApplicationUser, Receipt, ReceiptShare, ChatMessage) 
- [x] Phase 2: Service tests - All services complete with 117 tests
  - TokenService (12 tests)
  - LocalFileStorageService (11 tests)
  - EmailNotificationService (14 tests)
  - SmsNotificationService (0 tests - thin wrapper around Twilio)
  - CompositeNotificationService (8 tests)
  - LogNotificationService (12 tests)
  - PhoneVerificationService (10 tests)
  - OpenAiOcrService (16 tests)
  - WarrantyExpirationService (17 tests)
  - ChatbotService (17 tests)
- [ ] Phase 3: E2E tests with Playwright (after frontend implementation)
  - Authentication flows (login, register, 2FA, email confirmation)
  - Receipt workflows (upload, OCR, batch processing, download, delete)
  - User profile management (update profile, phone verification, preferences)
  - Warranty notifications (expiring warranties API)
- [ ] Generate code coverage report (current: service layer ~100%)

**Code Quality**
- [x] Add XML documentation comments to public APIs
- [x] Review and standardize API error responses

**Deployment & Infrastructure**
- [x] Create deployment workflows (Azure Container Apps documented)
- [ ] Configure GitHub secrets for production deployment
- [ ] Provision Azure resources (ACR, SQL Database, Container Apps)
- [ ] Test deployment workflow end-to-end
- [ ] Configure Application Insights monitoring
- [ ] Create operations runbook documentation

**Performance & Optimization**
- [x] Add response caching for GET endpoints
- [x] Optimize database queries and add indexes
- [x] Implement rate limiting middleware
- [x] Add request/response compression

**Receipt Sharing Features**
- [x] Add receipt sharing data model (ReceiptShare entity with owner and recipient)
- [x] Implement share receipt API (share by email/username with read-only access)
- [x] Add shared receipts endpoints (list shared with me, list my shares)
- [x] Implement revoke sharing functionality
- [x] Add shared receipts to warranty expiration monitoring
- [x] Create shared receipt access audit logging
- [x] Implement notifications for new shared receipts

**AI Chatbot for Receipt Queries**
- [x] Design chatbot conversation interface and message format
- [x] Implement OpenAI integration for natural language processing
- [x] Create receipt query service (search by merchant, date, amount, product)
- [x] Add conversation history management and context tracking
- [x] Implement chat endpoints (send message, get history, clear conversation)
- [x] Add support for natural language date parsing (e.g., "last month", "this year")
- [x] Implement receipt statistics queries (total spending, category breakdown)
- [x] Add warranty status queries via chatbot
- [x] Create suggested questions/prompts for common queries
- [x] Add chat message persistence to database
- [x] Implement rate limiting for chatbot API calls

### Frontend/UI Tasks 🎨
- [x] Choose frontend framework (Angular selected)
- [x] Create design reference document
- [ ] Set up Angular project with TypeScript
- [ ] Configure ESLint for code quality and consistency
- [ ] Create UI wireframes and mockups based on design reference
- [ ] Implement design system (colors, typography, components)
- [ ] Implement authentication UI (login, register, 2FA)
- [ ] Implement receipt management UI (upload, view, OCR)
- [ ] Implement warranty dashboard
- [ ] Implement user profile and preferences UI
- [ ] Implement phone verification flow UI
- [ ] Implement receipt sharing UI (share modal, manage shares, shared receipts view)
- [ ] Implement AI chatbot UI (chat window, message history, suggested queries)
- [ ] Add responsive design for mobile
- [ ] Add frontend deployment workflow

---

**Built with** ❤️ **using .NET 8 and .NET Aspire**
