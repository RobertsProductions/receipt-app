/**
 * Simple diagnostic test for receipts
 */

import { test, expect } from '@playwright/test';
import { generateTestUser } from '../helpers/test-data';
import { registerAndLogin } from '../helpers/auth.helpers';

test('simple receipts access test', async ({ page }) => {
  // Register and login
  const user = generateTestUser();
  console.log('Registering user:', user.email);
  
  await registerAndLogin(page, user);
  console.log('registerAndLogin completed');
  
  // Check we're on receipts with token
  const currentUrl = page.url();
  console.log('Current URL:', currentUrl);
  
  const token = await page.evaluate(() => localStorage.getItem('access_token'));
  console.log('Token exists:', !!token);
  
  // Navigate to receipts explicitly
  await page.goto('/receipts');
  await page.waitForLoadState('networkidle');
  
  // Check what's on the page
  const pageTitle = await page.textContent('h1');
  console.log('Page heading:', pageTitle);
  
  // Verify we can see the receipts page heading
  await expect(page.getByRole('heading', { name: /my receipts/i })).toBeVisible({ timeout: 10000 });
  
  console.log('Test passed!');
});
