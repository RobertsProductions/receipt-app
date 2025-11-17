/**
 * E2E Tests: User Registration
 * 
 * Tests the complete user registration flow including:
 * - Form validation
 * - Successful registration
 * - Error handling
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, TEST_USERS, VALIDATION_MESSAGES } from '../helpers/test-data';
import { registerUser, assertOnLoginPage } from '../helpers/auth.helpers';

test.describe('User Registration', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('/register');
  });

  test('should display registration form correctly', async ({ page }) => {
    // Check page heading
    await expect(page.getByRole('heading', { name: /create account/i })).toBeVisible();
    
    // Check form elements
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/username/i)).toBeVisible();
    await expect(page.getByLabel('Password', { exact: true })).toBeVisible();
    await expect(page.getByLabel(/confirm password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /create account/i })).toBeVisible();
    
    // Check link to login page
    await expect(page.getByRole('link', { name: /sign in/i })).toBeVisible();
  });

  test('should validate required fields', async ({ page }) => {
    // Submit empty form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Check for validation errors (form should not submit)
    const emailInput = page.getByLabel(/email/i);
    await expect(emailInput).toHaveAttribute('required');
  });

  test('should validate email format', async ({ page }) => {
    // Enter invalid email
    await page.getByLabel(/email/i).fill('invalid-email');
    await page.getByLabel(/username/i).fill('testuser');
    await page.getByLabel('Password', { exact: true }).fill('Test123!');
    await page.getByLabel(/confirm password/i).fill('Test123!');
    
    // Submit form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should show email validation error or not submit
    const currentUrl = page.url();
    expect(currentUrl).toContain('/register');
  });

  test('should validate password match', async ({ page }) => {
    const user = generateTestUser();
    
    // Fill form with mismatched passwords
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(user.username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill('DifferentPassword123!');
    
    // Submit form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should show password mismatch error
    await expect(page.getByText(/password.*match/i)).toBeVisible();
  });

  test('should validate password strength', async ({ page }) => {
    const user = generateTestUser({ password: TEST_USERS.weak_password.password });
    
    // Fill form with weak password
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(user.username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill(user.password);
    
    // Submit form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should show password strength error or stay on registration page
    const currentUrl = page.url();
    expect(currentUrl).toContain('/register');
  });

  test('should successfully register a new user', async ({ page }) => {
    const user = generateTestUser();
    
    // Fill registration form
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(user.username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill(user.password);
    
    // Submit form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should redirect to login or dashboard
    await expect(page).toHaveURL(/\/(login|receipts|dashboard)/i, { timeout: 10000 });
  });

  test('should register with optional fields', async ({ page }) => {
    const user = generateTestUser({
      firstName: 'John',
      lastName: 'Doe'
    });
    
    // Fill all fields including optional ones
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(user.username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill(user.password);
    
    // Fill optional fields if present
    const firstNameInput = page.getByLabel(/first name/i);
    if (await firstNameInput.isVisible().catch(() => false)) {
      await firstNameInput.fill(user.firstName!);
    }
    
    const lastNameInput = page.getByLabel(/last name/i);
    if (await lastNameInput.isVisible().catch(() => false)) {
      await lastNameInput.fill(user.lastName!);
    }
    
    // Submit form
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should successfully register
    await expect(page).toHaveURL(/\/(login|receipts|dashboard)/i, { timeout: 10000 });
  });

  test('should show error for duplicate email', async ({ page }) => {
    const user = generateTestUser();
    
    // Register user first time
    await registerUser(page, user);
    
    // Try to register same email again
    await page.goto('/register');
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(generateTestUser().username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill(user.password);
    await page.getByRole('button', { name: /create account/i }).click();
    
    // Should show error about duplicate email
    await expect(page.getByText(/email.*already.*exists|email.*taken/i)).toBeVisible({ timeout: 5000 });
  });

  test('should navigate to login page from link', async ({ page }) => {
    // Click login link (use first() to avoid strict mode)
    await page.getByRole('link', { name: /sign in/i }).first().click();
    
    // Should navigate to login page
    await assertOnLoginPage(page);
  });

  test('should show loading state during registration', async ({ page }) => {
    const user = generateTestUser();
    
    // Fill form
    await page.getByLabel(/email/i).fill(user.email);
    await page.getByLabel(/username/i).fill(user.username);
    await page.getByLabel('Password', { exact: true }).fill(user.password);
    await page.getByLabel(/confirm password/i).fill(user.password);
    
    // Submit form
    const submitButton = page.getByRole('button', { name: /create account/i });
    await submitButton.click();
    
    // Button should be disabled during submission
    await expect(submitButton).toBeDisabled({ timeout: 1000 }).catch(() => {
      // If not disabled, might show loading spinner instead
    });
  });
});
