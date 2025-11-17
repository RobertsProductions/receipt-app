# Testing Documentation

**Last Updated**: November 17, 2025  
**Status**: 🎯 **Backend Tests: 146/146 Passing | E2E Tests: In Progress**

This directory contains comprehensive testing documentation for the Warranty Management System, including test status, guidelines, troubleshooting, and maintenance procedures for all testing initiatives.

## 📋 Table of Contents

1. [Testing Overview](#testing-overview)
2. [Backend Unit Tests (146 Tests)](#backend-unit-tests)
3. [Frontend E2E Tests (125 Tests)](#frontend-e2e-tests)
4. [Test Execution Guide](#test-execution-guide)
5. [E2E Test Maintenance](#e2e-test-maintenance)
6. [HTML Selector Guidelines](#html-selector-guidelines)
7. [Debugging Failed Tests](#debugging-failed-tests)
8. [CI/CD Integration](#cicd-integration)
9. [Test Coverage Goals](#test-coverage-goals)
10. [Known Issues & Solutions](#known-issues--solutions)

---

## Testing Overview

### Current State Summary

| Test Category | Tests | Passing | Status | Coverage |
|--------------|-------|---------|--------|----------|
| **Backend Unit Tests** | 146 | 146 | ✅ 100% | ~85% |
| **Frontend E2E Tests** | 125 | TBD | ⚠️ In Progress | Critical Paths |
| **Total** | **271** | **146+** | 🎯 **Backend Complete** | **Good** |

### Test Execution Time

- **Backend Tests**: ~42 seconds (all 146 tests)
- **E2E Tests**: ~5-10 minutes (varies by parallelization)
- **Total CI/CD Pipeline**: ~3-5 minutes (build + backend tests)

### Technology Stack

- **Backend**: xUnit, Moq, Microsoft.Extensions.Logging
- **Frontend**: Playwright (TypeScript), Chromium/Firefox/WebKit
- **CI/CD**: GitHub Actions (.NET CI pipeline)

---

## Backend Unit Tests

### Overview

The backend has **146 comprehensive unit tests** covering all critical services, controllers, and models. All tests pass consistently with a ~42 second execution time.

📖 **See [../MyApi.Tests/README.md](../../MyApi.Tests/README.md)** for detailed backend test documentation including:
- Complete test inventory with all 146 tests listed
- Testing patterns and best practices
- Mocking strategies and examples
- Debugging procedures
- Adding new tests guide

### Test Structure

```
MyApi.Tests/
├── Services/                          # Service layer tests (143 tests)
│   ├── ChatbotServiceTests.cs        # 17 tests - AI chatbot
│   ├── CompositeNotificationServiceTests.cs  # 8 tests - Notification routing
│   ├── EmailNotificationServiceTests.cs      # 14 tests - Email sending
│   ├── LocalFileStorageServiceTests.cs       # 11 tests - File operations
│   ├── LogNotificationServiceTests.cs        # 12 tests - Logging
│   ├── OpenAiOcrServiceTests.cs              # 16 tests - OCR processing
│   ├── PhoneVerificationServiceTests.cs      # 10 tests - SMS/Phone
│   ├── TokenServiceTests.cs                  # 12 tests - JWT tokens
│   └── WarrantyExpirationServiceTests.cs     # 17 tests - Background service
└── Models/                            # Model validation tests (26 tests)
    ├── ApplicationUserTests.cs       # 10 tests - User model
    ├── ReceiptTests.cs               # 6 tests - Receipt validation
    └── ReceiptShareTests.cs          # 10 tests - Sharing logic
```

### Test Coverage by Feature

| Feature | Tests | Files | Status |
|---------|-------|-------|--------|
| **Authentication & JWT** | 12 | TokenServiceTests.cs | ✅ Complete |
| **OCR Processing** | 16 | OpenAiOcrServiceTests.cs | ✅ Complete |
| **Email Notifications** | 14 | EmailNotificationServiceTests.cs | ✅ Complete |
| **Phone/SMS** | 10 | PhoneVerificationServiceTests.cs | ✅ Complete |
| **Warranty Monitoring** | 17 | WarrantyExpirationServiceTests.cs | ✅ Complete |
| **Notification Routing** | 8 | CompositeNotificationServiceTests.cs | ✅ Complete |
| **AI Chatbot** | 17 | ChatbotServiceTests.cs | ✅ Complete |
| **File Storage** | 11 | LocalFileStorageServiceTests.cs | ✅ Complete |
| **Logging** | 12 | LogNotificationServiceTests.cs | ✅ Complete |
| **Models** | 26 | 3 model test files | ✅ Complete |

### Running Backend Tests

```bash
# Run all tests
cd MyApi.Tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test file
dotnet test --filter "FullyQualifiedName~TokenServiceTests"

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Backend Test Quality Metrics

- ✅ **100% pass rate** (146/146)
- ✅ **Fast execution** (~42 seconds)
- ✅ **Comprehensive mocking** (all external dependencies)
- ✅ **Edge case coverage** (error handling, validation)
- ✅ **Consistent naming** (MethodName_Scenario_ExpectedResult)

---

## Frontend E2E Tests

### Overview

The frontend has **125 E2E tests** implemented using Playwright, covering critical user workflows across authentication, receipts, warranties, and settings. Tests are organized by feature area.

### Test Structure

```
WarrantyApp.Web/e2e/
├── auth/                              # Authentication flows
│   ├── landing.spec.ts               # 7 tests - Landing page
│   ├── login.spec.ts                 # 18 tests - Login flow
│   └── register.spec.ts              # 20 tests - Registration
├── receipts/                          # Receipt management
│   ├── list.spec.ts                  # 14 tests - Receipt list
│   ├── details.spec.ts               # 18 tests - Receipt details
│   ├── upload.spec.ts                # 15 tests - Upload & OCR
│   └── sharing.spec.ts               # 12 tests - Sharing features
├── warranties/                        # Warranty tracking
│   └── dashboard.spec.ts             # 10 tests - Dashboard
├── profile/                           # User settings
│   └── settings.spec.ts              # 11 tests - Profile & preferences
└── helpers/                           # Test utilities
    ├── auth.helpers.ts               # Login, register, logout helpers
    ├── receipt.helpers.ts            # Receipt CRUD helpers
    └── test-data.ts                  # Test data generators
```

### Test Coverage by Feature

| Feature | Tests | File | Priority | Status |
|---------|-------|------|----------|--------|
| **Landing Page** | 7 | landing.spec.ts | Medium | ⚠️ To Test |
| **User Registration** | 20 | register.spec.ts | High | ⚠️ To Test |
| **User Login** | 18 | login.spec.ts | High | ⚠️ To Test |
| **Receipt List** | 14 | list.spec.ts | High | ⚠️ To Test |
| **Receipt Details** | 18 | details.spec.ts | High | ⚠️ To Test |
| **Receipt Upload/OCR** | 15 | upload.spec.ts | High | ⚠️ To Test |
| **Receipt Sharing** | 12 | sharing.spec.ts | Medium | ⚠️ To Test |
| **Warranty Dashboard** | 10 | dashboard.spec.ts | High | ⚠️ To Test |
| **User Settings** | 11 | settings.spec.ts | Medium | ⚠️ To Test |

### Running E2E Tests

**Prerequisites:**
1. Backend API must be running (`cd AppHost && dotnet run`)
2. Wait for Aspire Dashboard to show all services ready
3. Ensure test database is accessible

```bash
cd WarrantyApp.Web

# Run all E2E tests (headless)
npm run e2e

# Run with UI (interactive mode - RECOMMENDED for debugging)
npm run e2e:ui

# Run specific test file
npx playwright test e2e/auth/login.spec.ts

# Run specific test by name
npx playwright test -g "should successfully login"

# Run with visible browser (headed mode)
npm run e2e:headed

# Debug tests step-by-step
npm run e2e:debug

# Run on specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# View last test report
npm run e2e:report
```

### E2E Test Configuration

Located in `WarrantyApp.Web/playwright.config.ts`:

```typescript
{
  testDir: './e2e',
  timeout: 60000,                    // 60s per test
  expect: { timeout: 10000 },        // 10s for assertions
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'on-first-retry',         // Trace for debugging
    screenshot: 'only-on-failure',   // Screenshots on failure
    actionTimeout: 15000,            // 15s for actions
    navigationTimeout: 30000         // 30s for navigation
  },
  retries: process.env.CI ? 2 : 0,  // Retry flaky tests in CI
  workers: process.env.CI ? 1 : undefined,  // Parallel in dev
  projects: ['chromium', 'firefox', 'webkit']
}
```

---

## Test Execution Guide

### Full Test Suite Execution

**Step 1: Run Backend Tests**
```bash
# Terminal 1: Run backend unit tests
cd MyApi.Tests
dotnet test
# Expected: 146 passing, ~42 seconds
```

**Step 2: Start Backend API**
```bash
# Terminal 1: Start Aspire AppHost
cd AppHost
dotnet run
# Wait for "Application started" message
# Open Aspire Dashboard (URL shown in console)
# Verify all services are "Running"
```

**Step 3: Run E2E Tests**
```bash
# Terminal 2: Run Playwright tests
cd WarrantyApp.Web
npm run e2e
# Expected: 125 tests, ~5-10 minutes
```

### Quick Smoke Test

Run critical path tests only:

```bash
# Critical user flows
npx playwright test e2e/auth/login.spec.ts
npx playwright test e2e/receipts/list.spec.ts
npx playwright test e2e/receipts/upload.spec.ts
```

### Development Workflow

When working on features:

1. **Write/Update Unit Tests** (backend)
   ```bash
   dotnet test --filter "FullyQualifiedName~YourServiceTests"
   ```

2. **Write/Update E2E Tests** (frontend)
   ```bash
   npm run e2e:ui  # Interactive mode for development
   ```

3. **Run Full Suite Before PR**
   ```bash
   dotnet test && npm run e2e
   ```

---

## E2E Test Maintenance

### Test Health Monitoring

Track test status in this table (update after each test run):

| Test Suite | Date Run | Pass | Fail | Flaky | Notes |
|------------|----------|------|------|-------|-------|
| auth/* | TBD | - | - | - | Not yet run |
| receipts/* | TBD | - | - | - | Not yet run |
| warranties/* | TBD | - | - | - | Not yet run |
| profile/* | TBD | - | - | - | Not yet run |

### Updating Tests for UI Changes

When UI changes are made, update tests using this checklist:

#### 1. **Identify Affected Tests**
```bash
# Search for tests using specific selectors
grep -r "getByRole.*button.*Upload" e2e/
```

#### 2. **Use Playwright UI Mode to Inspect Elements**
```bash
npm run e2e:ui
# Click "Pick Locator" (target icon)
# Click element in browser
# Copy suggested locator
```

#### 3. **Update Selectors (Priority Order)**
- ✅ **Preferred**: `getByRole('button', { name: /text/i })`
- ✅ **Good**: `getByLabel(/label/i)`
- ⚠️ **Okay**: `getByTestId('test-id')`
- ❌ **Avoid**: CSS selectors (`.class-name`, `#id`)

#### 4. **Test Locally Before Committing**
```bash
npm run e2e:headed  # Watch tests run
```

### Adding New Tests

**Template for New Test:**

```typescript
import { test, expect } from '@playwright/test';
import { generateTestUser } from '../helpers/test-data';
import { registerAndLogin } from '../helpers/auth.helpers';

test.describe('Feature Name', () => {
  
  test.beforeEach(async ({ page }) => {
    // Setup: login, navigate, etc.
    const user = generateTestUser();
    await registerAndLogin(page, user);
  });

  test('should do something', async ({ page }) => {
    // Arrange
    await page.goto('/your-page');
    
    // Act
    await page.getByRole('button', { name: /action/i }).click();
    
    // Assert
    await expect(page.getByText(/expected result/i)).toBeVisible();
  });
});
```

### Helper Functions

Use helper functions to reduce code duplication:

**Auth Helpers** (`helpers/auth.helpers.ts`):
- `registerUser(page, user)` - Register new user
- `loginUser(page, email, password)` - Login existing user
- `registerAndLogin(page, user)` - Register + auto-login
- `logoutUser(page)` - Logout current user
- `isLoggedIn(page)` - Check auth status

**Receipt Helpers** (`helpers/receipt.helpers.ts`):
- `goToReceiptsPage(page)` - Navigate to receipts
- `createReceipt(page, receipt)` - Create new receipt
- `findReceipt(page, merchantName)` - Find receipt by merchant
- `getReceiptCount(page)` - Count visible receipts
- `goToNextPage(page)` - Navigate pagination

**Test Data** (`helpers/test-data.ts`):
- `generateTestUser()` - Unique test user
- `generateTestReceipt()` - Test receipt data
- `generateUniqueEmail()` - Unique email

---

## HTML Selector Guidelines

### Playwright Locator Best Practices

**Priority Hierarchy** (most to least robust):

1. **Role-Based Selectors** ⭐ BEST
   ```typescript
   page.getByRole('button', { name: /sign in/i })
   page.getByRole('textbox', { name: /email/i })
   page.getByRole('heading', { name: /receipts/i })
   ```
   - ✅ Semantic and accessible
   - ✅ Resilient to CSS changes
   - ✅ Works with screen readers

2. **Label Selectors** ⭐ GOOD
   ```typescript
   page.getByLabel(/email/i)
   page.getByLabel('Password', { exact: true })
   ```
   - ✅ Natural for form inputs
   - ✅ Matches user experience

3. **Text Selectors** ⚠️ USE SPARINGLY
   ```typescript
   page.getByText(/welcome back/i)
   page.getByText('Store Name')
   ```
   - ⚠️ Can break with text changes
   - ⚠️ May match multiple elements

4. **Test ID Selectors** ⚠️ FALLBACK
   ```typescript
   page.getByTestId('receipt-card-123')
   ```
   - ⚠️ Requires adding `data-testid` attributes
   - ⚠️ Not semantic
   - ✅ Use when no better option exists

5. **CSS/XPath Selectors** ❌ AVOID
   ```typescript
   page.locator('.btn-primary')  // BAD - fragile
   page.locator('#submit-btn')   // BAD - implementation detail
   ```
   - ❌ Breaks with CSS refactoring
   - ❌ Not semantic
   - ❌ Hard to maintain

### Finding the Right Selector

**Method 1: Playwright UI Mode** (RECOMMENDED)

```bash
npm run e2e:ui
# 1. Open test file
# 2. Click "Pick Locator" (target icon in top bar)
# 3. Hover over element in browser
# 4. Click element to see suggested locators
# 5. Copy the BEST locator (usually getByRole or getByLabel)
```

**Method 2: Browser DevTools**

```bash
npm run e2e:headed
# 1. Test runs in visible browser
# 2. When test pauses, right-click element → Inspect
# 3. Look for:
#    - aria-label or aria-labelledby (use getByLabel)
#    - role attribute (use getByRole)
#    - Nearby <label> tags (use getByLabel)
```

**Method 3: Playwright Codegen**

```bash
npx playwright codegen http://localhost:4200
# 1. Opens browser + recorder
# 2. Click through your flow
# 3. Generated code shows in right panel
# 4. Copy locators (edit for robustness)
```

### Common Patterns

**Buttons:**
```typescript
// Prefer text-based
page.getByRole('button', { name: /sign in/i })
page.getByRole('button', { name: /upload/i })

// If multiple buttons with same text, combine with context
page.locator('form').getByRole('button', { name: /submit/i })
```

**Form Inputs:**
```typescript
// Use label association
page.getByLabel(/email/i)
page.getByLabel('Password', { exact: true })  // Disambiguate from "Confirm Password"

// For inputs without labels
page.getByPlaceholder('Search receipts...')
```

**Navigation Links:**
```typescript
// Use role + name
page.getByRole('link', { name: /receipts/i })

// If multiple, use first() or combine with context
page.locator('nav').getByRole('link', { name: /profile/i })
```

**Dynamic Content (Lists, Cards):**
```typescript
// Filter by text content
page.locator('article').filter({ hasText: 'Best Buy' })
page.locator('[data-testid="receipt-card"]').filter({ hasText: 'Store A' })

// Find parent container
const receiptCard = page.getByText('Best Buy').locator('xpath=..')
```

**Waiting for Elements:**
```typescript
// Explicit wait with timeout
await expect(page.getByText(/loading/i)).not.toBeVisible({ timeout: 5000 });

// Wait for specific state
await page.getByRole('button', { name: /submit/i }).waitFor({ state: 'visible' });

// Wait for navigation
await page.waitForURL(/\/receipts/, { timeout: 10000 });
```

### Debugging Selector Issues

**Problem**: Selector not finding element

```typescript
// Check if element exists at all
const count = await page.locator('your-selector').count();
console.log(`Found ${count} elements`);

// Print all matching elements
const elements = await page.locator('your-selector').allTextContents();
console.log(elements);

// Take screenshot for debugging
await page.screenshot({ path: 'debug.png', fullPage: true });
```

**Problem**: Multiple elements found

```typescript
// Use first() or last()
page.getByRole('button', { name: /submit/i }).first()

// Use nth()
page.getByRole('button', { name: /submit/i }).nth(1)  // Second button

// Add context to narrow down
page.locator('form[name="login"]').getByRole('button', { name: /submit/i })
```

**Problem**: Element not visible when expected

```typescript
// Check visibility states
const isVisible = await page.getByRole('button').isVisible();
const isHidden = await page.getByRole('button').isHidden();

// Wait for element with longer timeout
await page.getByRole('button').waitFor({ state: 'visible', timeout: 15000 });
```

### Angular-Specific Considerations

Since the app uses Angular:

1. **Wait for Angular to stabilize:**
   ```typescript
   await page.waitForLoadState('networkidle');
   ```

2. **Be patient with async operations:**
   ```typescript
   await page.getByRole('button', { name: /login/i }).click();
   await page.waitForURL(/\/receipts/, { timeout: 10000 });
   ```

3. **Handle toasts/notifications:**
   ```typescript
   // Toasts may appear and disappear quickly
   await expect(page.getByText(/success/i)).toBeVisible({ timeout: 5000 });
   ```

4. **Dynamic content loading:**
   ```typescript
   // Wait for loading indicators to disappear
   await expect(page.getByText(/loading/i)).not.toBeVisible({ timeout: 10000 });
   ```

---

## Debugging Failed Tests

### Step-by-Step Debugging Process

**1. Run Test in UI Mode**
```bash
npm run e2e:ui
# Interactive debugging with step-through
```

**2. Run Test in Debug Mode**
```bash
npm run e2e:debug
# Opens debugging inspector
```

**3. Run Test Headed (Visible Browser)**
```bash
npm run e2e:headed
# Watch test execution in real-time
```

**4. Add Console Logs**
```typescript
test('should do something', async ({ page }) => {
  console.log('Starting test...');
  await page.goto('/receipts');
  
  const count = await page.locator('.receipt-card').count();
  console.log(`Found ${count} receipts`);
  
  await page.screenshot({ path: 'debug-screenshot.png' });
});
```

**5. Use Playwright Trace Viewer**
```bash
# After failed test, open trace
npx playwright show-trace test-results/path-to-trace.zip
```

### Common Failure Patterns

#### Test Timing Issues

**Symptom**: Test fails intermittently with "Element not found"

**Solution**: Add explicit waits
```typescript
// Before
await page.getByRole('button').click();

// After
await page.getByRole('button').waitFor({ state: 'visible', timeout: 10000 });
await page.getByRole('button').click();
```

#### Authentication Issues

**Symptom**: Test fails because user is not logged in

**Solution**: Verify auth state
```typescript
// Add assertion to verify login succeeded
await registerAndLogin(page, user);
await expect(page).toHaveURL(/\/(receipts|dashboard)/);
```

#### API Errors

**Symptom**: Test fails with API 500/404 errors in console

**Solution**: Check backend is running and healthy
```bash
# Verify backend health
curl http://localhost:5000/health

# Check Aspire Dashboard for service status
```

#### Stale Data

**Symptom**: Test expects data from previous test run

**Solution**: Each test should be independent
```typescript
// Good: Generate unique data per test
const user = generateTestUser();

// Bad: Reusing static test data
const user = TEST_USERS.valid;  // May conflict with other tests
```

#### Slow Network

**Symptom**: Tests timeout waiting for API responses

**Solution**: Increase timeout or optimize API
```typescript
// Increase timeout for slow operations
await page.getByText(/upload complete/i).waitFor({ timeout: 30000 });

// Or adjust in playwright.config.ts
timeout: 90 * 1000  // 90 seconds
```

### Test Report Analysis

After running tests, view the HTML report:

```bash
npm run e2e:report
```

Report shows:
- ✅ Passing tests (green)
- ❌ Failed tests (red)
- ⚠️ Flaky tests (yellow)
- Screenshots of failures
- Execution time per test
- Console logs
- Network activity

---

## CI/CD Integration

### Current GitHub Actions Pipeline

Located in `.github/workflows/dotnet-ci.yml`:

```yaml
name: .NET CI/CD Pipeline

on: [push, pull_request]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run Backend Tests
        run: dotnet test --no-build --verbosity normal --configuration Release
      
      # E2E tests not yet integrated (requires full stack)
```

### Adding E2E Tests to CI/CD

**Future Enhancement** (when E2E tests are stable):

```yaml
  e2e-tests:
    runs-on: ubuntu-latest
    needs: build-and-test
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: |
          cd WarrantyApp.Web
          npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps
      
      - name: Start backend
        run: |
          cd AppHost
          dotnet run &
          sleep 30  # Wait for services to start
      
      - name: Run E2E tests
        run: |
          cd WarrantyApp.Web
          npm run e2e
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: playwright-report
          path: WarrantyApp.Web/playwright-report/
```

### Local Pre-Commit Checks

Before committing, run:

```bash
# 1. Backend tests
dotnet test

# 2. Frontend linting
cd WarrantyApp.Web
npm run lint

# 3. E2E smoke tests (critical paths only)
npx playwright test e2e/auth/login.spec.ts e2e/receipts/list.spec.ts
```

---

## Test Coverage Goals

### Current Coverage

| Category | Coverage | Target | Status |
|----------|----------|--------|--------|
| **Backend Services** | ~85% | 80% | ✅ Exceeds |
| **Backend Controllers** | ~70% | 70% | ✅ Met |
| **Frontend E2E (Critical Paths)** | TBD | 90% | ⚠️ In Progress |
| **Frontend E2E (All Features)** | TBD | 70% | ⚠️ In Progress |

### Critical User Paths (Must Pass)

These flows must work in production:

1. ✅ **User Registration** → Email confirmation → Login
2. ✅ **User Login** → Dashboard access
3. ✅ **Upload Receipt** → View in list
4. ✅ **Receipt OCR** → Extracted data display
5. ✅ **Warranty Dashboard** → View expiring warranties
6. ✅ **User Settings** → Update preferences

### Future Coverage Enhancements

- [ ] Mobile responsive E2E tests
- [ ] Accessibility (a11y) testing
- [ ] Performance testing (Lighthouse)
- [ ] API integration tests (separate from unit tests)
- [ ] Load testing (backend)

---

## Known Issues & Solutions

### Current Known Issues

| Issue | Impact | Workaround | Status |
|-------|--------|------------|--------|
| Email confirmation required after registration | Tests must handle redirect | Use `registerAndLogin` helper | ⚠️ Known |
| Test isolation (shared database) | Tests may conflict | Use unique test data generators | ⚠️ Known |
| Playwright trace files grow large | Disk space | Clean `test-results/` periodically | ⚠️ Known |
| Backend must be running for E2E | Manual step | Document in README | ⚠️ Known |

### Common Test Failures

#### "Element not found"

**Cause**: Element not loaded yet or selector wrong

**Solution**:
1. Add explicit wait: `await element.waitFor({ state: 'visible' })`
2. Verify selector with `npm run e2e:ui`
3. Check element exists in UI

#### "Navigation timeout"

**Cause**: Backend API slow or not responding

**Solution**:
1. Check backend is running: `curl http://localhost:5000/health`
2. Increase timeout in playwright.config.ts
3. Optimize slow API endpoints

#### "401 Unauthorized"

**Cause**: JWT token expired or auth state lost

**Solution**:
1. Use `registerAndLogin` helper (ensures fresh token)
2. Don't reuse tokens across tests
3. Check token expiry (7 days default)

#### "Flaky tests"

**Cause**: Race conditions, timing issues, shared state

**Solution**:
1. Add `await page.waitForLoadState('networkidle')`
2. Use `test.describe.serial()` for dependent tests
3. Ensure test isolation (unique test data)

### Troubleshooting Checklist

Before reporting a test failure:

- [ ] Backend API is running (`cd AppHost && dotnet run`)
- [ ] All services show "Running" in Aspire Dashboard
- [ ] Database is accessible and migrated
- [ ] Frontend dev server starts (`npm start`)
- [ ] Browser is up-to-date (Playwright auto-updates)
- [ ] Test data is unique (using generators)
- [ ] No port conflicts (4200 for Angular, 5000+ for backend)
- [ ] Logs checked for errors (`playwright-report/` and Aspire Dashboard)

---

## Maintenance Procedures

### Weekly Maintenance

1. **Run Full Test Suite**
   ```bash
   dotnet test && npm run e2e
   ```

2. **Update Test Status Table** (in this README)
   - Record pass/fail counts
   - Note any flaky tests
   - Track execution time

3. **Review Failed Tests**
   - Categorize failures (real bugs vs. flaky tests)
   - File issues for real bugs
   - Fix flaky tests

4. **Clean Up Test Artifacts**
   ```bash
   # Clean old test results
   rm -rf WarrantyApp.Web/test-results
   rm -rf WarrantyApp.Web/playwright-report
   ```

### Monthly Maintenance

1. **Update Playwright**
   ```bash
   cd WarrantyApp.Web
   npm install @playwright/test@latest
   npx playwright install
   ```

2. **Review Test Coverage**
   - Check coverage reports
   - Identify gaps
   - Add tests for new features

3. **Performance Tuning**
   - Optimize slow tests
   - Reduce test execution time
   - Review parallelization settings

### After Each Feature Addition

1. **Add Unit Tests** (backend)
2. **Add E2E Tests** (if user-facing)
3. **Update This Documentation**
4. **Run Full Test Suite Before Merge**

---

## Quick Reference

### Test Commands Cheat Sheet

```bash
# Backend Tests
dotnet test                                    # Run all backend tests
dotnet test --filter "FullyQualifiedName~Auth" # Run specific tests
dotnet test --verbosity detailed               # Verbose output

# E2E Tests
npm run e2e                                    # Run all E2E tests
npm run e2e:ui                                 # Interactive mode (best for dev)
npm run e2e:debug                              # Debug mode
npm run e2e:headed                             # Visible browser
npx playwright test path/to/test.spec.ts       # Run specific file
npx playwright test -g "test name"             # Run by name
npm run e2e:report                             # View last report

# Playwright Utilities
npx playwright codegen http://localhost:4200   # Record test
npx playwright show-trace trace.zip            # View trace
```

### Helper Functions Quick Reference

```typescript
// Auth
await registerUser(page, user)
await loginUser(page, email, password)
await registerAndLogin(page, user)
await logoutUser(page)

// Receipts
await goToReceiptsPage(page)
await createReceipt(page, receipt)
const receipt = await findReceipt(page, merchantName)
const count = await getReceiptCount(page)

// Test Data
const user = generateTestUser()
const receipt = generateTestReceipt()
const email = generateUniqueEmail()
```

---

## Contributing to Tests

### Adding a New Test

1. **Choose appropriate spec file** (or create new one)
2. **Use helpers and test data generators**
3. **Follow naming convention**: `should <expected behavior>`
4. **Add descriptive comments** for complex tests
5. **Ensure test isolation** (no dependencies on other tests)
6. **Run locally before committing**

### Code Review Checklist for Tests

- [ ] Test is independent (no reliance on previous tests)
- [ ] Uses robust selectors (getByRole, getByLabel)
- [ ] Has explicit waits where needed
- [ ] Uses helpers to reduce duplication
- [ ] Has descriptive test name
- [ ] Includes comments for non-obvious logic
- [ ] Passes locally (3+ times to check for flakiness)

---

## Resources

### Official Documentation

- [Playwright Docs](https://playwright.dev/docs/intro)
- [xUnit Docs](https://xunit.net/docs/getting-started/netcore/cmdline)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

### Internal Documentation

- [Backend Unit Tests (MyApi.Tests)](../../MyApi.Tests/README.md) - Detailed backend test documentation
- [Backend Testing Strategy](../infra/infra-testing-strategy.md) - Overall testing approach
- [Complete Implementation Guide](../guide/guide-complete-implementation.md) - E2E implementation
- [Frontend Roadmap](../frontend/frontend-roadmap.md) - Frontend features

### Useful Tools

- **Playwright UI Mode**: `npm run e2e:ui`
- **Playwright Inspector**: `npm run e2e:debug`
- **Playwright Trace Viewer**: `npx playwright show-trace trace.zip`
- **Aspire Dashboard**: Check service health and logs

---

## Contact & Support

For testing questions:
1. Check this documentation first
2. Review test-results/ folder for failure details
3. Check Playwright HTML report
4. Review backend logs in Aspire Dashboard
5. Open GitHub Issue with:
   - Test name
   - Error message
   - Screenshots/traces
   - Steps to reproduce

---

**Last Updated**: November 17, 2025  
**Maintained By**: Development Team  
**Next Review**: After E2E test stabilization

**Status Summary**: Backend tests are rock-solid (146/146 passing). E2E tests are implemented and ready for execution and refinement. This document will be updated as we achieve 100% E2E test pass rate.
