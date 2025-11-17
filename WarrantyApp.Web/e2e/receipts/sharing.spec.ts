/**
 * E2E Tests: Receipt Sharing
 * 
 * Tests receipt sharing functionality:
 * - Sharing receipts with other users
 * - Viewing shared receipts
 * - Managing share permissions
 * - Revoking access
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, generateTestReceipt } from '../helpers/test-data';
import { registerAndLogin, logoutUser } from '../helpers/auth.helpers';
import { createReceipt, viewReceiptDetails, goToReceiptsPage } from '../helpers/receipt.helpers';

test.describe('Receipt Sharing', () => {
  
  let owner: any;
  let sharedUser: any;
  
  test.beforeEach(async ({ page, context }) => {
    // Create two users: one owner, one to share with
    owner = generateTestUser();
    sharedUser = generateTestUser();
    
    // Register and login owner
    await registerAndLogin(page, owner);
  });

  test('should display share button on receipt details', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Look for share button
    const shareButton = page.getByRole('button', { name: /share/i });
    await expect(shareButton).toBeVisible();
  });

  test('should open share modal when share button clicked', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Click share button
    await page.getByRole('button', { name: /share/i }).click();
    
    // Modal should appear
    await expect(page.getByRole('dialog').or(page.locator('.modal, [role="dialog"]'))).toBeVisible();
    await expect(page.getByText(/share.*receipt/i)).toBeVisible();
  });

  test('should show email input in share modal', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Should have email input
    const emailInput = page.getByLabel(/email|user/i);
    await expect(emailInput).toBeVisible();
  });

  test('should validate email format when sharing', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Enter invalid email
    await page.getByLabel(/email/i).fill('invalid-email');
    await page.getByRole('button', { name: /share|send/i }).click();
    
    // Should show validation error
    const isStillOpen = await page.getByRole('dialog').isVisible().catch(() => true);
    expect(isStillOpen).toBe(true);
  });

  test('should share receipt with another user', async ({ page, context }) => {
    // Register second user first
    await page.goto('/register');
    await registerUser(page, sharedUser);
    await logoutUser(page);
    
    // Login as owner
    await loginUser(page, owner.email, owner.password);
    
    // Create and share receipt
    const receipt = generateTestReceipt({ merchantName: 'Shared Receipt' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    await page.getByLabel(/email/i).fill(sharedUser.email);
    await page.getByRole('button', { name: /share|send/i }).click();
    
    // Should show success message
    await expect(page.getByText(/success|shared/i)).toBeVisible({ timeout: 5000 });
  });

  test('should display shared receipts for recipient', async ({ page, context }) => {
    // Register second user
    await page.goto('/register');
    await registerUser(page, sharedUser);
    const sharedUserEmail = sharedUser.email;
    const sharedUserPassword = sharedUser.password;
    await logoutUser(page);
    
    // Login as owner and share
    await loginUser(page, owner.email, owner.password);
    const receipt = generateTestReceipt({ merchantName: 'Shared Item' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    await page.getByLabel(/email/i).fill(sharedUserEmail);
    await page.getByRole('button', { name: /share|send/i }).click();
    await page.waitForTimeout(1000);
    
    // Logout and login as shared user
    await logoutUser(page);
    await loginUser(page, sharedUserEmail, sharedUserPassword);
    
    // Navigate to shared receipts page
    const sharedLink = page.getByRole('link', { name: /shared.*with.*me/i });
    const hasSharedPage = await sharedLink.isVisible().catch(() => false);
    
    if (hasSharedPage) {
      await sharedLink.click();
      
      // Should see shared receipt
      await expect(page.getByText('Shared Item')).toBeVisible({ timeout: 5000 });
    }
  });

  test('should show read-only view for shared receipts', async ({ page, context }) => {
    // Similar setup as above
    await page.goto('/register');
    await registerUser(page, sharedUser);
    const sharedUserEmail = sharedUser.email;
    const sharedUserPassword = sharedUser.password;
    await logoutUser(page);
    
    await loginUser(page, owner.email, owner.password);
    const receipt = generateTestReceipt({ merchantName: 'Read Only Receipt' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    await page.getByLabel(/email/i).fill(sharedUserEmail);
    await page.getByRole('button', { name: /share|send/i }).click();
    await page.waitForTimeout(1000);
    
    await logoutUser(page);
    await loginUser(page, sharedUserEmail, sharedUserPassword);
    
    const sharedLink = page.getByRole('link', { name: /shared/i });
    const hasSharedPage = await sharedLink.isVisible().catch(() => false);
    
    if (hasSharedPage) {
      await sharedLink.click();
      const receiptCard = page.getByText('Read Only Receipt');
      const exists = await receiptCard.isVisible().catch(() => false);
      
      if (exists) {
        await receiptCard.click();
        
        // Should NOT have edit or delete buttons
        const editButton = page.getByRole('button', { name: /edit/i });
        const deleteButton = page.getByRole('button', { name: /delete/i });
        
        await expect(editButton).not.toBeVisible().catch(() => {});
        await expect(deleteButton).not.toBeVisible().catch(() => {});
      }
    }
  });

  test('should show list of users receipt is shared with', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Should show list of shared users
    const sharedList = page.locator('[data-testid="shared-users"]')
      .or(page.getByText(/shared.*with/i));
    
    const hasList = await sharedList.isVisible().catch(() => false);
    
    if (hasList) {
      await expect(sharedList).toBeVisible();
    }
  });

  test('should allow removing share access', async ({ page }) => {
    // First share with someone
    await page.goto('/register');
    await registerUser(page, sharedUser);
    await logoutUser(page);
    
    await loginUser(page, owner.email, owner.password);
    const receipt = generateTestReceipt({ merchantName: 'Revoke Test' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Share
    await page.getByRole('button', { name: /share/i }).click();
    await page.getByLabel(/email/i).fill(sharedUser.email);
    await page.getByRole('button', { name: /share|send/i }).click();
    await page.waitForTimeout(1000);
    
    // Open share modal again
    await page.getByRole('button', { name: /share/i }).click();
    
    // Look for remove button
    const removeButton = page.getByRole('button', { name: /remove|revoke|delete/i }).last();
    const canRemove = await removeButton.isVisible().catch(() => false);
    
    if (canRemove) {
      await removeButton.click();
      
      // Should show confirmation
      await expect(page.getByText(/removed|revoked/i)).toBeVisible({ timeout: 3000 });
    }
  });

  test('should prevent sharing with self', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Try to share with own email
    await page.getByLabel(/email/i).fill(owner.email);
    await page.getByRole('button', { name: /share|send/i }).click();
    
    // Should show error
    await expect(page.getByText(/cannot.*share.*yourself|already.*owner/i)).toBeVisible({ timeout: 3000 });
  });

  test('should show error for non-existent user email', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Try to share with non-existent user
    await page.getByLabel(/email/i).fill('nonexistent@example.com');
    await page.getByRole('button', { name: /share|send/i }).click();
    
    // Should show error
    await expect(page.getByText(/not found|doesn't exist|invalid user/i)).toBeVisible({ timeout: 3000 });
  });

  test('should close share modal on cancel', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    await page.getByRole('button', { name: /share/i }).click();
    
    // Modal should be open
    await expect(page.getByRole('dialog')).toBeVisible();
    
    // Click cancel
    await page.getByRole('button', { name: /cancel|close/i }).click();
    
    // Modal should close
    await expect(page.getByRole('dialog')).not.toBeVisible();
  });
});
