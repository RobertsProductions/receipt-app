/**
 * Test Data Generators and Constants
 * 
 * Provides consistent test data across E2E tests with utility functions
 * for generating unique test data to avoid conflicts.
 */

export interface TestUser {
  email: string;
  password: string;
  username: string;
  firstName?: string;
  lastName?: string;
}

export interface TestReceipt {
  merchantName: string;
  totalAmount: number;
  purchaseDate: string;
  productDescription: string;
  warrantyMonths: number;
}

/**
 * Generate a unique email address for testing
 */
export function generateUniqueEmail(prefix = 'test'): string {
  const timestamp = Date.now();
  const random = Math.floor(Math.random() * 1000);
  return `${prefix}-${timestamp}-${random}@example.com`;
}

/**
 * Generate a unique username for testing
 */
export function generateUniqueUsername(prefix = 'user'): string {
  const timestamp = Date.now();
  const random = Math.floor(Math.random() * 1000);
  return `${prefix}${timestamp}${random}`;
}

/**
 * Generate a test user with unique credentials
 */
export function generateTestUser(overrides: Partial<TestUser> = {}): TestUser {
  return {
    email: generateUniqueEmail(),
    password: 'Test123!@#',
    username: generateUniqueUsername(),
    firstName: 'Test',
    lastName: 'User',
    ...overrides
  };
}

/**
 * Pre-defined test users for specific scenarios
 */
export const TEST_USERS = {
  valid: {
    password: 'Test123!@#'
  },
  invalid: {
    email: 'invalid@example.com',
    password: 'WrongPassword123!'
  },
  weak_password: {
    password: '123'
  }
};

/**
 * Generate test receipt data
 */
export function generateTestReceipt(overrides: Partial<TestReceipt> = {}): TestReceipt {
  return {
    merchantName: 'Test Store',
    totalAmount: 99.99,
    purchaseDate: new Date().toISOString().split('T')[0],
    productDescription: 'Test Product',
    warrantyMonths: 12,
    ...overrides
  };
}

/**
 * Sample receipts for different scenarios
 */
export const SAMPLE_RECEIPTS = {
  electronics: {
    merchantName: 'Best Electronics',
    totalAmount: 499.99,
    purchaseDate: '2024-01-15',
    productDescription: 'Laptop Computer',
    warrantyMonths: 24
  },
  appliance: {
    merchantName: 'Home Depot',
    totalAmount: 299.50,
    purchaseDate: '2024-06-20',
    productDescription: 'Refrigerator',
    warrantyMonths: 12
  },
  small_item: {
    merchantName: 'Office Supply Co',
    totalAmount: 19.99,
    purchaseDate: '2024-11-01',
    productDescription: 'Stapler',
    warrantyMonths: 6
  }
};

/**
 * Common form validation messages
 */
export const VALIDATION_MESSAGES = {
  required: {
    email: 'Email is required',
    password: 'Password is required',
    username: 'Username is required'
  },
  invalid: {
    email: 'Invalid email format',
    password_weak: 'Password must be at least 6 characters'
  }
};

/**
 * API response delays for testing (milliseconds)
 */
export const DELAYS = {
  short: 100,
  medium: 500,
  long: 1000,
  api_timeout: 5000
};
