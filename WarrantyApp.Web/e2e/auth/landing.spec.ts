import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test('should display landing page correctly', async ({ page }) => {
    await page.goto('/');
    
    // Check page title
    await expect(page).toHaveTitle(/Warranty/i);
    
    // Check hero section heading
    await expect(page.getByRole('heading', { name: /warranty management made simple/i })).toBeVisible();
    
    // Check navigation links (use first() to handle multiple instances)
    await expect(page.getByRole('link', { name: /login/i }).first()).toBeVisible();
    await expect(page.getByRole('link', { name: /sign up/i }).first()).toBeVisible();
  });

  test('should navigate to login page', async ({ page }) => {
    await page.goto('/');
    
    await page.getByRole('link', { name: /login/i }).first().click();
    
    await expect(page).toHaveURL('/login');
    await expect(page.getByRole('heading', { name: /welcome back/i })).toBeVisible();
  });

  test('should navigate to register page', async ({ page }) => {
    await page.goto('/');
    
    // Click the "Get Started Free" button
    await page.getByRole('button', { name: /get started/i }).first().click();
    
    await expect(page).toHaveURL('/register');
    await expect(page.getByRole('heading', { name: /create account/i })).toBeVisible();
  });
});
