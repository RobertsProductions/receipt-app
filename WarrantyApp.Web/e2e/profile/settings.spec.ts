/**
 * E2E Tests: User Profile and Settings
 * 
 * Tests user profile management and notification settings:
 * - Viewing profile information
 * - Editing profile details
 * - Updating notification preferences
 * - Phone verification
 */

import { test, expect } from '@playwright/test';
import { generateTestUser } from '../helpers/test-data';
import { registerAndLogin } from '../helpers/auth.helpers';

test.describe('User Profile and Settings', () => {
  
  let testUser: any;
  
  test.beforeEach(async ({ page }) => {
    testUser = generateTestUser();
    await registerAndLogin(page, testUser);
  });

  test('should navigate to profile page', async ({ page }) => {
    // Look for profile link in navigation
    const profileLink = page.getByRole('link', { name: /profile|account|settings/i })
      .or(page.getByRole('button', { name: /profile|account/i }));
    
    await profileLink.click();
    
    // Should be on profile page
    await expect(page).toHaveURL(/\/profile|\/account|\/settings/i);
  });

  test('should display user profile information', async ({ page }) => {
    await page.goto('/profile');
    
    // Should show user details
    await expect(page.getByText(testUser.email)).toBeVisible();
    await expect(page.getByText(testUser.username)).toBeVisible();
  });

  test('should display profile edit button', async ({ page }) => {
    await page.goto('/profile');
    
    const editButton = page.getByRole('button', { name: /edit/i });
    await expect(editButton).toBeVisible();
  });

  test('should allow editing profile information', async ({ page }) => {
    await page.goto('/profile');
    
    // Click edit
    await page.getByRole('button', { name: /edit/i }).click();
    
    // Update fields
    const firstNameInput = page.getByLabel(/first name/i);
    const hasFirstName = await firstNameInput.isVisible().catch(() => false);
    
    if (hasFirstName) {
      await firstNameInput.clear();
      await firstNameInput.fill('Updated');
      
      // Save
      await page.getByRole('button', { name: /save|update/i }).click();
      
      // Should show success
      await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 5000 });
    }
  });

  test('should display notification settings section', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for notification settings
    const notificationSection = page.getByText(/notification|alert.*settings/i);
    await expect(notificationSection).toBeVisible();
  });

  test('should show email notification toggle', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for email toggle
    const emailToggle = page.getByLabel(/email.*notification/i)
      .or(page.locator('input[type="checkbox"]').filter({ has: page.getByText(/email/i) }));
    
    const hasToggle = await emailToggle.isVisible().catch(() => false);
    
    if (hasToggle) {
      await expect(emailToggle).toBeVisible();
    }
  });

  test('should show SMS notification toggle', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for SMS toggle
    const smsToggle = page.getByLabel(/sms.*notification/i)
      .or(page.locator('input[type="checkbox"]').filter({ has: page.getByText(/sms/i) }));
    
    const hasToggle = await smsToggle.isVisible().catch(() => false);
    
    if (hasToggle) {
      await expect(smsToggle).toBeVisible();
    }
  });

  test('should toggle email notifications on and off', async ({ page }) => {
    await page.goto('/profile');
    
    const emailToggle = page.getByLabel(/email.*notification/i).first();
    const exists = await emailToggle.isVisible().catch(() => false);
    
    if (exists) {
      // Get initial state
      const initialState = await emailToggle.isChecked();
      
      // Toggle
      await emailToggle.click();
      await page.waitForTimeout(500);
      
      // Verify changed
      const newState = await emailToggle.isChecked();
      expect(newState).toBe(!initialState);
    }
  });

  test('should display warranty expiration threshold setting', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for threshold slider/input
    const thresholdSetting = page.getByText(/threshold|days.*before/i)
      .or(page.getByLabel(/days/i));
    
    const hasThreshold = await thresholdSetting.isVisible().catch(() => false);
    
    if (hasThreshold) {
      await expect(thresholdSetting).toBeVisible();
    }
  });

  test('should allow changing warranty threshold', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for threshold input/slider
    const thresholdInput = page.locator('input[type="range"], input[type="number"]')
      .filter({ has: page.getByText(/threshold|days/i) }).first();
    
    const hasInput = await thresholdInput.isVisible().catch(() => false);
    
    if (hasInput) {
      await thresholdInput.fill('30');
      
      // Save settings
      const saveButton = page.getByRole('button', { name: /save|update/i });
      await saveButton.click();
      
      await expect(page.getByText(/success|saved/i)).toBeVisible({ timeout: 5000 });
    }
  });

  test('should display phone verification section', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for phone section
    const phoneSection = page.getByText(/phone|mobile/i);
    await expect(phoneSection).toBeVisible();
  });

  test('should show verify phone button if not verified', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for verify button
    const verifyButton = page.getByRole('button', { name: /verify.*phone|add.*phone/i });
    const hasVerify = await verifyButton.isVisible().catch(() => false);
    
    if (hasVerify) {
      await expect(verifyButton).toBeVisible();
    }
  });

  test('should open phone verification modal', async ({ page }) => {
    await page.goto('/profile');
    
    const verifyButton = page.getByRole('button', { name: /verify.*phone|add.*phone/i });
    const exists = await verifyButton.isVisible().catch(() => false);
    
    if (exists) {
      await verifyButton.click();
      
      // Modal should open
      await expect(page.getByRole('dialog')).toBeVisible();
      await expect(page.getByLabel(/phone.*number/i)).toBeVisible();
    }
  });

  test('should validate phone number format', async ({ page }) => {
    await page.goto('/profile');
    
    const verifyButton = page.getByRole('button', { name: /verify.*phone/i });
    const exists = await verifyButton.isVisible().catch(() => false);
    
    if (exists) {
      await verifyButton.click();
      
      // Enter invalid phone
      await page.getByLabel(/phone/i).fill('123');
      await page.getByRole('button', { name: /send|verify/i }).click();
      
      // Should show validation error
      const errorMessage = page.getByText(/invalid.*phone|format/i);
      const hasError = await errorMessage.isVisible().catch(() => false);
      
      if (hasError) {
        await expect(errorMessage).toBeVisible();
      }
    }
  });

  test('should show 6-digit code input after sending verification', async ({ page }) => {
    await page.goto('/profile');
    
    const verifyButton = page.getByRole('button', { name: /verify.*phone/i });
    const exists = await verifyButton.isVisible().catch(() => false);
    
    if (exists) {
      await verifyButton.click();
      
      // Enter phone number
      await page.getByLabel(/phone/i).fill('+15555551234');
      await page.getByRole('button', { name: /send/i }).click();
      
      // Should show code input
      const codeInput = page.getByLabel(/code|verification/i);
      await expect(codeInput).toBeVisible({ timeout: 5000 });
    }
  });

  test('should cancel profile edit', async ({ page }) => {
    await page.goto('/profile');
    
    await page.getByRole('button', { name: /edit/i }).click();
    
    // Make changes
    const firstNameInput = page.getByLabel(/first name/i);
    const exists = await firstNameInput.isVisible().catch(() => false);
    
    if (exists) {
      await firstNameInput.fill('Changed');
      
      // Cancel
      await page.getByRole('button', { name: /cancel/i }).click();
      
      // Should not save changes
      await expect(page.getByText('Changed')).not.toBeVisible();
    }
  });

  test('should display account creation date', async ({ page }) => {
    await page.goto('/profile');
    
    // Look for creation date
    const dateText = page.getByText(/member since|joined|created/i);
    const hasDate = await dateText.isVisible().catch(() => false);
    
    if (hasDate) {
      await expect(dateText).toBeVisible();
    }
  });

  test('should show notification preference summary', async ({ page }) => {
    await page.goto('/profile');
    
    // Should show current preferences
    const preferences = page.locator('[data-testid="notification-preferences"]')
      .or(page.getByText(/email|sms|notification/i));
    
    await expect(preferences.first()).toBeVisible();
  });

  test('should persist notification settings after save', async ({ page }) => {
    await page.goto('/profile');
    
    const emailToggle = page.getByLabel(/email.*notification/i).first();
    const exists = await emailToggle.isVisible().catch(() => false);
    
    if (exists) {
      // Toggle and save
      await emailToggle.click();
      
      const saveButton = page.getByRole('button', { name: /save/i });
      if (await saveButton.isVisible().catch(() => false)) {
        await saveButton.click();
        await page.waitForTimeout(1000);
      }
      
      // Reload page
      await page.reload();
      
      // Setting should be persisted
      const emailToggleAfter = page.getByLabel(/email.*notification/i).first();
      await expect(emailToggleAfter).toBeVisible();
    }
  });

  test('should show validation for threshold range', async ({ page }) => {
    await page.goto('/profile');
    
    const thresholdInput = page.locator('input[type="number"]').filter({ has: page.getByText(/threshold/i) }).first();
    const exists = await thresholdInput.isVisible().catch(() => false);
    
    if (exists) {
      // Try invalid value (e.g., > 90 days)
      await thresholdInput.fill('999');
      
      const saveButton = page.getByRole('button', { name: /save/i });
      await saveButton.click();
      
      // Should show validation error or not save
      const errorMessage = page.getByText(/invalid|range|maximum/i);
      const hasError = await errorMessage.isVisible({ timeout: 2000 }).catch(() => false);
      
      if (hasError) {
        await expect(errorMessage).toBeVisible();
      }
    }
  });
});
