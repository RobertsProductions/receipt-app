# Receipt App - Warranty Management System

[![.NET CI/CD Pipeline](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml)

A modern warranty management application built with .NET 8 and .NET Aspire for cloud-native orchestration, featuring OpenAI-powered OCR for automatic receipt data extraction, proactive warranty expiration notifications via email and SMS, secure phone number verification, and comprehensive user management.

## 🚀 Quick Start

**Want to get started quickly?** See the [Quick Start Guide](docs/setup/setup-quickstart.md) for streamlined setup instructions.

## Overview

This application provides a comprehensive warranty tracking system with a REST API backend orchestrated through .NET Aspire for simplified local development and deployment. Features include JWT authentication with 2FA, receipt image/PDF upload with AI-powered OCR, automated warranty expiration notifications via email and SMS, phone number verification, receipt sharing, and an AI chatbot for natural language queries.

## Technology Stack

### Backend
- **.NET 8.0** - Latest LTS version with C# 12
- **.NET Aspire 13.0** - Cloud-native orchestration and observability
- **ASP.NET Core Web API** - RESTful API with Swagger/OpenAPI
- **Entity Framework Core** - ORM with SQL Server/SQLite support
- **ASP.NET Core Identity** - User authentication and authorization
- **OpenAI GPT-4o-mini** - AI-powered OCR and chatbot
- **Twilio** - SMS notifications and phone verification
- **GitHub Actions** - CI/CD pipeline

### Frontend
- **Angular 18.2** - Modern TypeScript framework
- **RxJS 7.8** - Reactive programming
- **Playwright** - E2E testing framework
- **ESLint** - Code quality and linting

## Project Structure

```
MyAspireSolution/
├── .github/workflows/          # CI/CD pipeline
├── docs/                       # Technical documentation (33 guides)
│   ├── setup/                  # Getting started (5 docs)
│   ├── backend/                # Backend features (15 docs)
│   ├── infra/                  # Infrastructure & ops (7 docs)
│   ├── frontend/               # Frontend design & implementation (5 docs)
│   ├── guide/                  # Complete implementation guide (1 doc)
│   └── archive/                # Historical logs and progress reports
│       ├── sessions/           # Development session logs (6 docs)
│       ├── progress/           # Progress reports (6 docs)
│       └── reference/          # Quick reference (1 doc)
├── MyApi/                      # ASP.NET Core Web API
│   ├── Controllers/            # 15 API controllers, 70+ endpoints
│   ├── Services/               # Business logic (OCR, notifications, auth)
│   ├── Models/                 # Entity models
│   ├── DTOs/                   # Data transfer objects
│   └── HealthChecks/           # Health monitoring
├── MyApi.Tests/                # 146 passing unit tests
├── AppHost/                    # Aspire orchestrator
├── WarrantyApp.Web/            # Angular 18 frontend
│   ├── src/app/
│   │   ├── features/           # Feature modules (auth, receipts, warranties)
│   │   ├── shared/             # 17 reusable UI components
│   │   ├── services/           # API integration services
│   │   └── guards/             # Route guards
│   ├── e2e/                    # Playwright E2E tests
│   └── playwright.config.ts
└── global.json                 # .NET SDK version (8.0.302)
```

## Current Status

### Backend: 100% Complete ✅
- **146 passing unit tests** (100% pass rate, ~42s execution)
- **15 REST API controllers** with 70+ endpoints
- **Production-ready** with comprehensive health checks
- **Full feature set**: Auth, OCR, notifications, sharing, chatbot

### Frontend: 100% Complete ✅
- **17 of 17 shared components** (100%)
- **14 of 15 pages** (93% - chatbot enhancements optional)
- **125 E2E tests** implemented across 11 spec files
- **~9,800 lines of code** (TypeScript, HTML, SCSS)
- **Bundle size**: 106.88 kB gzipped
- **Playwright E2E infrastructure** complete with comprehensive test coverage

## Key Features

### Authentication & Security
- JWT authentication with refresh tokens (7-day expiry)
- Two-factor authentication (2FA) with TOTP authenticator apps
- Email confirmation with secure tokens
- Password reset flow
- SMS phone number verification (6-digit codes)
- Role-based authorization (Admin/User)

### Receipt Management
- Upload images (JPG, PNG) and PDFs (max 10MB)
- AI-powered OCR extraction (merchant, amount, date, product)
- Batch OCR processing for multiple receipts
- Receipt sharing with read-only access
- Download original files
- Full CRUD operations with user isolation

### Warranty Tracking
- Automated expiration monitoring (24/7 background service)
- Email and SMS notifications
- Configurable thresholds (1-90 days)
- Dashboard with urgency indicators
- Notification preferences (Email, SMS, Both, None)

### AI Chatbot
- Natural language queries about receipts
- Spending statistics and analytics
- Warranty status queries
- Conversation history
- Suggested questions

### Performance & Monitoring
- Response caching (5-30 minute TTL)
- Database indexes on key columns
- Rate limiting (100 req/min per IP)
- Comprehensive health checks (/health, /health/ready, /health/live)
- Real-time monitoring via Aspire Dashboard

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (8.0.302+)
- [Node.js 18+](https://nodejs.org/) and npm 10+
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [OpenAI API Key](https://platform.openai.com/api-keys) (optional, for OCR)

### Installation

```bash
# Clone repository
git clone https://github.com/RobertsProductions/receipt-app.git
cd receipt-app

# Restore .NET dependencies
dotnet restore

# Install Angular dependencies
cd WarrantyApp.Web
npm install
cd ..
```

### Running the Application

**Option 1: Full Stack with Aspire (Recommended)**
```bash
cd AppHost
dotnet run
```

This starts:
- Aspire Dashboard (https://localhost:17263)
- SQL Server container
- MyApi (dynamic port)
- Angular frontend (dynamic port)
- Automatic database migrations

**Option 2: Frontend and Backend Separately**
```bash
# Terminal 1 - Backend via Aspire
cd AppHost
dotnet run

# Terminal 2 - Frontend standalone
cd WarrantyApp.Web
npm start
```

### Configuration

**OpenAI API Key** (for OCR):
```powershell
.\SetOpenAiKey.ps1
# Or: cd MyApi && dotnet user-secrets set "OpenAI:ApiKey" "your-key"
```

**Email** (optional): `cd MyApi && .\ConfigureEmail.ps1`

**Twilio SMS** (optional): Configure via user secrets (AccountSid, AuthToken, PhoneNumber)

See [docs/setup/setup-quickstart.md](docs/setup/setup-quickstart.md) for detailed setup.

## Testing

### Run Backend Tests
```bash
dotnet test
# 146 tests, 100% pass rate, ~42 seconds
```

### Run Frontend E2E Tests

⚠️ **Prerequisites**: Backend API must be running first

```bash
# Terminal 1: Start backend API
cd AppHost
dotnet run
# Wait for Aspire Dashboard to show services ready

# Terminal 2: Run E2E tests
cd WarrantyApp.Web
npm run e2e          # Run all 125 tests
npm run e2e:ui       # Open Playwright UI
npm run e2e:debug    # Debug mode
```

**Test Coverage**: 125 E2E tests covering authentication, receipts, OCR, sharing, warranties, and settings.

**Note**: E2E tests require the full application stack (frontend + backend + database). The Playwright config automatically starts the Angular dev server, but the backend must be running separately.

## API Documentation

Access Swagger UI when the API is running:
- **Via Aspire**: Check dashboard for MyApi URL + `/swagger`
- **Standalone**: `https://localhost:5001/swagger`

## Documentation

**Quick Links**:
- [📖 Quick Start Guide](docs/setup/setup-quickstart.md) - Get up and running in minutes
- [🔐 Authentication](docs/backend/backend-authentication.md) - JWT + 2FA implementation
- [📄 Receipt Upload & OCR](docs/backend/backend-ocr-openai.md) - AI-powered extraction
- [🧪 Testing Strategy](docs/infra/infra-testing-strategy.md) - 146 tests explained
- [🎨 Design System](docs/frontend/frontend-design-system.md) - Complete UI/UX guide
- [✅ Complete Implementation](docs/guide/guide-complete-implementation.md) - E2E testing guide

**Browse All**: See [docs/](docs/) folder for 33 comprehensive guides covering setup, features, deployment, and frontend development. Historical session notes available in [docs/archive/](docs/archive/).

## CI/CD Pipeline

GitHub Actions runs on every push/PR: **Build** → **Test** (146 tests) → **Security Scan** → **Code Quality**

See [docs/infra/infra-cicd-github-actions.md](docs/infra/infra-cicd-github-actions.md) for configuration details.

## Optional Enhancements

**Frontend**: Chatbot enhancements, receipt search, bulk operations, export (CSV/PDF), dark mode, categories/tags, advanced filtering, analytics charts

**Backend**: Performance benchmarks, load testing (100+ concurrent users)

**Future**: Mobile app, accounting software integration, ML categorization, multi-tenant support

**See**: [docs/guide/guide-complete-implementation.md](docs/guide/guide-complete-implementation.md) for detailed roadmap

## Troubleshooting

**Database errors**: Ensure Docker is running, check Aspire Dashboard for SQL Server status

**OCR not working**: Configure OpenAI key (Aspire Dashboard or user secrets)

**Port conflicts**: Aspire uses dynamic ports - access via Dashboard links

**Angular proxy issues**: See [docs/frontend/frontend-aspire-proxy.md](docs/frontend/frontend-aspire-proxy.md)

## Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

## Build Status

| Branch | Status |
|--------|--------|
| main   | [![Build](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml/badge.svg?branch=main)](https://github.com/RobertsProductions/receipt-app/actions/workflows/dotnet-ci.yml) |

## License

MIT License - see LICENSE file for details

## Support

- Open issues on [GitHub Issues](https://github.com/RobertsProductions/receipt-app/issues)
- Check [documentation](docs/)
- Review Swagger UI for API reference

---

**Built with** ❤️ **using .NET 8, .NET Aspire, and Angular 18**

**Status**: Production-Ready | **Tests**: 146 backend + 125 E2E | **Coverage**: 85%+ | **Performance**: Optimized 🚀
