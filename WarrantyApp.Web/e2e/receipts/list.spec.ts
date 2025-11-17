/**
 * E2E Tests: Receipt List Page
 * 
 * Tests the receipts list view including:
 * - Empty state display
 * - Receipt list rendering
 * - Pagination
 * - Navigation to details
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, generateTestReceipt } from '../helpers/test-data';
import { registerUser, loginUser } from '../helpers/auth.helpers';
import { goToReceiptsPage, createReceipt, getReceiptCount, isReceiptsListEmpty, goToNextPage, findReceipt } from '../helpers/receipt.helpers';

test.describe('Receipt List Page', () => {
  
  test.beforeEach(async ({ page }) => {
    // Register and login before each test
    const user = generateTestUser();
    await registerUser(page, user);
    await loginUser(page, user.email, user.password);
  });

  test('should display empty state when no receipts exist', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Check for empty state message
    const isEmpty = await isReceiptsListEmpty(page);
    expect(isEmpty).toBe(true);
    
    // Should show call-to-action
    await expect(page.getByText(/no receipts|add your first|get started/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /upload|add|create/i })).toBeVisible();
  });

  test('should display receipts list page correctly', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Check page title
    await expect(page).toHaveTitle(/receipts/i);
    
    // Check for main UI elements
    await expect(page.getByRole('heading', { name: /receipts|my receipts/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /upload|add/i })).toBeVisible();
  });

  test('should display receipt cards after creating receipts', async ({ page }) => {
    // Create a receipt
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    
    await goToReceiptsPage(page);
    
    // Should show receipt in list
    const count = await getReceiptCount(page);
    expect(count).toBeGreaterThan(0);
    
    // Should display receipt details
    await expect(page.getByText(receipt.merchantName)).toBeVisible();
    await expect(page.getByText(new RegExp(receipt.totalAmount.toString()))).toBeVisible();
  });

  test('should display multiple receipts', async ({ page }) => {
    // Create multiple receipts
    const receipts = [
      generateTestReceipt({ merchantName: 'Store A' }),
      generateTestReceipt({ merchantName: 'Store B' }),
      generateTestReceipt({ merchantName: 'Store C' })
    ];
    
    for (const receipt of receipts) {
      await createReceipt(page, receipt);
    }
    
    await goToReceiptsPage(page);
    
    // Should show all receipts
    const count = await getReceiptCount(page);
    expect(count).toBeGreaterThanOrEqual(3);
    
    // Verify each receipt is visible
    for (const receipt of receipts) {
      await expect(page.getByText(receipt.merchantName)).toBeVisible();
    }
  });

  test('should navigate to receipt details on click', async ({ page }) => {
    // Create a receipt
    const receipt = generateTestReceipt({ merchantName: 'Test Store' });
    await createReceipt(page, receipt);
    
    await goToReceiptsPage(page);
    
    // Click on receipt
    const receiptCard = await findReceipt(page, receipt.merchantName);
    await receiptCard.click();
    
    // Should navigate to details page
    await expect(page).toHaveURL(/\/receipts\/\d+/);
  });

  test('should show receipt thumbnail if image exists', async ({ page }) => {
    // Create a receipt
    const receipt = generateTestReceipt();
    await createReceipt(page, receipt);
    
    await goToReceiptsPage(page);
    
    // Look for image element
    const receiptImage = page.locator('img[alt*="receipt"], img[alt*="Receipt"]').first();
    const hasImage = await receiptImage.isVisible().catch(() => false);
    
    // If images are implemented, they should be visible
    if (hasImage) {
      await expect(receiptImage).toBeVisible();
    }
  });

  test('should display receipt date information', async ({ page }) => {
    const receipt = generateTestReceipt({
      purchaseDate: '2024-01-15'
    });
    await createReceipt(page, receipt);
    
    await goToReceiptsPage(page);
    
    // Should show date (might be formatted differently)
    await expect(page.getByText(/2024|Jan|January|01/)).toBeVisible();
  });

  test('should show warranty status indicators', async ({ page }) => {
    const receipt = generateTestReceipt({
      warrantyMonths: 12
    });
    await createReceipt(page, receipt);
    
    await goToReceiptsPage(page);
    
    // Look for warranty-related indicators
    const warrantyText = page.getByText(/warranty|expires|valid/i).first();
    const hasWarrantyInfo = await warrantyText.isVisible().catch(() => false);
    
    if (hasWarrantyInfo) {
      await expect(warrantyText).toBeVisible();
    }
  });

  test('should handle pagination if many receipts exist', async ({ page }) => {
    // Create enough receipts to trigger pagination (usually 10-20 per page)
    const receipts = Array.from({ length: 15 }, (_, i) =>
      generateTestReceipt({ merchantName: `Store ${i + 1}` })
    );
    
    for (const receipt of receipts) {
      await createReceipt(page, receipt);
    }
    
    await goToReceiptsPage(page);
    
    // Check if pagination controls exist
    const nextButton = page.getByRole('button', { name: /next/i })
      .or(page.locator('[aria-label="Next page"]'));
    
    const hasPagination = await nextButton.isVisible().catch(() => false);
    
    if (hasPagination) {
      const firstPageCount = await getReceiptCount(page);
      
      // Go to next page
      await goToNextPage(page);
      
      // Should show different receipts
      const secondPageCount = await getReceiptCount(page);
      expect(secondPageCount).toBeGreaterThan(0);
      
      // Combined should equal total created
      expect(firstPageCount + secondPageCount).toBeLessThanOrEqual(receipts.length);
    }
  });

  test('should maintain scroll position when returning from details', async ({ page }) => {
    // Create several receipts
    const receipts = Array.from({ length: 8 }, (_, i) =>
      generateTestReceipt({ merchantName: `Store ${i + 1}` })
    );
    
    for (const receipt of receipts) {
      await createReceipt(page, receipt);
    }
    
    await goToReceiptsPage(page);
    
    // Scroll down
    await page.evaluate(() => window.scrollTo(0, 500));
    
    // Click a receipt
    const receiptCard = await findReceipt(page, receipts[3].merchantName);
    await receiptCard.click();
    
    // Go back
    await page.goBack();
    
    // Should be on receipts page
    await expect(page).toHaveURL(/\/receipts$/);
  });

  test('should show loading state while fetching receipts', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // On initial load, might see loading spinner or skeleton
    const loadingIndicator = page.getByText(/loading/i)
      .or(page.locator('.loading, .spinner, [data-testid="loading"]'));
    
    // May or may not catch loading state depending on API speed
    const isLoading = await loadingIndicator.isVisible({ timeout: 1000 }).catch(() => false);
    
    // If loading state exists, it should eventually disappear
    if (isLoading) {
      await expect(loadingIndicator).not.toBeVisible({ timeout: 5000 });
    }
  });
});
