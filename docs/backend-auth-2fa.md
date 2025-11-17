# Two-Factor Authentication (2FA) Implementation

This document describes the two-factor authentication (2FA) feature implementation using TOTP (Time-based One-Time Password).

## Overview

Users can enable 2FA on their accounts using authenticator apps like Google Authenticator, Microsoft Authenticator, or Authy. Once enabled, users must provide both their password and a 6-digit code from their authenticator app to log in.

## Features

- **TOTP-based authentication**: Uses industry-standard Time-based One-Time Password algorithm
- **QR code setup**: Provides QR code URI for easy authenticator app setup
- **Recovery codes**: Generates 10 recovery codes for account recovery
- **Secure enable/disable**: Requires 2FA code verification to disable 2FA
- **Status endpoint**: Check if 2FA is enabled and how many recovery codes remain
- **Recovery code regeneration**: Generate new recovery codes if needed

## API Endpoints

### Enable 2FA (Step 1: Get Setup Info)

```http
POST /api/Auth/2fa/enable
Authorization: Bearer <your_token>
```

**Response:**
```json
{
  "sharedKey": "abcd efgh ijkl mnop qrst uvwx yz12 3456",
  "qrCodeUri": "otpauth://totp/WarrantyApp:user@example.com?secret=ABCDEFGHIJKLMNOPQRSTUVWXYZ123456&issuer=WarrantyApp&digits=6",
  "recoveryCodes": null
}
```

**Setup Steps:**
1. Call this endpoint to get the shared key and QR code URI
2. Open your authenticator app (Google Authenticator, Microsoft Authenticator, Authy, etc.)
3. Scan the QR code or manually enter the shared key
4. The app will start generating 6-digit codes that change every 30 seconds
5. Use one of these codes in the verify endpoint

### Verify and Complete 2FA Setup (Step 2)

```http
POST /api/Auth/2fa/verify
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "code": "123456"
}
```

**Response:**
```json
{
  "sharedKey": null,
  "qrCodeUri": null,
  "recoveryCodes": [
    "12345678",
    "23456789",
    "34567890",
    "45678901",
    "56789012",
    "67890123",
    "78901234",
    "89012345",
    "90123456",
    "01234567"
  ]
}
```

**Important:** Save the recovery codes in a secure location. They can be used to access your account if you lose your authenticator device.

### Check 2FA Status

```http
GET /api/Auth/2fa/status
Authorization: Bearer <your_token>
```

**Response:**
```json
{
  "is2FAEnabled": true,
  "recoveryCodesLeft": 10
}
```

### Login with 2FA

Once 2FA is enabled, use this endpoint instead of the regular login:

```http
POST /api/Auth/login/2fa
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123!",
  "twoFactorCode": "123456"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "email": "user@example.com",
  "username": "user",
  "expiresAt": "2025-11-16T13:00:00Z",
  "refreshTokenExpiresAt": "2025-11-23T12:00:00Z"
}
```

### Disable 2FA

```http
POST /api/Auth/2fa/disable
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "code": "123456"
}
```

**Response:**
```json
{
  "message": "2FA disabled successfully"
}
```

### Regenerate Recovery Codes

```http
POST /api/Auth/2fa/recovery-codes/regenerate
Authorization: Bearer <your_token>
Content-Type: application/json

{
  "code": "123456"
}
```

**Response:**
```json
{
  "recoveryCodes": [
    "12345678",
    "23456789",
    ...
  ]
}
```

## Security Features

1. **Code Verification Required**: All sensitive 2FA operations (disable, regenerate recovery codes) require a valid 2FA code
2. **Automatic Key Reset**: When disabling 2FA, the authenticator key is automatically reset
3. **Recovery Codes**: 10 single-use recovery codes are generated for account recovery
4. **Logging**: All 2FA operations are logged for security auditing

## Testing with Postman/Swagger

### Step-by-Step Testing

1. **Register and Login:**
   ```json
   POST /api/Auth/register
   {
     "username": "testuser",
     "email": "test@example.com",
     "password": "Test123!",
     "firstName": "Test",
     "lastName": "User"
   }
   ```
   Save the token from the response.

2. **Enable 2FA:**
   ```json
   POST /api/Auth/2fa/enable
   Authorization: Bearer <token>
   ```
   Copy the `sharedKey` from the response.

3. **Add to Authenticator App:**
   - Open Google Authenticator or similar app
   - Add account manually
   - Enter the account name: "WarrantyApp - test@example.com"
   - Enter the key: Use the `sharedKey` (remove spaces)
   - Ensure Type is "Time based"
   - Get the 6-digit code from the app

4. **Verify 2FA:**
   ```json
   POST /api/Auth/2fa/verify
   Authorization: Bearer <token>
   {
     "code": "123456"
   }
   ```
   Save the recovery codes!

5. **Test 2FA Login:**
   ```json
   POST /api/Auth/login/2fa
   {
     "email": "test@example.com",
     "password": "Test123!",
     "twoFactorCode": "123456"
   }
   ```

6. **Try Regular Login (Should Fail):**
   ```json
   POST /api/Auth/login
   {
     "email": "test@example.com",
     "password": "Test123!"
   }
   ```
   Should return error indicating 2FA is required.

## Authenticator Apps

Compatible authenticator apps include:

- **Google Authenticator** (iOS, Android)
- **Microsoft Authenticator** (iOS, Android)
- **Authy** (iOS, Android, Desktop)
- **1Password** (with TOTP support)
- **Bitwarden** (with premium subscription)

## Recovery Process

If a user loses their authenticator device:

1. **Using Recovery Codes:**
   - Users can use one of their 10 recovery codes in place of a 2FA code
   - Each recovery code can only be used once
   - After using recovery codes, user should disable and re-enable 2FA with a new device

2. **Administrator Reset:**
   - If recovery codes are lost, an administrator must manually disable 2FA
   - This requires database access or a dedicated admin endpoint (not yet implemented)

## Database Schema

2FA uses ASP.NET Core Identity's built-in fields:

- `TwoFactorEnabled` (bool): Whether 2FA is enabled
- `AuthenticatorKey` (string): The TOTP shared secret
- `RecoveryCodes` (stored as user tokens): Single-use recovery codes

No additional migrations are required.

## Security Best Practices

1. **Always save recovery codes**: Store them in a secure password manager
2. **Verify before disable**: Always require a valid 2FA code before disabling
3. **Monitor login attempts**: Log all 2FA-related operations
4. **Rate limiting**: Consider adding rate limiting to prevent brute force attacks on 2FA codes
5. **Backup authenticators**: Add the same account to multiple authenticator devices

## Common Issues

### Issue: "Invalid verification code"
**Solutions:**
- Ensure your device's time is synchronized (TOTP requires accurate time)
- Check that you're using the correct account in your authenticator app
- Make sure the code hasn't expired (codes change every 30 seconds)
- Remove spaces or dashes when entering the code

### Issue: Can't scan QR code
**Solutions:**
- Use the shared key instead (manual entry)
- Remove spaces from the shared key when entering manually
- Ensure the authenticator app supports TOTP (most do)

### Issue: Lost authenticator device
**Solutions:**
- Use a recovery code to log in
- Contact an administrator if recovery codes are also lost
- Always set up 2FA on multiple devices as backup

## Future Enhancements

Potential improvements for the 2FA system:

- [ ] SMS-based 2FA as an alternative (less secure than TOTP)
- [ ] Email-based 2FA as a fallback option
- [ ] Trusted devices (remember this device for 30 days)
- [ ] Admin endpoints to reset user 2FA
- [ ] Rate limiting on 2FA attempts
- [ ] Backup codes via email
- [ ] WebAuthn/FIDO2 support (hardware keys)

## References

- [RFC 6238 - TOTP](https://tools.ietf.org/html/rfc6238)
- [ASP.NET Core Identity 2FA](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-enable-qrcodes)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
