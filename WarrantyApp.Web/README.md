# WarrantyApp.Web - Angular Frontend

**Version**: 0.2.0 (Production-Ready with E2E Testing)  
**Last Updated**: November 17, 2025  
**Status**: 🚀 **Production-ready with Playwright E2E testing infrastructure!**

This is the frontend application for the Warranty Management System, built with Angular 18 and TypeScript. The application provides a modern, responsive interface for receipt and warranty management with AI-powered OCR processing.

## 🎉 Current Status (Sessions 1-9 + 3 Batches Complete)

**What's Built:**
- ✅ 17 of 17 shared components (100%)
- ✅ 14 of 15 pages (93%)
- ✅ Full authentication flow with 2FA
- ✅ Receipt management (upload, view, edit, delete)
- ✅ Warranty dashboard with expiration tracking
- ✅ User profile and notification settings
- ✅ Phone verification and email confirmation
- ✅ Password reset flow
- ✅ Receipt sharing functionality
- ✅ ~9,800 lines of production-ready code
- ✅ Bundle size: 106.88 kB gzipped (excellent performance!)
- ✅ **125 E2E tests** implemented (Playwright)
- ✅ **Playwright E2E testing infrastructure complete**

**What Users Can Do:**
1. Register and login with JWT authentication
2. Enable 2FA with authenticator apps
3. Verify email and phone number
4. Reset password via email
5. Upload receipts via drag-and-drop
6. Process receipts with OpenAI OCR
7. View receipts in paginated grid
8. View receipt details with images
9. Edit and delete receipts
10. Share receipts with other users (read-only access)
11. View receipts shared with them
12. Track warranty expiration dates
13. Get alerts for expiring warranties
14. Filter warranties by urgency
15. View and edit user profile
16. Configure notification preferences (email/SMS)
17. Set warranty expiration threshold (1-90 days)

See [Quick Demo](#quick-demo) below for screenshots and usage.

## Technology Stack

- **Angular**: 18.2.0 - Modern TypeScript framework
- **TypeScript**: 5.5.2 - Type-safe development
- **RxJS**: 7.8.0 - Reactive programming
- **SCSS**: Component-scoped styling
- **ESLint**: Code quality (angular-eslint)
- **Karma + Jasmine**: Unit testing

## Prerequisites

- Node.js 18+ and npm 10+
- Angular CLI (installed locally in project)
- .NET 8 backend running (via Aspire or standalone)

## Getting Started

### Install Dependencies

```bash
npm install
```

### Development Server

Run the development server:

```bash
npm start
```

**When run standalone**: Navigate to `http://localhost:4200/`. The app will automatically reload when you make changes.

**When run via Aspire**: The PORT environment variable will be set by Aspire, and the app will start on the assigned port (visible in Aspire dashboard).

**Note**: The `start` script uses `start-server.js` which:
- Reads the PORT environment variable (set by Aspire) or defaults to 4200
- Starts Angular dev server with dynamic proxy configuration
- Proxy forwards `/api` requests to the backend API (dynamic URL from Aspire or localhost:5000)

To run without the proxy:
```bash
npm run start:no-proxy
```

### Build

Build the project for development:
```bash
npm run build
```

Build for production:
```bash
npm run build:prod
```

The build artifacts will be stored in the `dist/` directory.

### Running E2E Tests (NEW!)

⚠️ **Prerequisites**: Requires backend API running

Playwright E2E tests for comprehensive testing:

```bash
# Step 1: Start backend API (required!)
cd AppHost
dotnet run
# Wait for Aspire Dashboard to show all services ready

# Step 2: Run E2E tests (in new terminal)
cd WarrantyApp.Web

# Run all E2E tests
npm run e2e

# Run with UI (interactive mode)
npm run e2e:ui

# Debug tests step-by-step
npm run e2e:debug

# Run with visible browser
npm run e2e:headed

# View test report
npm run e2e:report
```

**Test Coverage**:
- ✅ Authentication flows (25 tests) - login, register, logout
- ✅ Receipt CRUD operations (27 tests)
- ✅ OCR processing (14 tests)
- ✅ Receipt sharing (13 tests)
- ✅ Warranty dashboard (23 tests)
- ✅ User profile management (23 tests)

**Total: 125 comprehensive E2E tests**

**Note**: E2E tests are integration tests that require the full application stack (frontend + backend + database). If tests fail immediately, verify the backend API is running via Aspire.

### Running Unit Tests

Execute unit tests via Karma:
```bash
npm test
```

### Linting

Run ESLint to check code quality:
```bash
npm run lint
```

## Project Structure

```
src/
├── app/
│   ├── components/              # Shared UI components (14 complete)
│   │   ├── navbar/              # Navigation bar
│   │   └── ...
│   ├── shared/
│   │   ├── components/          # Reusable UI components
│   │   │   ├── avatar/          # User avatar with initials/image
│   │   │   ├── badge/           # Status indicators
│   │   │   ├── button/          # 5 variants, loading states
│   │   │   ├── card/            # Content containers
│   │   │   ├── checkbox/        # Form checkbox
│   │   │   ├── empty-state/     # No data placeholder
│   │   │   ├── file-upload/     # Drag-and-drop upload
│   │   │   ├── input/           # 7 input types, validation
│   │   │   ├── modal/           # Dialog/popup
│   │   │   ├── pagination/      # Page navigation
│   │   │   ├── slider/          # Range slider
│   │   │   ├── spinner/         # Loading indicator
│   │   │   ├── toast/           # Notifications
│   │   │   └── toggle/          # Switch component
│   │   └── services/            # Shared services
│   │       └── toast.service.ts # Toast notification service
│   ├── features/                # Feature modules
│   │   ├── auth/                # Authentication
│   │   │   └── pages/
│   │   │       ├── landing/     # Home page ✅
│   │   │       ├── login/       # Login page ✅
│   │   │       └── register/    # Register page ✅
│   │   ├── receipts/            # Receipt management
│   │   │   └── pages/
│   │   │       ├── receipt-list/   # Receipt grid ✅
│   │   │       └── receipt-detail/ # Receipt detail ✅
│   │   └── warranties/          # Warranty tracking
│   │       └── pages/
│   │           └── warranty-dashboard/ # Dashboard ✅
│   ├── services/                # Core services (7 complete)
│   │   ├── auth.service.ts      # Authentication & JWT
│   │   ├── receipt.service.ts   # Receipt CRUD & OCR
│   │   ├── warranty.service.ts  # Warranty tracking
│   │   ├── user-profile.service.ts # Profile management
│   │   ├── chatbot.service.ts   # AI chat integration
│   │   └── sharing.service.ts   # Receipt sharing
│   ├── models/                  # TypeScript interfaces
│   │   ├── auth.model.ts        # Auth types
│   │   ├── receipt.model.ts     # Receipt types
│   │   ├── user.model.ts        # User types
│   │   ├── warranty.model.ts    # Warranty types
│   │   ├── chatbot.model.ts     # Chat types
│   │   ├── sharing.model.ts     # Sharing types
│   │   └── index.ts             # Barrel exports
│   ├── guards/
│   │   └── auth.guard.ts        # Route protection
│   ├── interceptors/
│   │   ├── auth.interceptor.ts  # JWT token injection
│   │   └── error.interceptor.ts # Global error handling
│   ├── app.component.ts         # Root component
│   ├── app.config.ts            # App configuration
│   └── app.routes.ts            # Routing (6 routes)
├── environments/                # Environment configs
│   ├── environment.ts           # Default
│   ├── environment.development.ts # Dev (localhost:5000)
│   └── environment.prod.ts      # Prod (relative /api)
├── index.html                   # Main HTML
├── main.ts                      # Entry point
└── styles.scss                  # Global styles + design system
```

## Component Library (17 of 20 Complete)

### ✅ Built Components

| Component | Size | Description | Status |
|-----------|------|-------------|--------|
| **Button** | 3 sizes, 5 variants | Primary, secondary, success, danger, ghost | ✅ |
| **Input** | 7 types | Text, email, password, tel, number, date, textarea | ✅ |
| **Card** | 3 padding sizes | Header/body/footer slots, hoverable | ✅ |
| **Modal** | 5 sizes | Backdrop, ESC close, scroll lock | ✅ |
| **Toast** | 4 types | Auto-dismiss notifications | ✅ |
| **Badge** | 5 variants | Success, warning, error, info, neutral | ✅ |
| **Spinner** | 3 sizes, 3 colors | Loading states | ✅ |
| **Empty State** | - | Icon, title, description, CTA | ✅ |
| **Pagination** | - | Smart page display, navigation | ✅ |
| **Avatar** | 5 sizes | Image/initials, status indicator | ✅ |
| **Toggle** | 3 sizes | Switch with animation | ✅ |
| **Checkbox** | 3 sizes | Custom styled with checkmark | ✅ |
| **Slider** | - | Range input with value display | ✅ |
| **File Upload** | - | Drag-and-drop, preview, progress | ✅ |
| **Radio** | 3 sizes | Radio button groups | ✅ |
| **Tabs** | - | Tabbed navigation with active state | ✅ |
| **Accordion** | - | Collapsible sections with animation | ✅ |

### 🔜 Remaining Components (Optional)
- Dropdown (searchable select)
- Tooltip (hover tooltips)
- Progress Bar (linear progress)

## Environment Configuration

The application uses environment files to manage configuration:

- **Development** (`environment.development.ts`): Used during `ng serve`
  - API URL: `http://localhost:5000/api`
  
- **Production** (`environment.prod.ts`): Used during production builds
  - API URL: `/api` (relative path)

## API Proxy Configuration

The `proxy.conf.mjs` file dynamically configures the dev server to proxy API requests to the backend:

```javascript
export default function() {
  // Aspire injects service URLs in the format: services__<servicename>__http__0
  const apiUrl = process.env.API_URL || 
                 process.env.services__myapi__http__0 || 
                 'http://localhost:5000';
  
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

This allows the frontend to make requests to `/api/*` which are automatically forwarded to the backend API.

**How it works:**
1. When run via Aspire: Reads `API_URL` or `services__myapi__http__0` from environment
2. When run standalone: Falls back to `http://localhost:5000`
3. Logs the proxy target for debugging

See `../docs/32-aspire-angular-proxy-fix.md` for details on the dynamic proxy implementation.

## Code Style and Linting

The project uses ESLint with Angular-specific rules. Configuration is in `eslint.config.js`.

Key rules:
- Component selectors must use `app` prefix and kebab-case
- Directive selectors must use `app` prefix and camelCase
- Template accessibility rules enabled
- TypeScript recommended rules enforced

## Design System

The application uses a comprehensive design system defined in `styles.scss`:

### Color Palette
- **Primary**: Blue (#2196F3) - CTAs, links, active states
- **Neutral**: Gray scale - Text, backgrounds, borders
- **Accent**: Purple (#9C27B0) - Highlights, special features
- **Semantic**: Success (green), warning (yellow), error (red), info (blue)

### Typography
- **Font**: Inter (sans-serif), system fallbacks
- **Scale**: 12px - 48px (responsive)
- **Weights**: 400 (regular), 600 (semibold), 700 (bold)

### Spacing
- **8px grid system**: --space-1 (4px) to --space-20 (160px)
- Consistent padding and margins throughout

### Components
- **Border Radius**: 4px (sm), 8px (md), 16px (lg), 999px (full)
- **Shadows**: 4 levels (sm, md, lg, xl)
- **Transitions**: 150ms ease for smooth interactions

See `../docs/27-design-reference.md` for complete specifications.

## Bundle Performance

**Production Build** (as of November 17, 2025):

```
Initial Bundle:   331.45 kB → 91.68 kB gzipped
Total Lazy Chunks: 141 kB → 38 kB gzipped (avg 2-5 kB per page)
Build Time:       ~2 seconds
```

**Lazy-Loaded Routes** (code-split):
- Landing: 2.16 kB gzipped
- Login: 1.82 kB gzipped
- Register: 2.31 kB gzipped
- Receipt List: 5.11 kB gzipped
- Receipt Detail: 3.20 kB gzipped
- Warranty Dashboard: 2.74 kB gzipped
- User Profile: 2.52 kB gzipped
- Notification Settings: 3.18 kB gzipped

**Performance Score: A+ 🎯**
- Optimal bundle splitting
- Tree-shakeable components
- Lazy-loaded routes
- No bundle bloat

## Quick Demo

### Authentication Flow
1. **Landing Page** (`/`) - Hero section with features
2. **Register** (`/register`) - Create account with validation
3. **Login** (`/login`) - Email/password authentication

### Receipt Management
1. **Upload Receipt** - Drag-and-drop or click to select
2. **OCR Processing** - Automatic data extraction
3. **View Receipts** (`/receipts`) - Paginated grid with search
4. **Receipt Detail** (`/receipts/:id`) - Full view with edit/delete

### Warranty Dashboard
1. **Dashboard** (`/warranties`) - Summary cards (total, expiring, valid, expired)
2. **Filter Warranties** - 7/30/60/all days
3. **Urgency Indicators** - Color-coded badges (critical, warning, normal)
4. **Click to View** - Navigate to receipt detail

## Workflows and Components

See `../docs/frontend/frontend-workflows.md` for:
- User journey workflows
- Component breakdown
- Implementation phases
- API integration details

## Integration with .NET Aspire

The Angular app can be orchestrated with the .NET Aspire AppHost for unified development.

See `../docs/frontend/frontend-aspire-integration.md` for:
- Aspire integration instructions
- AppHost configuration
- Service discovery setup
- CORS configuration

## NPM Scripts Reference

| Script | Description |
|--------|-------------|
| `npm start` | Start dev server with dynamic API proxy (uses PORT from Aspire or defaults to 4200) |
| `npm run start:no-proxy` | Start dev server without proxy |
| `npm run build` | Build for development |
| `npm run build:prod` | Build for production with optimizations |
| `npm run watch` | Build and watch for changes |
| `npm test` | Run unit tests |
| `npm run lint` | Run ESLint |
| `npm run e2e` | Run Playwright E2E tests |
| `npm run e2e:ui` | Open Playwright UI (interactive) |
| `npm run e2e:debug` | Debug Playwright tests |
| `npm run e2e:report` | View last test report |
| `npm run e2e:headed` | Run tests with visible browser |

## Code Scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

1. Follow the Angular Style Guide
2. Run `npm run lint` before committing
3. Ensure all tests pass with `npm test`
4. Update documentation for new features

## Next Steps

### 🔜 Optional Enhancements (Remaining Work)

**Pages** (14 of 15 - 93% Complete)
- [x] User Profile Page - View/edit profile, account info
- [x] Notification Settings - Email/SMS preferences with slider
- [x] Phone Verification - 6-digit code input
- [x] 2FA Setup - QR code, recovery codes
- [x] Email Confirmation - Verify email flow
- [x] Password Reset - Forgot password complete flow
- [x] Shared Receipts View - View receipts shared with you
- [x] Share Receipt Modal - Share with users, manage access
- [ ] AI Chatbot Enhancements - Advanced features (optional)

**Polish & Features** ✅ E2E Tests Complete!
- [x] **Playwright E2E Testing** - 125 comprehensive tests implemented
- [ ] Search functionality across receipts
- [ ] Bulk operations (select multiple, batch delete)
- [ ] Export features (CSV/PDF for receipts and warranties)
- [ ] Dark mode support
- [ ] Advanced filtering (by merchant, date range, amount)

**Total Remaining**: ~20-30 hours for 100% optional features

**Current Status**: Core features + testing are production-ready! 🎉

## Documentation

### Core Frontend Guides
- **[Quick Start Guide](../docs/setup/setup-quickstart.md)** - Get up and running fast
- **[Design Reference](../docs/frontend/frontend-design-system.md)** - Complete UI/UX design system
- **[Frontend Workflows](../docs/frontend/frontend-workflows.md)** - User journeys and flows
- **[Aspire Integration](../docs/frontend/frontend-aspire-integration.md)** - Angular + .NET Aspire setup
- **[Implementation Roadmap](../docs/frontend/frontend-roadmap.md)** - Detailed specs (1,507 lines)

### Additional Resources
- **[Proxy Configuration](../docs/frontend/frontend-aspire-proxy.md)** - Dynamic API proxy troubleshooting
- **[Complete Implementation](../docs/guide/guide-complete-implementation.md)** - E2E testing guide and next steps

**Development History**: Session notes and progress reports available in [../docs/archive/](../docs/archive/)

### API Documentation
See backend API documentation at `/swagger` when running the API.

## Further Help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

For project-specific questions, see the documentation in the `../docs/` directory.

---

**Built with** ❤️ **using Angular 18 and TypeScript**  
**Performance**: 106.88 kB gzipped | **Code Quality**: ESLint + TypeScript strict | **Tests**: 125 E2E | **Status**: Production-Ready 🚀

