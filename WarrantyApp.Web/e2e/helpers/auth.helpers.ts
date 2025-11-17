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
 */
export async function registerUser(page: Page, user: TestUser): Promise<void> {
  await page.goto('/register');
  
  // Fill registration form
  await page.getByLabel(/email/i).fill(user.email);
  await page.getByLabel(/username/i).fill(user.username);
  await page.getByLabel('Password', { exact: true }).fill(user.password);
  await page.getByLabel(/confirm password/i).fill(user.password);
  
  if (user.firstName) {
    await page.getByLabel(/first name/i).fill(user.firstName);
  }
  if (user.lastName) {
    await page.getByLabel(/last name/i).fill(user.lastName);
  }
  
  // Submit form
  await page.getByRole('button', { name: /create account|register/i }).click();
  
  // Wait for registration to complete (redirect or success message)
  await page.waitForURL(/\/(login|receipts|dashboard)/i, { timeout: 10000 });
}

/**
 * Login with email and password
 */
export async function loginUser(page: Page, email: string, password: string): Promise<void> {
  await page.goto('/login');
  
  // Fill login form
  await page.getByLabel(/email/i).fill(email);
  await page.getByLabel(/password/i).fill(password);
  
  // Submit form
  await page.getByRole('button', { name: /login|sign in/i }).click();
  
  // Wait for successful login (redirect to receipts or dashboard)
  await page.waitForURL(/\/(receipts|dashboard)/i, { timeout: 10000 });
}

/**
 * Login with 2FA code
 */
export async function loginWith2FA(page: Page, email: string, password: string, code: string): Promise<void> {
  await page.goto('/login');
  
  // Fill login form
  await page.getByLabel(/email/i).fill(email);
  await page.getByLabel(/password/i).fill(password);
  await page.getByRole('button', { name: /login|sign in/i }).click();
  
  // Wait for 2FA input
  await expect(page.getByLabel(/2fa code|verification code/i)).toBeVisible();
  
  // Enter 2FA code
  await page.getByLabel(/2fa code|verification code/i).fill(code);
  await page.getByRole('button', { name: /verify|submit/i }).click();
  
  // Wait for successful login
  await page.waitForURL(/\/(receipts|dashboard)/i, { timeout: 10000 });
}

/**
 * Logout current user
 */
export async function logoutUser(page: Page): Promise<void> {
  // Look for logout button/link (might be in dropdown or nav)
  const logoutButton = page.getByRole('button', { name: /logout|sign out/i })
    .or(page.getByRole('link', { name: /logout|sign out/i }));
  
  await logoutButton.click();
  
  // Wait for redirect to login or landing page
  await page.waitForURL(/\/(login|\/)/i, { timeout: 5000 });
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
  await expect(page.getByRole('heading', { name: /login|sign in/i })).toBeVisible();
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
  
  await page.getByLabel(/email/i).fill(email);
  await page.getByRole('button', { name: /reset|send/i }).click();
  
  // Wait for success message
  await expect(page.getByText(/email sent|check your email/i)).toBeVisible();
}
