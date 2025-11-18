/**
 * Debug version of list test to see what's actually on the page
 */

import { test, expect } from '@playwright/test';
import { generateTestUser } from '../helpers/test-data';
import { registerAndLogin } from '../helpers/auth.helpers';
import { goToReceiptsPage } from '../helpers/receipt.helpers';

test('debug empty state test', async ({ page }) => {
  // Register and login
  const user = generateTestUser();
  await registerAndLogin(page, user);
  
  // Go to receipts
  await goToReceiptsPage(page);
  console.log('On receipts page');
  
  // Take screenshot
  await page.screenshot({ path: 'debug-receipts-empty.png', fullPage: true });
  
  // Get all text content
  const bodyText = await page.locator('body').textContent();
  console.log('Page text:', bodyText);
  
  // Check for various possible empty state messages
  const possibleMessages = [
    /no receipts/i,
    /add your first/i,
    /get started/i,
    /empty/i,
    /create.*receipt/i,
    /upload.*receipt/i
  ];
  
  for (const pattern of possibleMessages) {
    const found = await page.getByText(pattern).isVisible({ timeout: 1000 }).catch(() => false);
    console.log(`Pattern ${pattern}: ${found ? 'FOUND' : 'not found'}`);
  }
  
  // Check for upload button
  const uploadBtn = await page.getByRole('button', { name: /upload|add|create/i }).count();
  console.log(`Upload buttons found: ${uploadBtn}`);
  
  console.log('Debug test complete');
});
