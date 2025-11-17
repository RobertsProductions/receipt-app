# Login Error Interceptor Fix - Preventing Auth Controller Spam

**Date**: November 17, 2025  
**Status**: ✅ Resolved  
**Type**: Bug Fix

## Problem

When users entered incorrect login credentials, the application would spam the auth controller with repeated requests until the rate limiter middleware was triggered. This created a poor user experience and unnecessary server load.

## Root Cause

The issue was in the Angular error interceptor (`error.interceptor.ts`). The problematic flow was:

1. User enters incorrect credentials → API returns 401 Unauthorized
2. Error interceptor catches the 401
3. Interceptor calls `authService.logout()` which makes an HTTP request to `/api/Auth/logout`
4. Logout request fails with 401 (no valid token)
5. Error interceptor catches this 401 and calls logout again
6. **Infinite loop/cascade of requests** until rate limiter stops it

## Solution

Updated the error interceptor to prevent cascading logout calls:

### Key Changes

1. **Check if 401 came from an auth endpoint** - Skip automatic logout for login/register/refresh failures
2. **Only redirect if user is authenticated** - Failed login attempts shouldn't trigger logout
3. **Clear tokens locally without API call** - Prevents cascading HTTP requests

### Code Changes

**File**: `WarrantyApp.Web/src/app/interceptors/error.interceptor.ts`

**Before**:
```typescript
if (error.status === 401) {
  // Unauthorized - token expired or invalid
  authService.logout().subscribe(() => {
    router.navigate(['/login']);
  });
}
```

**After**:
```typescript
if (error.status === 401) {
  // Only trigger logout and redirect if this is not a login/auth endpoint
  // to avoid cascading logout calls on failed login attempts
  const isAuthEndpoint = req.url.includes('/Auth/login') || 
                         req.url.includes('/Auth/register') ||
                         req.url.includes('/Auth/refresh');
  
  if (!isAuthEndpoint && authService.isAuthenticated()) {
    // Clear local auth state without making API call to avoid loops
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    router.navigate(['/login']);
  }
}
```

## Testing

### Before Fix
- ❌ Incorrect login triggered 10+ rapid requests to auth controller
- ❌ Rate limiter middleware was hit
- ❌ Poor user experience with delayed error feedback

### After Fix
- ✅ Incorrect login makes only 1 request to login endpoint
- ✅ Clean error handling with immediate feedback
- ✅ No cascading requests
- ✅ Rate limiter is not triggered

## Impact

- **User Experience**: Clean, immediate error feedback on failed login
- **Server Load**: Eliminated unnecessary request spam
- **Security**: Rate limiter reserves capacity for actual threats
- **Maintainability**: More predictable error handling behavior

## Related Files

- `WarrantyApp.Web/src/app/interceptors/error.interceptor.ts` - Fixed interceptor
- `MyApi/Controllers/AuthController.cs` - Auth endpoint (no changes needed)

## Notes

This fix applies to all 401 responses throughout the application, not just login. The interceptor now properly distinguishes between:

1. **Auth endpoint failures** (login/register/refresh) - No automatic logout
2. **Authenticated endpoint failures** (expired token) - Clean local logout and redirect

This ensures proper behavior for both failed login attempts and expired token scenarios.

---

**Resolution**: The authentication flow now handles errors gracefully without creating request cascades.
