import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test('should display landing page correctly', async ({ page }) => {
    await page.goto('/');
    
    // Check page title
    await expect(page).toHaveTitle(/Warranty/i);
    
    // Check hero section
    await expect(page.getByRole('heading', { name: /manage your receipts/i })).toBeVisible();
    
    // Check navigation links
    await expect(page.getByRole('link', { name: /login/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /register/i })).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.goto('/');
    
    await page.getByRole('link', { name: /login/i }).first().click();
    
    await expect(page).toHaveURL('/login');
    await expect(page.getByRole('heading', { name: /login/i })).toBeVisible();
  });

  test('should navigate to register page', async ({ page }) => {
    await page.goto('/');
    
    await page.getByRole('link', { name: /get started/i }).first().click();
    
    await expect(page).toHaveURL('/register');
    await expect(page.getByRole('heading', { name: /create account/i })).toBeVisible();
  });
});
