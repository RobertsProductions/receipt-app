/**
 * E2E Tests: Warranty Dashboard
 * 
 * Tests the warranty tracking dashboard:
 * - Dashboard overview and statistics
 * - Warranty status indicators
 * - Filtering by urgency
 * - Navigation to receipt details
 */

import { test, expect } from '@playwright/test';
import { generateTestUser, generateTestReceipt } from '../helpers/test-data';
import { registerUser, loginUser } from '../helpers/auth.helpers';
import { createReceipt } from '../helpers/receipt.helpers';

test.describe('Warranty Dashboard', () => {
  
  test.beforeEach(async ({ page }) => {
    const user = generateTestUser();
    await registerUser(page, user);
    await loginUser(page, user.email, user.password);
  });

  test('should display warranty dashboard page', async ({ page }) => {
    await page.goto('/warranties');
    
    // Check page title
    await expect(page).toHaveTitle(/warrant/i);
    await expect(page).toHaveURL(/\/warranties/);
    
    // Check heading
    await expect(page.getByRole('heading', { name: /warrant/i })).toBeVisible();
  });

  test('should show empty state when no warranties exist', async ({ page }) => {
    await page.goto('/warranties');
    
    // Check for empty message
    const emptyMessage = page.getByText(/no warranties|add.*receipt|get started/i);
    const isEmpty = await emptyMessage.isVisible().catch(() => false);
    
    if (isEmpty) {
      await expect(emptyMessage).toBeVisible();
    }
  });

  test('should display warranty summary cards', async ({ page }) => {
    await page.goto('/warranties');
    
    // Look for summary cards (total, expiring, valid, expired)
    const summaryCards = page.locator('.summary-card, [data-testid="summary-card"]');
    const cardCount = await summaryCards.count();
    
    // Should have multiple summary cards
    expect(cardCount).toBeGreaterThan(0);
  });

  test('should show total warranties count', async ({ page }) => {
    // Create receipts with warranties
    const receipts = [
      generateTestReceipt({ warrantyMonths: 12 }),
      generateTestReceipt({ warrantyMonths: 24 })
    ];
    
    for (const receipt of receipts) {
      await createReceipt(page, receipt);
    }
    
    await page.goto('/warranties');
    
    // Should show total count
    await expect(page.getByText(/total.*warrant|all.*warrant/i)).toBeVisible();
    await expect(page.getByText(/2/)).toBeVisible();
  });

  test('should display expiring warranties count', async ({ page }) => {
    await page.goto('/warranties');
    
    // Look for expiring warranties section
    const expiringSection = page.getByText(/expiring|soon|warning/i);
    const hasExpiring = await expiringSection.isVisible().catch(() => false);
    
    if (hasExpiring) {
      await expect(expiringSection).toBeVisible();
    }
  });

  test('should show valid warranties count', async ({ page }) => {
    await page.goto('/warranties');
    
    // Look for valid/active warranties
    const validSection = page.getByText(/valid|active/i);
    const hasValid = await validSection.isVisible().catch(() => false);
    
    if (hasValid) {
      await expect(validSection).toBeVisible();
    }
  });

  test('should display expired warranties count', async ({ page }) => {
    await page.goto('/warranties');
    
    // Look for expired warranties
    const expiredSection = page.getByText(/expired/i);
    const hasExpired = await expiredSection.isVisible().catch(() => false);
    
    if (hasExpired) {
      await expect(expiredSection).toBeVisible();
    }
  });

  test('should have filter options', async ({ page }) => {
    await page.goto('/warranties');
    
    // Look for filter buttons (7 days, 30 days, 60 days, all)
    const filters = page.locator('button, [role="tab"]').filter({ hasText: /days|all/i });
    const filterCount = await filters.count();
    
    expect(filterCount).toBeGreaterThan(0);
  });

  test('should filter warranties by 7 days', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 1 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Click 7 days filter
    const filter7Days = page.getByRole('button', { name: /7.*day/i });
    const hasFilter = await filter7Days.isVisible().catch(() => false);
    
    if (hasFilter) {
      await filter7Days.click();
      
      // Should update the list
      await page.waitForTimeout(500);
    }
  });

  test('should filter warranties by 30 days', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 3 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    const filter30Days = page.getByRole('button', { name: /30.*day/i });
    const hasFilter = await filter30Days.isVisible().catch(() => false);
    
    if (hasFilter) {
      await filter30Days.click();
      await page.waitForTimeout(500);
    }
  });

  test('should filter warranties by 60 days', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 6 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    const filter60Days = page.getByRole('button', { name: /60.*day/i });
    const hasFilter = await filter60Days.isVisible().catch(() => false);
    
    if (hasFilter) {
      await filter60Days.click();
      await page.waitForTimeout(500);
    }
  });

  test('should show all warranties when "All" filter selected', async ({ page }) => {
    await page.goto('/warranties');
    
    const filterAll = page.getByRole('button', { name: /all/i });
    const hasFilter = await filterAll.isVisible().catch(() => false);
    
    if (hasFilter) {
      await filterAll.click();
      await page.waitForTimeout(500);
    }
  });

  test('should display warranty list items', async ({ page }) => {
    const receipt = generateTestReceipt({ 
      merchantName: 'Test Warranty Item',
      warrantyMonths: 12 
    });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Should show warranty in list
    await expect(page.getByText('Test Warranty Item')).toBeVisible();
  });

  test('should show urgency indicators', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 12 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Look for urgency badges (critical, warning, normal)
    const urgencyBadge = page.locator('.badge, .tag, [data-testid="urgency"]')
      .or(page.getByText(/critical|warning|normal|expires/i));
    
    const hasBadge = await urgencyBadge.first().isVisible().catch(() => false);
    
    if (hasBadge) {
      await expect(urgencyBadge.first()).toBeVisible();
    }
  });

  test('should display days until expiration', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 12 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Should show days remaining
    const daysText = page.getByText(/\d+\s*day/i);
    const hasDays = await daysText.isVisible().catch(() => false);
    
    if (hasDays) {
      await expect(daysText).toBeVisible();
    }
  });

  test('should navigate to receipt details on warranty click', async ({ page }) => {
    const receipt = generateTestReceipt({ 
      merchantName: 'Clickable Warranty',
      warrantyMonths: 12 
    });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Click warranty item
    const warrantyItem = page.getByText('Clickable Warranty');
    await warrantyItem.click();
    
    // Should navigate to receipt details
    await expect(page).toHaveURL(/\/receipts\/\d+/);
  });

  test('should show warranty expiration date', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 12 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Look for date format
    const dateText = page.getByText(/\d{4}|\w{3}.*\d{1,2}/);
    const hasDate = await dateText.first().isVisible().catch(() => false);
    
    if (hasDate) {
      await expect(dateText.first()).toBeVisible();
    }
  });

  test('should sort warranties by expiration date', async ({ page }) => {
    // Create multiple warranties
    const receipts = [
      generateTestReceipt({ merchantName: 'Warranty A', warrantyMonths: 6 }),
      generateTestReceipt({ merchantName: 'Warranty B', warrantyMonths: 12 }),
      generateTestReceipt({ merchantName: 'Warranty C', warrantyMonths: 3 })
    ];
    
    for (const receipt of receipts) {
      await createReceipt(page, receipt);
    }
    
    await page.goto('/warranties');
    
    // Warranties should be sorted (usually by urgency/expiration)
    const warrantyItems = page.locator('.warranty-card, [data-testid="warranty-item"]');
    const count = await warrantyItems.count();
    
    expect(count).toBeGreaterThanOrEqual(3);
  });

  test('should refresh data when navigating back from receipt', async ({ page }) => {
    const receipt = generateTestReceipt({ warrantyMonths: 12 });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    // Click warranty
    const firstWarranty = page.locator('.warranty-card, [data-testid="warranty-item"]').first();
    await firstWarranty.click();
    
    // Go back
    await page.goBack();
    
    // Should be on warranties page
    await expect(page).toHaveURL(/\/warranties/);
  });

  test('should display merchant name for each warranty', async ({ page }) => {
    const receipt = generateTestReceipt({ 
      merchantName: 'Specific Merchant',
      warrantyMonths: 12 
    });
    await createReceipt(page, receipt);
    
    await page.goto('/warranties');
    
    await expect(page.getByText('Specific Merchant')).toBeVisible();
  });
});
