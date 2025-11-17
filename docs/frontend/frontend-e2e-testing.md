# E2E Testing Strategy - Playwright

**Created**: November 17, 2025  
**Status**: In Progress  
**Framework**: Playwright 1.48.2

## Overview

This document outlines the comprehensive end-to-end testing strategy for the Warranty App frontend using Playwright. Tests are organized by feature area and follow best practices for maintainability and reliability.

## Testing Philosophy

- **User-centric**: Tests simulate real user workflows
- **Isolated**: Each test runs independently with clean state
- **Fast**: Parallel execution across browsers
- **Reliable**: Proper waits, no flaky tests
- **Maintainable**: Reusable helpers and page objects

## Test Organization

```
e2e/
├── auth/                   # Authentication & onboarding tests
│   ├── landing.spec.ts     # ✅ Landing page navigation
│   ├── login.spec.ts       # Login flow
│   ├── register.spec.ts    # Registration flow
│   └── 2fa.spec.ts         # Two-factor authentication
├── receipts/               # Receipt management tests
│   ├── upload.spec.ts      # Receipt upload
│   ├── list.spec.ts        # Receipt listing & pagination
│   ├── details.spec.ts     # Receipt view/edit/delete
│   └── sharing.spec.ts     # Receipt sharing
├── warranties/             # Warranty tracking tests
│   └── dashboard.spec.ts   # Warranty dashboard
├── profile/                # User profile tests
│   └── settings.spec.ts    # Profile & notification settings
└── helpers/                # Shared utilities
    ├── auth.helpers.ts     # Authentication helpers
    └── test-data.ts        # Test data generators
```

## Test Coverage Roadmap

### Batch 1: Authentication (Priority 1) 🔄
- [x] Landing page navigation
- [ ] User registration
- [ ] User login (email/password)
- [ ] Login with 2FA
- [ ] Logout
- [ ] Password reset flow
- [ ] Email confirmation

**Estimated**: 3-4 hours

### Batch 2: Receipt Management (Priority 2)
- [ ] Upload single receipt (image)
- [ ] Upload single receipt (PDF)
- [ ] View receipt list (empty state)
- [ ] View receipt list (with data)
- [ ] Receipt pagination
- [ ] View receipt details
- [ ] Edit receipt
- [ ] Delete receipt

**Estimated**: 3-4 hours

### Batch 3: OCR & Advanced Features (Priority 3)
- [ ] Trigger OCR processing
- [ ] View OCR results
- [ ] Edit OCR-extracted data
- [ ] Upload multiple receipts
- [ ] Receipt sharing flow
- [ ] View shared receipts

**Estimated**: 2-3 hours

### Batch 4: Warranties & Settings (Priority 4)
- [ ] View warranty dashboard
- [ ] Filter warranties by urgency
- [ ] Update notification settings
- [ ] Phone verification
- [ ] Profile update

**Estimated**: 2-3 hours

**Total Estimated Time**: 10-14 hours

## Test Environment

### Configuration

- **Base URL**: `http://localhost:4200`
- **Browsers**: Chromium, Firefox, WebKit
- **Parallel Execution**: Yes (except CI)
- **Retries**: 2 in CI, 0 locally
- **Timeout**: 30 seconds per test

### Prerequisites

```bash
# Install dependencies
npm install

# Start backend API (required)
cd ../AppHost && dotnet run

# Run tests (starts frontend automatically)
npm run e2e
```

## Best Practices

### 1. Authentication Helpers

Create reusable auth helpers to avoid repeating login logic:

```typescript
// e2e/helpers/auth.helpers.ts
export async function loginAsUser(page: Page, email: string, password: string) {
  await page.goto('/login');
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Password').fill(password);
  await page.getByRole('button', { name: /login/i }).click();
  await page.waitForURL('/receipts');
}
```

### 2. Test Data Management

Use consistent test data:

```typescript
export const TEST_USERS = {
  validUser: {
    email: 'test@example.com',
    password: 'Test123!',
    username: 'testuser'
  }
};
```

### 3. Page Object Pattern

For complex pages, use page objects:

```typescript
export class ReceiptsPage {
  constructor(private page: Page) {}
  
  async uploadReceipt(filePath: string) {
    await this.page.getByRole('button', { name: /upload/i }).click();
    await this.page.setInputFiles('input[type="file"]', filePath);
  }
}
```

### 4. Assertions

Use meaningful assertions with good selectors:

```typescript
// Good: Semantic selectors
await expect(page.getByRole('heading', { name: 'Login' })).toBeVisible();

// Avoid: Brittle selectors
await expect(page.locator('.login-title')).toBeVisible();
```

### 5. Wait Strategies

Use proper wait conditions:

```typescript
// Wait for navigation
await page.waitForURL('/receipts');

// Wait for API response
await page.waitForResponse(resp => resp.url().includes('/api/receipts'));

// Wait for element state
await page.getByRole('button').waitFor({ state: 'visible' });
```

## Running Tests

### Development

```bash
# Run all tests
npm run e2e

# Run specific file
npx playwright test e2e/auth/login.spec.ts

# Run in UI mode (interactive)
npm run e2e:ui

# Debug mode (step through)
npm run e2e:debug

# Run with visible browser
npm run e2e:headed
```

### CI/CD Integration

Tests run automatically in GitHub Actions on:
- Pull requests
- Pushes to main branch
- Manual workflow dispatch

```yaml
- name: Run E2E tests
  run: npm run e2e
```

## Test Data Strategy

### User Accounts

Tests should create fresh users per test to avoid conflicts:

```typescript
test('should register new user', async ({ page }) => {
  const uniqueEmail = `test-${Date.now()}@example.com`;
  // ... registration logic
});
```

### Cleanup

Tests should clean up created resources when possible:

```typescript
test.afterEach(async ({ request }) => {
  // Delete test receipts
  await request.delete('/api/receipts/test-data');
});
```

## Debugging Failed Tests

### 1. View Test Report

```bash
npm run e2e:report
```

### 2. Check Screenshots

Failed tests automatically capture screenshots in `test-results/`

### 3. View Traces

```bash
npx playwright show-trace test-results/*/trace.zip
```

### 4. Run in Debug Mode

```bash
npm run e2e:debug
```

## Performance Considerations

- **Parallel execution**: Tests run in parallel by default
- **Resource sharing**: Use `webServer` config to share dev server
- **Selective testing**: Use `.only` for focused development
- **Skip slow tests**: Use `.skip` for known slow tests during development

## Known Issues & Workarounds

### Issue: API not ready
**Symptom**: Tests fail with network errors  
**Solution**: Increase `webServer.timeout` to 120 seconds

### Issue: Flaky navigation tests
**Symptom**: Intermittent failures on navigation  
**Solution**: Use `page.waitForURL()` instead of checking current URL

## Success Metrics

- **Coverage**: All critical user paths tested
- **Reliability**: <1% flaky test rate
- **Speed**: Full suite completes in <5 minutes
- **Maintainability**: Tests updated with feature changes

## Next Steps

1. ✅ Complete Batch 1 (Authentication tests)
2. Implement Batch 2 (Receipt management)
3. Implement Batch 3 (OCR & advanced features)
4. Implement Batch 4 (Warranties & settings)
5. Integrate with CI/CD pipeline
6. Add visual regression tests (optional)

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Best Practices Guide](https://playwright.dev/docs/best-practices)
- [Test API Reference](https://playwright.dev/docs/api/class-test)

---

**Status**: Batch 1 in progress (Authentication tests)  
**Last Updated**: November 17, 2025
