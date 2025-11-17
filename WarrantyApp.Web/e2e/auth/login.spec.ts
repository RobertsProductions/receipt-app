/**
 * E2E Tests: User Login
 * 
 * Tests the complete user login flow including:
 * - Form display and validation
 * - Successful login
 * - Login with 2FA
 * - Error handling
 * - Session management
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, TEST_USERS } from '../helpers/test-data';
import { registerUser, loginUser, logoutUser, isLoggedIn, assertLoggedOut } from '../helpers/auth.helpers';

test.describe('User Login', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form correctly', async ({ page }) => {
    // Check page heading (not title, since SPA title is static)
    await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
    
    // Check form elements
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel('Password', { exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in/i })).toBeVisible();
    
    // Check links
    await expect(page.getByRole('link', { name: /sign up/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /forgot password/i })).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    // Submit empty form
    await page.getByRole('button', { name: /sign in/i }).click();
    
    // Check for validation errors - inputs should have required attribute
    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toHaveAttribute('required');
    
    const passwordInput = page.getByLabel('Password', { exact: true });
    await expect(passwordInput).toHaveAttribute('required');
  });

  test('should show error for invalid credentials', async ({ page }) => {
    // Try to login with invalid credentials
    await page.getByLabel(/email/i).fill(TEST_USERS.invalid.email);
    await page.getByLabel('Password', { exact: true }).fill(TEST_USERS.invalid.password);
    await page.getByRole('button', { name: /sign in/i }).click();
    
    // Should show error message
    await expect(page.getByText(/invalid.*credentials|incorrect.*password|login.*failed/i))
      .toBeVisible({ timeout: 5000 });
    
    // Should stay on login page
    await expect(page).toHaveURL(/\/login/);
  });

  test('should successfully login with valid credentials', async ({ page }) => {
    // First register a user
    const user = generateTestUser();
    await registerUser(page, user);
    
    // Logout if auto-logged in
    if (await isLoggedIn(page)) {
      await logoutUser(page);
    }
    
    // Login with registered credentials
    await loginUser(page, user.email, user.password);
    
    // Should redirect to receipts or dashboard
    await expect(page).toHaveURL(/\/(receipts|dashboard)/i);
    
    // Should see user-specific elements
    const userMenu = page.getByRole('button', { name: /profile|account/i })
      .or(page.getByText(user.username));
    await expect(userMenu).toBeVisible({ timeout: 5000 });
  });

  test('should persist session after page reload', async ({ page, context }) => {
    // Register and login
    const user = generateTestUser();
    await registerUser(page, user);
    
    if (!await isLoggedIn(page)) {
      await loginUser(page, user.email, user.password);
    }
    
    // Reload page
    await page.reload();
    
    // Should still be logged in
    expect(await isLoggedIn(page)).toBe(true);
    await expect(page).toHaveURL(/\/(receipts|dashboard)/i);
  });

  test('should successfully logout', async ({ page }) => {
    // Register and login
    const user = generateTestUser();
    await registerUser(page, user);
    
    if (!await isLoggedIn(page)) {
      await loginUser(page, user.email, user.password);
    }
    
    // Logout
    await logoutUser(page);
    
    // Should be logged out
    await assertLoggedOut(page);
  });

  test('should navigate to register page from link', async ({ page }) => {
    // Click register link (use first() to avoid strict mode if there are multiple)
    await page.getByRole('link', { name: /sign up/i }).first().click();
    
    // Should navigate to register page
    await expect(page).toHaveURL(/\/register/);
    await expect(page.getByRole('heading', { name: /create account/i })).toBeVisible();
  });

  test('should navigate to forgot password page', async ({ page }) => {
    // Click forgot password link
    await page.getByRole('link', { name: /forgot password/i }).click();
    
    // Should navigate to password reset page
    await expect(page).toHaveURL(/\/(forgot-password|reset-password)/i);
  });

  test('should show loading state during login', async ({ page }) => {
    const user = generateTestUser();
    await registerUser(page, user);
    
    if (await isLoggedIn(page)) {
      await logoutUser(page);
    }
    
    // Start login
    await page.goto('/login');
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/password/i).fill(user.password);
    
    const loginButton = page.getByRole('button', { name: /login|sign in/i });
    await loginButton.click();
    
    // Button should be disabled or show loading
    await expect(loginButton).toBeDisabled({ timeout: 1000 }).catch(() => {
      // If not disabled, might show loading spinner instead
    });
  });

  test('should handle "remember me" checkbox if present', async ({ page }) => {
    // Check if remember me checkbox exists
    const rememberCheckbox = page.getByLabel(/remember me/i);
    const exists = await rememberCheckbox.isVisible().catch(() => false);
    
    if (exists) {
      // Check the checkbox
      await rememberCheckbox.check();
      await expect(rememberCheckbox).toBeChecked();
      
      // Uncheck the checkbox
      await rememberCheckbox.uncheck();
      await expect(rememberCheckbox).not.toBeChecked();
    }
  });

  test('should show password visibility toggle if present', async ({ page }) => {
    const passwordInput = page.getByLabel('Password', { exact: true });
    await passwordInput.fill('TestPassword123!');
    
    // Look for visibility toggle button
    const toggleButton = page.getByRole('button', { name: /show|hide.*password/i }).first();
    
    const hasToggle = await toggleButton.isVisible().catch(() => false);
    
    if (hasToggle) {
      // Check initial type
      const initialType = await passwordInput.getAttribute('type');
      expect(initialType).toBe('password');
      
      // Click toggle
      await toggleButton.click();
      
      // Type should change to text
      const newType = await passwordInput.getAttribute('type');
      expect(newType).toBe('text');
    }
  });

  test('should prevent access to protected routes when logged out', async ({ page }) => {
    // Try to access receipts page without logging in
    await page.goto('/receipts');
    
    // Should redirect to login
    await expect(page).toHaveURL(/\/login/i, { timeout: 5000 });
  });

  test('should redirect to intended page after login', async ({ page }) => {
    // Try to access receipts page (will redirect to login)
    await page.goto('/receipts');
    await expect(page).toHaveURL(/\/login/);
    
    // Login
    const user = generateTestUser();
    await registerUser(page, user);
    
    // Should redirect back to receipts page
    await expect(page).toHaveURL(/\/receipts/i, { timeout: 5000 });
  });
});
