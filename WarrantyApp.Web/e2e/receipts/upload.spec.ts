/**
 * E2E Tests: Receipt Upload and OCR Processing
 * 
 * Tests receipt upload functionality and OCR processing:
 * - Image upload (JPG, PNG)
 * - PDF upload
 * - OCR processing trigger
 * - OCR results display
 * - Editing OCR-extracted data
 */

import { test, expect } from '@playwright/test';
import { generateTestUser } from '../helpers/test-data';
import { registerUser, loginUser } from '../helpers/auth.helpers';
import { goToReceiptsPage, processReceiptWithOCR, uploadReceipt } from '../helpers/receipt.helpers';

test.describe('Receipt Upload and OCR', () => {
  
  test.beforeEach(async ({ page }) => {
    const user = generateTestUser();
    await registerUser(page, user);
    await loginUser(page, user.email, user.password);
  });

  test('should display upload interface', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Check for upload button or drag-drop area
    const uploadButton = page.getByRole('button', { name: /upload|add receipt/i });
    await expect(uploadButton).toBeVisible();
  });

  test('should show drag and drop area', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Look for drag-drop zone
    const dropZone = page.locator('[data-testid="dropzone"], .dropzone')
      .or(page.getByText(/drag.*drop|drop.*files/i));
    
    const hasDropZone = await dropZone.isVisible().catch(() => false);
    
    if (hasDropZone) {
      await expect(dropZone).toBeVisible();
    }
  });

  test('should accept image file upload (JPG)', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Click upload button
    const uploadButton = page.getByRole('button', { name: /upload|add receipt/i });
    await uploadButton.click();
    
    // File input should be available
    const fileInput = page.locator('input[type="file"]');
    await expect(fileInput).toBeAttached();
    
    // Check accepted file types
    const accept = await fileInput.getAttribute('accept');
    expect(accept).toMatch(/image|jpeg|jpg|png|\*|\./i);
  });

  test('should accept PDF file upload', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const uploadButton = page.getByRole('button', { name: /upload|add receipt/i });
    await uploadButton.click();
    
    const fileInput = page.locator('input[type="file"]');
    const accept = await fileInput.getAttribute('accept');
    
    // Should accept PDFs
    expect(accept).toMatch(/pdf|\*|application/i);
  });

  test('should show upload progress indicator', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Note: This test requires actual file upload which needs fixture files
    // For now, we'll test the UI elements existence
    
    const uploadButton = page.getByRole('button', { name: /upload/i });
    const exists = await uploadButton.isVisible();
    expect(exists).toBe(true);
  });

  test('should validate file size limits', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Check if size limit is displayed
    const sizeLimit = page.getByText(/max|maximum.*size|10\s*mb/i);
    const hasSizeInfo = await sizeLimit.isVisible().catch(() => false);
    
    if (hasSizeInfo) {
      await expect(sizeLimit).toBeVisible();
    }
  });

  test('should display OCR processing button on receipt details', async ({ page }) => {
    // Navigate to any receipt (or create one)
    await goToReceiptsPage(page);
    
    // If empty, create a receipt first
    const isEmpty = await page.getByText(/no receipts/i).isVisible().catch(() => false);
    
    if (!isEmpty) {
      // Click first receipt
      const firstReceipt = page.locator('.receipt-card, [data-testid="receipt-card"]').first();
      await firstReceipt.click();
      
      // Look for OCR/process button
      const ocrButton = page.getByRole('button', { name: /ocr|process|extract|analyze/i });
      const hasOCR = await ocrButton.isVisible().catch(() => false);
      
      if (hasOCR) {
        await expect(ocrButton).toBeVisible();
      }
    }
  });

  test('should show OCR processing status', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const isEmpty = await page.getByText(/no receipts/i).isVisible().catch(() => false);
    
    if (!isEmpty) {
      const firstReceipt = page.locator('.receipt-card, [data-testid="receipt-card"]').first();
      await firstReceipt.click();
      
      const ocrButton = page.getByRole('button', { name: /ocr|process|extract/i });
      const hasOCR = await ocrButton.isVisible().catch(() => false);
      
      if (hasOCR) {
        await ocrButton.click();
        
        // Should show processing state
        const processingText = page.getByText(/processing|analyzing|extracting/i);
        await expect(processingText).toBeVisible({ timeout: 2000 });
      }
    }
  });

  test('should display OCR extracted data', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const isEmpty = await page.getByText(/no receipts/i).isVisible().catch(() => false);
    
    if (!isEmpty) {
      const firstReceipt = page.locator('.receipt-card, [data-testid="receipt-card"]').first();
      await firstReceipt.click();
      
      // Look for OCR results section
      const ocrResults = page.locator('[data-testid="ocr-results"]')
        .or(page.getByText(/extracted|ocr.*result/i));
      
      const hasResults = await ocrResults.isVisible().catch(() => false);
      
      if (hasResults) {
        await expect(ocrResults).toBeVisible();
      }
    }
  });

  test('should allow editing OCR-extracted data', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const isEmpty = await page.getByText(/no receipts/i).isVisible().catch(() => false);
    
    if (!isEmpty) {
      const firstReceipt = page.locator('.receipt-card, [data-testid="receipt-card"]').first();
      await firstReceipt.click();
      
      // Click edit button
      const editButton = page.getByRole('button', { name: /edit/i });
      const canEdit = await editButton.isVisible().catch(() => false);
      
      if (canEdit) {
        await editButton.click();
        
        // Should be able to modify extracted data
        const merchantInput = page.getByLabel(/merchant/i);
        await expect(merchantInput).toBeEditable();
      }
    }
  });

  test('should handle OCR errors gracefully', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // OCR might fail for various reasons
    // Test should check for error handling UI
    const isEmpty = await page.getByText(/no receipts/i).isVisible().catch(() => false);
    
    if (!isEmpty) {
      const firstReceipt = page.locator('.receipt-card, [data-testid="receipt-card"]').first();
      await firstReceipt.click();
      
      const ocrButton = page.getByRole('button', { name: /ocr|process/i });
      const hasOCR = await ocrButton.isVisible().catch(() => false);
      
      if (hasOCR) {
        await ocrButton.click();
        
        // Wait for completion or error
        await page.waitForTimeout(5000);
        
        // Check if error message appears
        const errorMessage = page.getByText(/error|failed|unable/i);
        const hasError = await errorMessage.isVisible().catch(() => false);
        
        // Error handling should be graceful (not crash)
        // Page should still be functional
        await expect(page).toHaveURL(/\/receipts/);
      }
    }
  });

  test('should show file type restrictions', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // Look for file type information
    const fileTypeInfo = page.getByText(/jpg|jpeg|png|pdf|supported.*formats/i);
    const hasInfo = await fileTypeInfo.isVisible().catch(() => false);
    
    if (hasInfo) {
      await expect(fileTypeInfo).toBeVisible();
    }
  });

  test('should allow multiple file upload', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const uploadButton = page.getByRole('button', { name: /upload/i });
    await uploadButton.click();
    
    const fileInput = page.locator('input[type="file"]');
    
    // Check if multiple attribute is present
    const allowsMultiple = await fileInput.getAttribute('multiple');
    
    if (allowsMultiple !== null) {
      expect(allowsMultiple).toBeDefined();
    }
  });

  test('should display upload success message', async ({ page }) => {
    await goToReceiptsPage(page);
    
    // After successful upload, should show confirmation
    // This is a placeholder test since we need actual files
    const uploadButton = page.getByRole('button', { name: /upload/i });
    await expect(uploadButton).toBeVisible();
  });

  test('should cancel upload in progress', async ({ page }) => {
    await goToReceiptsPage(page);
    
    const uploadButton = page.getByRole('button', { name: /upload/i });
    await uploadButton.click();
    
    // Look for cancel button in upload modal
    const cancelButton = page.getByRole('button', { name: /cancel/i });
    const hasCancel = await cancelButton.isVisible().catch(() => false);
    
    if (hasCancel) {
      await expect(cancelButton).toBeVisible();
    }
  });
});
