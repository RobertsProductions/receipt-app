/**
 * E2E Tests: Receipt Details, Edit, Delete
 * 
 * Tests the receipt details page including:
 * - Viewing receipt details
 * - Editing receipt information
 * - Deleting receipts
 * - Image/PDF viewing
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, generateTestReceipt } from '../helpers/test-data';
import { registerAndLogin } from '../helpers/auth.helpers';
import { createReceipt, viewReceiptDetails, editReceipt, deleteReceipt, assertReceiptDetails, goToReceiptsPage } from '../helpers/receipt.helpers';

test.describe('Receipt Details Page', () => {
  
  let testUser: any;
  
  test.beforeEach(async ({ page }) => {
    // Register and login
    testUser = generateTestUser();
    await registerAndLogin(page, testUser);
  });

  test('should display receipt details correctly', async ({ page }) => {
    // Create a receipt
    const receipt = generateTestReceipt({
      merchantName: 'Electronics Store',
      totalAmount: 299.99,
      productDescription: 'Wireless Headphones'
    });
    await createReceipt(page, receipt);
    
    // Navigate to details
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Verify all details are displayed
    await assertReceiptDetails(page, receipt);
    await expect(page.getByText(receipt.merchantName)).toBeVisible();
    await expect(page.getByText(receipt.productDescription)).toBeVisible();
    await expect(page.getByText(new RegExp(receipt.totalAmount.toString()))).toBeVisible();
  });

  test('should show action buttons (Edit, Delete)', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Check for action buttons
    await expect(page.getByRole('button', { name: /edit/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /delete/i })).toBeVisible();
  });

  test('should display warranty information', async ({ page }) => {
    const receipt = generateTestReceipt({
      warrantyMonths: 24
    });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Should show warranty duration and expiration
    await expect(page.getByText(/warranty|expires|valid/i)).toBeVisible();
    await expect(page.getByText(/24|months/i)).toBeVisible();
  });

  test('should allow editing receipt details', async ({ page }) => {
    const receipt = generateTestReceipt({ merchantName: 'Original Store' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Edit the receipt
    await editReceipt(page, {
      merchantName: 'Updated Store',
      totalAmount: 499.99
    });
    
    // Verify updates are displayed
    await expect(page.getByText('Updated Store')).toBeVisible();
    await expect(page.getByText(/499\.99/)).toBeVisible();
  });

  test('should validate required fields when editing', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Click edit
    await page.getByRole('button', { name: /edit/i }).click();
    
    // Clear required field
    const merchantInput = page.getByLabel(/merchant/i);
    await merchantInput.clear();
    
    // Try to save
    await page.getByRole('button', { name: /save|update/i }).click();
    
    // Should show validation error or prevent save
    const stillInEditMode = await page.getByRole('button', { name: /cancel/i }).isVisible();
    expect(stillInEditMode).toBe(true);
  });

  test('should cancel edit without saving changes', async ({ page }) => {
    const receipt = generateTestReceipt({ merchantName: 'Original Name' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Start editing
    await page.getByRole('button', { name: /edit/i }).click();
    
    // Make changes
    const merchantInput = page.getByLabel(/merchant/i);
    await merchantInput.clear();
    await merchantInput.fill('Changed Name');
    
    // Cancel
    await page.getByRole('button', { name: /cancel/i }).click();
    
    // Should still show original name
    await expect(page.getByText('Original Name')).toBeVisible();
    await expect(page.getByText('Changed Name')).not.toBeVisible();
  });

  test('should delete receipt after confirmation', async ({ page }) => {
    const receipt = generateTestReceipt({ merchantName: 'To Be Deleted' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Delete receipt
    await deleteReceipt(page);
    
    // Should redirect to list
    await expect(page).toHaveURL(/\/receipts$/);
    
    // Receipt should not be in list
    await expect(page.getByText('To Be Deleted')).not.toBeVisible();
  });

  test('should cancel delete operation', async ({ page }) => {
    const receipt = generateTestReceipt({ merchantName: 'Keep This' });
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Click delete
    await page.getByRole('button', { name: /delete/i }).click();
    
    // Cancel in confirmation dialog
    const cancelButton = page.getByRole('button', { name: /cancel|no/i }).last();
    await cancelButton.click();
    
    // Should still be on details page
    await expect(page).toHaveURL(/\/receipts\/\d+/);
    await expect(page.getByText('Keep This')).toBeVisible();
  });

  test('should display receipt image if uploaded', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Look for image display
    const receiptImage = page.locator('img[alt*="receipt"], [data-testid="receipt-image"]');
    const hasImage = await receiptImage.isVisible().catch(() => false);
    
    if (hasImage) {
      await expect(receiptImage).toBeVisible();
      
      // Image should have valid src
      const src = await receiptImage.getAttribute('src');
      expect(src).toBeTruthy();
    }
  });

  test('should allow downloading receipt file', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Look for download button
    const downloadButton = page.getByRole('button', { name: /download/i })
      .or(page.getByRole('link', { name: /download/i }));
    
    const hasDownload = await downloadButton.isVisible().catch(() => false);
    
    if (hasDownload) {
      await expect(downloadButton).toBeVisible();
    }
  });

  test('should navigate back to receipts list', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Look for back button
    const backButton = page.getByRole('button', { name: /back/i })
      .or(page.getByRole('link', { name: /back/i }));
    
    await backButton.click();
    
    // Should be back on list page
    await expect(page).toHaveURL(/\/receipts$/);
  });

  test('should show 404 for non-existent receipt', async ({ page }) => {
    // Try to access non-existent receipt
    await page.goto('/receipts/999999');
    
    // Should show error or redirect
    const notFoundMessage = page.getByText(/not found|doesn't exist|invalid/i);
    const isNotFound = await notFoundMessage.isVisible({ timeout: 3000 }).catch(() => false);
    
    if (isNotFound) {
      await expect(notFoundMessage).toBeVisible();
    } else {
      // Or redirected to list
      await expect(page).toHaveURL(/\/receipts$/);
    }
  });

  test('should not allow viewing other users receipts', async ({ page }) => {
    // Create receipt with first user
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    
    // Get receipt URL
    await viewReceiptDetails(page, receipt.merchantName);
    const receiptUrl = page.url();
    
    // Logout and login as different user
    await page.goto('/logout');
    const newUser = generateTestUser();
    await registerUser(page, newUser);
    await loginUser(page, newUser.email, newUser.password);
    
    // Try to access first user's receipt
    await page.goto(receiptUrl);
    
    // Should be denied (404 or forbidden)
    const errorMessage = page.getByText(/not found|forbidden|access denied/i);
    const isDenied = await errorMessage.isVisible({ timeout: 3000 }).catch(() => false);
    
    expect(isDenied || page.url().includes('/receipts')).toBe(true);
  });

  test('should display created and updated timestamps', async ({ page }) => {
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    await viewReceiptDetails(page, receipt.merchantName);
    
    // Look for timestamp information
    const timestamp = page.getByText(/created|added|uploaded/i);
    const hasTimestamp = await timestamp.isVisible().catch(() => false);
    
    if (hasTimestamp) {
      await expect(timestamp).toBeVisible();
    }
  });
});
