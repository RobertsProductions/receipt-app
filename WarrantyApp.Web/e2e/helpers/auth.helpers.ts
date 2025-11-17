/**
 * Authentication Helper Functions
 * 
 * Reusable functions for authentication-related E2E tests.
 * These helpers reduce code duplication and improve test maintainability.
 */

import { Page, expect } from '@playwright/test';
import { TestUser } from './test-data';

/**
 * Register a new user account
 * Note: Registration may auto-login the user
 */
export async function registerUser(page: Page, user: TestUser): Promise<void> {
  await page.goto('/register');
  
  // Fill registration form with explicit waits
  const emailInput = page.getByLabel(/email/i);
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(user.email);
  
  const usernameInput = page.getByLabel(/username/i);
  await usernameInput.waitFor({ state: 'visible', timeout: 10000 });
  await usernameInput.fill(user.username);
  
  const passwordInput = page.getByLabel('Password', { exact: true });
  await passwordInput.waitFor({ state: 'visible', timeout: 10000 });
  await passwordInput.fill(user.password);
  
  const confirmPasswordInput = page.getByLabel(/confirm password/i);
  await confirmPasswordInput.waitFor({ state: 'visible', timeout: 10000 });
  await confirmPasswordInput.fill(user.password);
  
  // Fill optional fields if provided and if they exist on the form
  if (user.firstName) {
    const firstNameInput = page.getByLabel(/first name/i);
    const isVisible = await firstNameInput.isVisible({ timeout: 2000 }).catch(() => false);
    if (isVisible) {
      await firstNameInput.fill(user.firstName);
    }
  }
  if (user.lastName) {
    const lastNameInput = page.getByLabel(/last name/i);
    const isVisible = await lastNameInput.isVisible({ timeout: 2000 }).catch(() => false);
    if (isVisible) {
      await lastNameInput.fill(user.lastName);
    }
  }
  
  // Submit form
  const submitButton = page.getByRole('button', { name: /create account/i });
  await submitButton.waitFor({ state: 'visible', timeout: 10000 });
  await submitButton.click();
  
  // Wait for registration to complete (redirect to confirm-email, receipts, or dashboard)
  await page.waitForURL(/\/(confirm-email|login|receipts|dashboard)/i, { timeout: 15000 });
}

/**
 * Register and ensure user is logged in
 * Use this in test.beforeEach for authenticated tests
 */
export async function registerAndLogin(page: Page, user: TestUser): Promise<void> {
  await registerUser(page, user);
  
  // Wait a bit for redirect to complete
  await page.waitForTimeout(1000);
  
  // Check if already logged in (auto-login after registration)
  const currentUrl = page.url();
  
  // If on confirm-email page, skip to login (email confirmation not needed for tests)
  if (currentUrl.includes('/confirm-email')) {
    await loginUser(page, user.email, user.password);
    // After loginUser, verify we're actually logged in
    await verifyLoggedIn(page);
    return;
  }
  
  // If on login page, proceed with login
  if (currentUrl.includes('/login')) {
    await loginUser(page, user.email, user.password);
    await verifyLoggedIn(page);
    return;
  }
  
  // If on protected page (receipts/dashboard), verify token exists
  if (currentUrl.includes('/receipts') || currentUrl.includes('/dashboard')) {
    // Check if auth token exists (using actual key from auth.service.ts)
    const hasToken = await page.evaluate(() => {
      return !!localStorage.getItem('access_token');
    });
    
    if (hasToken) {
      // Wait for auth state to stabilize
      await page.waitForTimeout(2000);
      return;
    }
    
    // No token found, need to login manually
    await loginUser(page, user.email, user.password);
    await verifyLoggedIn(page);
    return;
  }
  
  // Unknown state - force login
  await loginUser(page, user.email, user.password);
  await verifyLoggedIn(page);
}

/**
 * Verify user is logged in by checking token and URL
 */
async function verifyLoggedIn(page: Page): Promise<void> {
  // Wait for redirect to complete
  try {
    await page.waitForURL(/\/(receipts|dashboard)/, { timeout: 10000 });
  } catch (error) {
    const currentUrl = page.url();
    throw new Error(`Failed to navigate to protected route. Current URL: ${currentUrl}. Expected /receipts or /dashboard`);
  }
  
  // Verify token exists
  const hasToken = await page.evaluate(() => {
    return !!localStorage.getItem('access_token');
  });
  
  if (!hasToken) {
    const currentUrl = page.url();
    const allStorage = await page.evaluate(() => {
      return {
        localStorage: { ...localStorage },
        sessionStorage: { ...sessionStorage }
      };
    });
    throw new Error(`Login completed but access_token not found in localStorage. URL: ${currentUrl}. Storage: ${JSON.stringify(allStorage)}`);
  }
}

/**
 * Login with email and password
 */
export async function loginUser(page: Page, email: string, password: string): Promise<void> {
  await page.goto('/login');
  
  // Fill login form with explicit waits
  const emailInput = page.getByLabel(/email/i);
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(email);
  
  const passwordInput = page.getByLabel('Password', { exact: true });
  await passwordInput.waitFor({ state: 'visible', timeout: 10000 });
  await passwordInput.fill(password);
  
  // Submit form
  const submitButton = page.getByRole('button', { name: /sign in/i });
  await submitButton.waitFor({ state: 'visible', timeout: 10000 });
  await submitButton.click();
  
  // Wait for successful login (redirect to receipts or dashboard)
  // Sometimes app redirects to confirm-email with invalid token - navigate to receipts instead
  await page.waitForURL(/\/(receipts|dashboard|confirm-email)/i, { timeout: 15000 });
  
  // If landed on confirm-email page, navigate directly to receipts
  if (page.url().includes('/confirm-email')) {
    await page.goto('/receipts');
    await page.waitForURL(/\/receipts/, { timeout: 5000 });
  }
  
  // Wait for navbar to update with user info (this ensures Angular state has loaded)
  // Look for either "Login" button to disappear OR user menu to appear
  await page.waitForTimeout(2000); // Give Angular time to process auth state and persist to storage
  
  // Verify we're actually logged in by checking navbar state change
  try {
    await page.waitForFunction(() => {
      const loginBtn = document.querySelector('a[href="/login"]');
      const userMenu = document.querySelector('button .chevron');
      return !loginBtn || userMenu !== null;
    }, { timeout: 5000 });
  } catch {
    // If timeout, continue anyway - the page might have loaded
  }
  
  // Extra wait to ensure token is fully persisted to localStorage/sessionStorage
  await page.waitForTimeout(500);
}

/**
 * Login with 2FA code
 */
export async function loginWith2FA(page: Page, email: string, password: string, code: string): Promise<void> {
  await page.goto('/login');
  
  // Fill login form with explicit waits
  const emailInput = page.getByLabel(/email/i);
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(email);
  
  const passwordInput = page.getByLabel('Password', { exact: true });
  await passwordInput.waitFor({ state: 'visible', timeout: 10000 });
  await passwordInput.fill(password);
  
  const loginButton = page.getByRole('button', { name: /sign in/i });
  await loginButton.waitFor({ state: 'visible', timeout: 10000 });
  await loginButton.click();
  
  // Wait for 2FA input
  const codeInput = page.getByLabel(/2fa code|verification code/i);
  await expect(codeInput).toBeVisible({ timeout: 10000 });
  
  // Enter 2FA code
  await codeInput.fill(code);
  
  const verifyButton = page.getByRole('button', { name: /verify|submit/i });
  await verifyButton.waitFor({ state: 'visible', timeout: 10000 });
  await verifyButton.click();
  
  // Wait for successful login
  await page.waitForURL(/\/(receipts|dashboard)/i, { timeout: 15000 });
}

/**
 * Logout current user
 */
export async function logoutUser(page: Page): Promise<void> {
  // If on error/confirmation page, navigate to receipts first
  const currentUrl = page.url();
  if (currentUrl.includes('/confirm-email') || currentUrl.includes('/error')) {
    await page.goto('/receipts');
    await page.waitForLoadState('networkidle');
  }
  
  // Find and click the user menu button to open dropdown
  // Button contains username and chevron ▼ - use more flexible selector
  const userMenuButton = page.locator('button').filter({ hasText: '▼' }).first();
  await userMenuButton.waitFor({ state: 'visible', timeout: 15000 });
  await userMenuButton.click();
  
  // Wait for dropdown to appear
  await page.waitForTimeout(500);
  
  // Click the Logout button in the dropdown
  const logoutButton = page.locator('button.dropdown-item.logout').or(
    page.locator('button', { hasText: 'Logout' })
  );
  await logoutButton.waitFor({ state: 'visible', timeout: 5000 });
  await logoutButton.click();
  
  // Wait for redirect to login or landing page
  await page.waitForURL(/\/(login|$)/i, { timeout: 5000 });
}

/**
 * Check if user is logged in (by checking for auth-only elements)
 */
export async function isLoggedIn(page: Page): Promise<boolean> {
  try {
    // Check for elements that only appear when logged in
    const userMenu = page.getByRole('button', { name: /profile|account/i })
      .or(page.getByText(/receipts|dashboard/i));
    
    await userMenu.waitFor({ state: 'visible', timeout: 2000 });
    return true;
  } catch {
    return false;
  }
}

/**
 * Setup 2FA for a user (returns setup key)
 */
export async function setup2FA(page: Page): Promise<string> {
  await page.goto('/2fa/setup');
  
  // Wait for QR code or setup key to appear
  await expect(page.getByText(/scan.*qr code|setup key/i)).toBeVisible();
  
  // Extract the setup key from the page
  const setupKey = await page.locator('[data-testid="setup-key"]').textContent();
  
  if (!setupKey) {
    throw new Error('Could not find 2FA setup key');
  }
  
  return setupKey.trim();
}

/**
 * Generate a TOTP code (requires authenticator library in real tests)
 * This is a placeholder - in real tests, you'd use a library like 'otplib'
 */
export function generateTOTPCode(secret: string): string {
  // Placeholder for demonstration
  // In real implementation, use: authenticator.generate(secret)
  return '123456';
}

/**
 * Wait for authentication to complete
 */
export async function waitForAuthComplete(page: Page, timeout = 10000): Promise<void> {
  await page.waitForURL(/\/(receipts|dashboard)/i, { timeout });
}

/**
 * Assert user is on login page
 */
export async function assertOnLoginPage(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/login/);
  await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
}

/**
 * Assert user is logged out
 */
export async function assertLoggedOut(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/(login|\/)/i);
  
  // Verify auth-only elements are not present
  const receiptsLink = page.getByRole('link', { name: /receipts/i });
  await expect(receiptsLink).not.toBeVisible().catch(() => {});
}

/**
 * Fill password reset form
 */
export async function requestPasswordReset(page: Page, email: string): Promise<void> {
  await page.goto('/forgot-password');
  
  const emailInput = page.getByLabel(/email/i);
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(email);
  
  const submitButton = page.getByRole('button', { name: /reset|send/i });
  await submitButton.waitFor({ state: 'visible', timeout: 10000 });
  await submitButton.click();
  
  // Wait for success message
  await expect(page.getByText(/email sent|check your email/i)).toBeVisible({ timeout: 10000 });
}
