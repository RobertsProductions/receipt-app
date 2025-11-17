# MyApi - Warranty Management REST API

**Version**: 1.0.0  
**Last Updated**: November 17, 2025  
**Status**: 🚀 **Production-Ready - 100% Backend Features Complete**

This is the backend REST API for the Warranty Management System, built with ASP.NET Core 8.0. The API provides comprehensive endpoints for user authentication, receipt management, AI-powered OCR, warranty tracking, notifications, and more.

## 🎉 Current Status

**What's Built:**
- ✅ 100% of backend features complete
- ✅ 146 passing unit tests (100% pass rate)
- ✅ 15 REST API controllers with 70+ endpoints
- ✅ Comprehensive health checks and monitoring
- ✅ OpenAPI/Swagger documentation
- ✅ Production-ready with security best practices

**Key Capabilities:**
1. JWT authentication with refresh tokens and 2FA
2. Receipt upload (images and PDFs) with file storage
3. OpenAI-powered OCR for automatic data extraction
4. Warranty expiration monitoring with background service
5. Email and SMS notifications (SMTP, Twilio)
6. User profile management with phone verification
7. Receipt sharing with read-only access
8. AI chatbot for natural language receipt queries
9. Performance optimizations (caching, rate limiting)
10. Comprehensive health checks for all dependencies

## Technology Stack

- **ASP.NET Core**: 8.0 - Modern web framework
- **Entity Framework Core**: 8.0 - ORM with SQL Server/SQLite support
- **ASP.NET Core Identity**: User authentication and authorization
- **JWT Bearer**: Token-based authentication
- **OpenAI API**: GPT-4o-mini for OCR and chatbot
- **Twilio**: SMS notifications and phone verification
- **SMTP**: Email notifications (Gmail, Outlook, SendGrid)
- **PdfPig**: PDF text extraction
- **ImageSharp**: Image processing and validation
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (Docker container) or SQLite for development
- OpenAI API key (for OCR and chatbot features)
- Twilio account (optional, for SMS features)
- SMTP credentials (optional, for email notifications)

## Getting Started

### Install Dependencies

Dependencies are automatically restored when building the project:

```bash
dotnet restore
```

### Configuration

#### 1. Database Configuration

The API supports both SQL Server (production) and SQLite (development):

**SQL Server (via Aspire):**
```bash
# No configuration needed - Aspire manages the connection string
cd ../AppHost
dotnet run
```

**SQLite (standalone development):**
```bash
# Set environment variable
$env:USE_SQLITE = "true"
dotnet run
```

#### 2. OpenAI API Key

**Option A: Via Aspire Dashboard (Recommended)**
1. Start the AppHost: `cd ../AppHost && dotnet run`
2. Open Aspire Dashboard (URL shown in console)
3. Navigate to **Parameters**
4. Set `openai-apikey` parameter

**Option B: User Secrets (Standalone)**
```bash
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
```

#### 3. Email Configuration (Optional)

Run the interactive configuration script:
```powershell
cd ../MyApi
.\ConfigureEmail.ps1
```

Or configure manually via user secrets:
```bash
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SenderPassword" "your-app-password"
```

#### 4. Twilio Configuration (Optional)

For SMS notifications and phone verification:
```bash
dotnet user-secrets set "Twilio:AccountSid" "your-account-sid"
dotnet user-secrets set "Twilio:AuthToken" "your-auth-token"
dotnet user-secrets set "Twilio:PhoneNumber" "+1234567890"
```

### Running the API

#### Option 1: Via Aspire AppHost (Recommended)

```bash
cd ../AppHost
dotnet run
```

This will:
- Start SQL Server container
- Launch the API on a dynamic port
- Open Aspire Dashboard for monitoring
- Provide unified logging and telemetry

Access:
- **API**: Check Aspire Dashboard for assigned port
- **Swagger UI**: `https://localhost:{port}/swagger`
- **Health Checks**: `https://localhost:{port}/health`

#### Option 2: Standalone

```bash
cd MyApi
dotnet run
```

Access:
- **API**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`
- **Health Checks**: `https://localhost:5001/health`

### Running Tests

Execute all 146 unit tests:

```bash
dotnet test
```

Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~TokenServiceTests"
```

Generate code coverage:
```bash
dotnet test /p:CollectCoverage=true
```

### Building

Development build:
```bash
dotnet build
```

Release build:
```bash
dotnet build -c Release
```

Publish for deployment:
```bash
dotnet publish -c Release -o ./publish
```

## Project Structure

```
MyApi/
├── Controllers/                    # API endpoints (15 controllers)
│   ├── AuthController.cs          # Authentication (login, register, 2FA)
│   ├── ReceiptsController.cs      # Receipt CRUD and OCR
│   ├── UserProfileController.cs   # User profile management
│   ├── WarrantyNotificationsController.cs  # Warranty monitoring
│   ├── ReceiptSharingController.cs # Receipt sharing
│   ├── ChatbotController.cs       # AI chatbot queries
│   └── ...                         # Additional controllers
├── Services/                       # Business logic (16 services)
│   ├── TokenService.cs            # JWT token generation/validation
│   ├── LocalFileStorageService.cs # File storage management
│   ├── OpenAiOcrService.cs        # OCR processing
│   ├── WarrantyExpirationService.cs # Background monitoring
│   ├── EmailNotificationService.cs # Email sending
│   ├── SmsNotificationService.cs  # SMS via Twilio
│   ├── ChatbotService.cs          # AI chatbot logic
│   └── ...                         # Additional services
├── Models/                         # Entity models (6 entities)
│   ├── ApplicationUser.cs         # User with preferences
│   ├── Receipt.cs                 # Receipt entity
│   ├── ReceiptShare.cs            # Receipt sharing
│   ├── ChatMessage.cs             # Chatbot conversations
│   └── ...                         # Additional models
├── DTOs/                           # Data transfer objects
│   ├── Auth/                      # Authentication DTOs
│   ├── Receipts/                  # Receipt DTOs
│   ├── UserProfile/               # Profile DTOs
│   └── ...                        # Additional DTOs
├── Data/                           # Database context
│   └── ApplicationDbContext.cs    # EF Core DbContext
├── Migrations/                     # EF Core migrations
├── HealthChecks/                   # Custom health checks
│   ├── OpenAiHealthCheck.cs       # OpenAI API health
│   ├── SmtpHealthCheck.cs         # SMTP server health
│   ├── TwilioHealthCheck.cs       # Twilio API health
│   └── FileStorageHealthCheck.cs  # Disk space health
├── Middleware/                     # Custom middleware
│   └── RateLimitingMiddleware.cs  # Rate limiting
├── Attributes/                     # Custom attributes
│   └── RequireClaimAttribute.cs   # Claim-based authorization
├── uploads/                        # Receipt file storage
├── Program.cs                      # Application entry point
├── appsettings.json               # Configuration
└── MyApi.csproj                   # Project file
```

## API Endpoints

### Authentication (`/api/Auth`)
- `POST /register` - Register new user
- `POST /login` - Login with email/password
- `POST /logout` - Logout and revoke tokens
- `POST /refresh` - Refresh access token
- `POST /confirm-email` - Confirm email address
- `POST /resend-confirmation-email` - Resend confirmation
- `GET /2fa/setup` - Get 2FA QR code
- `POST /2fa/enable` - Enable 2FA with code
- `POST /2fa/disable` - Disable 2FA
- `POST /2fa/verify` - Verify 2FA code

### Receipts (`/api/Receipts`)
- `GET /` - Get all receipts (paginated)
- `GET /{id}` - Get receipt by ID
- `POST /` - Upload new receipt (with optional OCR)
- `POST /{id}/ocr` - Run OCR on existing receipt
- `POST /batch-ocr` - Process multiple receipts
- `PUT /{id}` - Update receipt
- `DELETE /{id}` - Delete receipt
- `GET /{id}/download` - Download receipt file

### User Profile (`/api/UserProfile`)
- `GET /` - Get current user profile
- `PUT /` - Update profile
- `POST /change-password` - Change password
- `POST /send-verification-code` - Send SMS verification
- `POST /verify-phone` - Verify phone number

### Warranty Notifications (`/api/WarrantyNotifications`)
- `GET /expiring` - Get expiring warranties
- `POST /check` - Trigger manual warranty check

### Receipt Sharing (`/api/ReceiptSharing`)
- `POST /share` - Share receipt with user
- `GET /shared-with-me` - Get receipts shared with you
- `GET /my-shares` - Get receipts you've shared
- `DELETE /{shareId}` - Revoke share access

### Chatbot (`/api/Chatbot`)
- `POST /message` - Send message to chatbot
- `GET /conversation` - Get conversation history
- `DELETE /conversation` - Clear conversation
- `GET /suggested-questions` - Get suggested questions

### Health (`/health`)
- `GET /health` - Complete health status
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

## Features

### Authentication & Security
- ✅ JWT-based authentication with 15-minute access tokens
- ✅ Refresh tokens with 7-day expiry
- ✅ Two-factor authentication (TOTP with QR codes)
- ✅ Email confirmation with secure tokens
- ✅ Password requirements (8+ chars, uppercase, lowercase, digit, special char)
- ✅ Rate limiting to prevent abuse
- ✅ CORS configuration
- ✅ Request/response compression

### Receipt Management
- ✅ Upload images (JPG, PNG) and PDFs (max 10MB)
- ✅ User-isolated file storage
- ✅ Automatic file cleanup on deletion
- ✅ Receipt metadata (merchant, amount, date, warranty, product, notes)
- ✅ Pagination support for large collections
- ✅ File download with proper MIME types

### AI-Powered OCR
- ✅ OpenAI GPT-4o-mini integration
- ✅ Automatic extraction (merchant, amount, date, product)
- ✅ PDF text extraction with PdfPig
- ✅ Batch processing for multiple receipts
- ✅ Smart data merging (preserves user edits)
- ✅ Cost-effective: ~$0.00015/image, ~$0.00005/PDF

### Warranty Monitoring
- ✅ Background service runs hourly
- ✅ User-specific thresholds (1-90 days)
- ✅ Email notifications with HTML templates
- ✅ Optional SMS notifications via Twilio
- ✅ In-memory caching for performance
- ✅ Duplicate notification prevention
- ✅ Shared receipts included in monitoring

### Notifications
- ✅ SMTP email service (Gmail, Outlook, SendGrid)
- ✅ HTML email templates with branding
- ✅ Twilio SMS integration
- ✅ Phone verification with 6-digit codes
- ✅ Multi-channel routing (Email/SMS/Both/None)
- ✅ User preferences per notification type

### Receipt Sharing
- ✅ Share with other users (email or username)
- ✅ Read-only access for recipients
- ✅ List shares (incoming and outgoing)
- ✅ Revoke access anytime
- ✅ Email notifications on share
- ✅ Audit logging

### AI Chatbot
- ✅ Natural language queries
- ✅ Search by merchant, date, amount, product
- ✅ Spending statistics and analytics
- ✅ Warranty status queries
- ✅ Conversation history with context
- ✅ Suggested questions
- ✅ Rate limiting

### Performance & Monitoring
- ✅ Response caching (5-minute cache)
- ✅ Database indexes on key columns
- ✅ User data preloading on login
- ✅ 10-30x faster API responses (cached)
- ✅ Health checks for all dependencies
- ✅ Aspire Dashboard integration
- ✅ Structured logging

### Testing & Quality
- ✅ 146 passing unit tests (100% pass rate)
- ✅ Service layer: 117 tests
- ✅ Model layer: 29 tests
- ✅ Fast execution (~42 seconds)
- ✅ XML documentation on all public APIs
- ✅ Standardized error responses
- ✅ CI/CD pipeline with GitHub Actions

## Configuration Files

### `appsettings.json`
Base configuration for all environments:
- JWT settings (secret key, issuer, audience)
- Database provider selection
- CORS origins
- File upload limits

### `appsettings.Development.json`
Development-specific settings:
- Detailed logging
- Development CORS policy
- SQLite connection string (if enabled)

### `appsettings.SqlServer.json`
SQL Server configuration:
- Connection string template
- Entity Framework provider

### `appsettings.Sqlite.json`
SQLite configuration:
- Local database file path
- Development database provider

## Environment Variables

- `USE_SQLITE` - Use SQLite instead of SQL Server (true/false)
- `ASPNETCORE_ENVIRONMENT` - Environment name (Development/Production)
- `ASPNETCORE_URLS` - URLs to bind (e.g., `https://localhost:5001`)

## User Secrets

Sensitive configuration stored in user secrets (development) or environment variables (production):

```bash
# OpenAI
OpenAI:ApiKey

# Email (SMTP)
EmailSettings:SmtpServer
EmailSettings:SmtpPort
EmailSettings:SenderEmail
EmailSettings:SenderPassword

# SMS (Twilio)
Twilio:AccountSid
Twilio:AuthToken
Twilio:PhoneNumber

# JWT (Production)
JwtSettings:SecretKey
```

## Database Migrations

Create new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

Remove last migration:
```bash
dotnet ef migrations remove
```

**Note**: Migrations are applied automatically on startup in development.

## Health Checks

The API includes comprehensive health checks:

### Database Health
- Connection test with query execution
- Response time monitoring

### OpenAI Health
- API connectivity check
- API key validation

### SMTP Health
- SMTP server connectivity
- Port accessibility test

### Twilio Health
- API connectivity check
- Account validation

### File Storage Health
- Disk space monitoring
- Directory accessibility

Access health checks:
- **Complete**: `GET /health` - All component status
- **Ready**: `GET /health/ready` - Kubernetes readiness probe
- **Live**: `GET /health/live` - Kubernetes liveness probe

## API Documentation

### Swagger UI
Interactive API documentation with request/response examples:
- **URL**: `https://localhost:{port}/swagger`
- Try out endpoints directly from browser
- View request/response schemas
- Download OpenAPI specification

### OpenAPI Spec
- **JSON**: `https://localhost:{port}/swagger/v1/swagger.json`
- Import into Postman, Insomnia, or other API clients

## Authentication Flow

### Register & Login
1. **Register**: `POST /api/Auth/register`
   - Receive email confirmation
   - Click confirmation link
2. **Login**: `POST /api/Auth/login`
   - Receive access token (15 min) and refresh token (7 days)
   - Include access token in `Authorization: Bearer {token}` header

### Token Refresh
3. **Refresh**: `POST /api/Auth/refresh`
   - Send expired access token + refresh token
   - Receive new access token
   - Seamless token renewal without re-login

### Two-Factor Authentication (Optional)
4. **Setup 2FA**: `GET /api/Auth/2fa/setup`
   - Receive QR code for authenticator app
5. **Enable 2FA**: `POST /api/Auth/2fa/enable`
   - Verify with 6-digit code
   - Receive 10 recovery codes
6. **Login with 2FA**: `POST /api/Auth/login`
   - Include `TwoFactorCode` in request
   - Authenticate with authenticator app code

## Error Handling

The API returns standardized error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Receipt with ID 123 not found"
}
```

Common status codes:
- `200 OK` - Success
- `201 Created` - Resource created
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Missing or invalid token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate resource
- `500 Internal Server Error` - Server error

## Performance Considerations

### Caching Strategy
- User profile: 30 minutes
- Receipts: 5 minutes
- Warranty notifications: 5 minutes
- Cache cleared on data modification

### Rate Limiting
- Default: 100 requests per 1 minute per IP
- Configurable per endpoint
- Prevents abuse and ensures fair usage

### Database Optimization
- Indexes on UserId, PurchaseDate, WarrantyEndDate
- Lazy loading for navigation properties
- Efficient LINQ queries
- Minimal database roundtrips

## Security Best Practices

✅ **Implemented:**
- JWT tokens with short expiry
- Refresh tokens for seamless renewal
- Password hashing with ASP.NET Core Identity
- User-isolated data access
- Input validation on all endpoints
- File upload validation (type, size)
- Rate limiting
- CORS configuration
- HTTPS enforcement (production)
- SQL injection prevention (EF Core parameterization)

## Deployment

See [../docs/21-automated-deployment.md](../docs/21-automated-deployment.md) for:
- Azure Container Apps deployment
- Docker containerization
- CI/CD pipeline setup
- Environment configuration
- Production best practices

## Troubleshooting

### Database Connection Issues
```bash
# Check Docker container status
docker ps

# Check connection string in Aspire Dashboard
# Navigate to Resources > SQL Server
```

### OCR Not Working
```bash
# Verify OpenAI API key
dotnet user-secrets list

# Check OpenAI health
curl https://localhost:5001/health
```

### Email Not Sending
```bash
# Test SMTP connection
# Run ConfigureEmail.ps1 and choose "Test Connection"

# Check SMTP health
curl https://localhost:5001/health
```

### Migrations Failing
```bash
# Reset database (development only)
dotnet ef database drop
dotnet ef database update
```

## Contributing

1. Follow C# coding conventions
2. Add XML documentation for public APIs
3. Write unit tests for new features
4. Run tests before committing: `dotnet test`
5. Update documentation for API changes

## Documentation

### API-Specific Docs
- **[Authentication](../docs/04-authentication-authorization.md)** - JWT setup and configuration
- **[Receipt Upload](../docs/08-receipt-upload-feature.md)** - File upload implementation
- **[OCR Integration](../docs/09-ocr-openai-integration.md)** - OpenAI OCR setup
- **[Warranty Notifications](../docs/10-warranty-expiration-notifications.md)** - Background service
- **[Email/SMS](../docs/11-email-sms-notifications.md)** - Notification setup
- **[User Profile](../docs/12-user-profile-management.md)** - Profile API
- **[Testing Strategy](../docs/20-testing-strategy.md)** - Comprehensive testing approach

### Feature Docs
- **[PDF OCR](../docs/13-pdf-ocr-support.md)** - PDF processing
- **[Phone Verification](../docs/14-phone-verification.md)** - SMS verification
- **[Batch OCR](../docs/15-batch-ocr-processing.md)** - Bulk processing
- **[Refresh Tokens](../docs/16-refresh-token-support.md)** - Token renewal
- **[2FA](../docs/17-two-factor-authentication.md)** - Two-factor auth
- **[Email Confirmation](../docs/18-email-confirmation.md)** - Email verification
- **[Monitoring](../docs/19-monitoring-and-alerting.md)** - Health checks
- **[Receipt Sharing](../docs/23-receipt-sharing.md)** - Sharing feature
- **[Chatbot](../docs/24-ai-chatbot-receipt-queries.md)** - AI chatbot
- **[Performance](../docs/25-performance-optimization.md)** - Optimization
- **[Caching](../docs/26-user-data-caching.md)** - Caching strategy

## License

This project is licensed under the MIT License.

## Support

For issues, questions, or contributions:
- Open an issue on [GitHub Issues](https://github.com/RobertsProductions/receipt-app/issues)
- Check [documentation](../docs/)
- Review Swagger UI for API reference

---

**Built with** ❤️ **using ASP.NET Core 8.0**  
**Status**: Production-Ready | **Tests**: 146 passing | **Coverage**: 85%+ | **Performance**: Optimized 🚀
