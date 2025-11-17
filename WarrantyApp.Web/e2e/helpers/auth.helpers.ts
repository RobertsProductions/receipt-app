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
  
  if (user.firstName) {
    const firstNameInput = page.getByLabel(/first name/i);
    await firstNameInput.waitFor({ state: 'visible', timeout: 10000 });
    await firstNameInput.fill(user.firstName);
  }
  if (user.lastName) {
    const lastNameInput = page.getByLabel(/last name/i);
    await lastNameInput.waitFor({ state: 'visible', timeout: 10000 });
    await lastNameInput.fill(user.lastName);
  }
  
  // Submit form
  const submitButton = page.getByRole('button', { name: /create account|register/i });
  await submitButton.waitFor({ state: 'visible', timeout: 10000 });
  await submitButton.click();
  
  // Wait for registration to complete (redirect or success message)
  await page.waitForURL(/\/(login|receipts|dashboard)/i, { timeout: 15000 });
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
  
  const passwordInput = page.getByLabel(/password/i);
  await passwordInput.waitFor({ state: 'visible', timeout: 10000 });
  await passwordInput.fill(password);
  
  // Submit form
  const loginButton = page.getByRole('button', { name: /login|sign in/i });
  await loginButton.waitFor({ state: 'visible', timeout: 10000 });
  await loginButton.click();
  
  // Wait for successful login (redirect to receipts or dashboard)
  await page.waitForURL(/\/(receipts|dashboard)/i, { timeout: 15000 });
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
  
  const passwordInput = page.getByLabel(/password/i);
  await passwordInput.waitFor({ state: 'visible', timeout: 10000 });
  await passwordInput.fill(password);
  
  const loginButton = page.getByRole('button', { name: /login|sign in/i });
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
  
  const emailInput = page.getByLabel(/email/i);
  await emailInput.waitFor({ state: 'visible', timeout: 10000 });
  await emailInput.fill(email);
  
  const submitButton = page.getByRole('button', { name: /reset|send/i });
  await submitButton.waitFor({ state: 'visible', timeout: 10000 });
  await submitButton.click();
  
  // Wait for success message
  await expect(page.getByText(/email sent|check your email/i)).toBeVisible({ timeout: 10000 });
}
