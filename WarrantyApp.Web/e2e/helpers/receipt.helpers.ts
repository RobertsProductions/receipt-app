/**
 * Receipt Management Helper Functions
 * 
 * Reusable functions for receipt-related E2E tests.
 */

import { Page, expect } from '@playwright/test';
import { TestReceipt } from './test-data';
import * as path from 'path';

/**
 * Navigate to receipts list page
 */
export async function goToReceiptsPage(page: Page): Promise<void> {
  await page.goto('/receipts');
  await expect(page).toHaveURL(/\/receipts/);
}

/**
 * Upload a receipt file (image or PDF)
 */
export async function uploadReceipt(page: Page, filePath: string): Promise<void> {
  // Don't navigate - assume caller is already on receipts page or will navigate
  
  // Look for upload button or file input
  const fileInput = page.locator('input[type="file"]');
  
  // If file input is hidden, click the upload button first
  const uploadButton = page.getByRole('button', { name: /upload|add receipt/i });
  const isUploadButtonVisible = await uploadButton.isVisible({ timeout: 2000 }).catch(() => false);
  
  if (isUploadButtonVisible) {
    await uploadButton.click();
  }
  
  // Set file
  await fileInput.setInputFiles(filePath);
  
  // Wait for upload to complete (look for success message or receipt in list)
  await expect(page.getByText(/upload.*success|receipt.*added/i).or(page.locator('.receipt-card').first()))
    .toBeVisible({ timeout: 10000 });
}

/**
 * Create a receipt using the upload API with generated test data
 * This properly uploads via the API endpoint for realistic testing
 * Note: Caller should refresh/navigate after this to see the new receipt
 */
export async function createReceipt(page: Page, receipt: TestReceipt): Promise<void> {
  // Get auth token
  const token = await page.evaluate(() => localStorage.getItem('access_token'));
  
  if (!token) {
    throw new Error('No auth token found. User must be logged in.');
  }
  
  // Create a minimal 1x1 PNG file as FormData
  const base64Image = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==';
  const imageBuffer = Buffer.from(base64Image, 'base64');
  
  // Make API call via the Angular dev server proxy
  const response = await page.request.post('/api/receipts/upload', {
    headers: {
      'Authorization': `Bearer ${token}`
    },
    multipart: {
      File: {
        name: 'test-receipt.png',
        mimeType: 'image/png',
        buffer: imageBuffer
      },
      Merchant: receipt.merchantName,
      Amount: receipt.totalAmount.toString(),
      PurchaseDate: receipt.purchaseDate,
      ProductName: receipt.productDescription,
      WarrantyMonths: receipt.warrantyMonths.toString(),
      UseOcr: 'false'
    }
  });
  
  if (!response.ok()) {
    const error = await response.text();
    throw new Error(`Failed to create receipt via API: ${response.status()} - ${error}`);
  }
}

/**
 * Find receipt by merchant name in the list
 */
export async function findReceipt(page: Page, merchantName: string): Promise<any> {
  // Don't navigate - assume caller is already on receipts page
  
  // Look for receipt card with merchant name
  const receiptCard = page.locator('.receipt-card, [data-testid="receipt-card"]')
    .filter({ hasText: merchantName })
    .first();
  
  await expect(receiptCard).toBeVisible({ timeout: 5000 });
  return receiptCard;
}

/**
 * Click on a receipt to view details
 */
export async function viewReceiptDetails(page: Page, merchantName: string): Promise<void> {
  const receiptCard = await findReceipt(page, merchantName);
  await receiptCard.click();
  
  // Wait for details page
  await expect(page).toHaveURL(/\/receipts\/\d+/);
}

/**
 * Edit a receipt
 */
export async function editReceipt(page: Page, updates: Partial<TestReceipt>): Promise<void> {
  // Assumes we're already on receipt details page
  
  // Click edit button
  const editButton = page.getByRole('button', { name: /edit/i });
  await editButton.waitFor({ state: 'visible', timeout: 10000 });
  await editButton.click();
  
  // Update fields with explicit waits
  if (updates.merchantName) {
    const merchantInput = page.getByLabel(/merchant/i);
    await merchantInput.waitFor({ state: 'visible', timeout: 10000 });
    await merchantInput.clear();
    await merchantInput.fill(updates.merchantName);
  }
  
  if (updates.totalAmount) {
    const amountInput = page.getByLabel(/amount|total/i);
    await amountInput.waitFor({ state: 'visible', timeout: 10000 });
    await amountInput.clear();
    await amountInput.fill(updates.totalAmount.toString());
  }
  
  if (updates.productDescription) {
    const descInput = page.getByLabel(/product|description/i);
    await descInput.waitFor({ state: 'visible', timeout: 10000 });
    await descInput.clear();
    await descInput.fill(updates.productDescription);
  }
  
  // Save changes
  const saveButton = page.getByRole('button', { name: /save|update/i });
  await saveButton.waitFor({ state: 'visible', timeout: 10000 });
  await saveButton.click();
  
  // Wait for success
  await expect(page.getByText(/success|updated/i)).toBeVisible({ timeout: 10000 });
}

/**
 * Delete a receipt
 */
export async function deleteReceipt(page: Page): Promise<void> {
  // Assumes we're already on receipt details page
  
  // Click delete button
  await page.getByRole('button', { name: /delete/i }).click();
  
  // Confirm deletion in modal/dialog
  const confirmButton = page.getByRole('button', { name: /confirm|yes|delete/i }).last();
  await confirmButton.click();
  
  // Wait for redirect to list
  await expect(page).toHaveURL(/\/receipts$/);
}

/**
 * Check if receipts list is empty (has empty state message)
 */
export async function isReceiptsListEmpty(page: Page): Promise<boolean> {
  // Don't navigate - assume caller is already on receipts page
  
  // Look for empty state message with proper timeout
  const emptyMessage = page.getByText(/no receipts|add your first|get started/i);
  const isEmpty = await emptyMessage.isVisible({ timeout: 5000 }).catch(() => false);
  
  return isEmpty;
}

/**
 * Get count of receipts on current page
 */
export async function getReceiptCount(page: Page): Promise<number> {
  // Don't navigate - assume caller is already on receipts page
  
  const receiptCards = page.locator('.receipt-card, [data-testid="receipt-card"]');
  return await receiptCards.count();
}

/**
 * Navigate to next page in pagination
 */
export async function goToNextPage(page: Page): Promise<void> {
  const nextButton = page.getByRole('button', { name: /next/i })
    .or(page.locator('[aria-label="Next page"]'));
  
  await nextButton.click();
  
  // Wait for page to update
  await page.waitForTimeout(500);
}

/**
 * Navigate to previous page in pagination
 */
export async function goToPreviousPage(page: Page): Promise<void> {
  const prevButton = page.getByRole('button', { name: /previous|prev/i })
    .or(page.locator('[aria-label="Previous page"]'));
  
  await prevButton.click();
  
  // Wait for page to update
  await page.waitForTimeout(500);
}

/**
 * Filter receipts by search term
 */
export async function searchReceipts(page: Page, searchTerm: string): Promise<void> {
  // Don't navigate - assume caller is already on receipts page
  
  const searchInput = page.getByPlaceholder(/search/i)
    .or(page.getByLabel(/search/i));
  
  await searchInput.waitFor({ state: 'visible', timeout: 10000 });
  await searchInput.fill(searchTerm);
  
  // Wait for results to update
  await page.waitForTimeout(1000);
}

/**
 * Trigger OCR processing for a receipt
 */
export async function processReceiptWithOCR(page: Page): Promise<void> {
  // Assumes we're on receipt details page
  
  // Click OCR button
  await page.getByRole('button', { name: /process|ocr|extract/i }).click();
  
  // Wait for processing to complete
  await expect(page.getByText(/processing|analyzing/i)).toBeVisible({ timeout: 2000 });
  await expect(page.getByText(/complete|success/i)).toBeVisible({ timeout: 15000 });
}

/**
 * Create sample test image file
 */
export function getSampleImagePath(): string {
  // In real tests, you'd have actual test images
  // For now, return a path that tests can use
  return path.join(__dirname, '..', 'fixtures', 'sample-receipt.jpg');
}

/**
 * Create sample test PDF file
 */
export function getSamplePDFPath(): string {
  return path.join(__dirname, '..', 'fixtures', 'sample-receipt.pdf');
}

/**
 * Assert receipt details are displayed correctly
 */
export async function assertReceiptDetails(page: Page, receipt: Partial<TestReceipt>): Promise<void> {
  if (receipt.merchantName) {
    await expect(page.getByText(receipt.merchantName)).toBeVisible();
  }
  
  if (receipt.totalAmount) {
    await expect(page.getByText(new RegExp(receipt.totalAmount.toString()))).toBeVisible();
  }
  
  if (receipt.productDescription) {
    await expect(page.getByText(receipt.productDescription)).toBeVisible();
  }
}
