# Email Confirmation Implementation

This document describes the email confirmation feature that ensures users verify their email addresses during registration.

## Overview

Email confirmation is a security best practice that verifies user ownership of their email address. Upon registration, users receive an email with a confirmation link. While email confirmation is implemented, it is currently optional and does not block login. This can be changed to require confirmation before allowing login.

## Features

- **Automatic email sending**: Confirmation email sent immediately upon registration
- **Secure tokens**: Uses ASP.NET Core Identity's built-in token generation
- **Expiration**: Tokens automatically expire after 24 hours
- **Resend functionality**: Users can request a new confirmation email
- **Status check**: Endpoint to check if email is confirmed
- **HTML email templates**: Professional, responsive email design
- **Graceful degradation**: Registration succeeds even if email sending fails

## API Endpoints

### Register (with Email Confirmation)

```http
POST /api/Auth/register
Content-Type: application/json

{
  "username": "john doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "abc123...",
  "email": "john@example.com",
  "username": "johndoe",
  "expiresAt": "2025-11-16T14:00:00Z",
  "refreshTokenExpiresAt": "2025-11-23T13:00:00Z"
}
```

**Note:** A confirmation email is automatically sent to the provided email address. Check your inbox (and spam folder) for the confirmation link.

### Confirm Email

```http
GET /api/Auth/confirm-email?userId={userId}&token={token}
```

This endpoint is typically accessed by clicking the link in the confirmation email.

**Response (Success):**
```json
{
  "message": "Email confirmed successfully",
  "emailConfirmed": true
}
```

**Response (Already Confirmed):**
```json
{
  "message": "Email already confirmed",
  "emailConfirmed": true
}
```

**Response (Invalid/Expired Token):**
```json
{
  "message": "Email confirmation failed",
  "errors": [
    "Invalid token."
  ]
}
```

### Resend Confirmation Email

If the confirmation email was lost or expired:

```http
POST /api/Auth/resend-confirmation-email
Content-Type: application/json

{
  "email": "john@example.com"
}
```

**Response:**
```json
{
  "message": "If the email exists, a confirmation link has been sent"
}
```

**Note:** For security reasons, the response doesn't reveal whether the email exists in the system.

### Check Email Confirmation Status

```http
GET /api/Auth/email-status
Authorization: Bearer <your_token>
```

**Response:**
```json
{
  "email": "john@example.com",
  "emailConfirmed": true
}
```

## Email Template

The confirmation email includes:

- Professional HTML design with responsive layout
- Clear call-to-action button
- Plain text link as backup
- Expiration warning (24 hours)
- Security notice about ignoring if not requested
- Company branding

### Example Email Content

```
Subject: Confirm Your Email - Warranty App

Welcome to Warranty App!

Confirm Your Email Address

Hi there,

Thank you for registering with Warranty App. To complete your registration and start managing your warranties, please confirm your email address by clicking the button below:

[Confirm Email Address] (button)

Or copy and paste this link into your browser:
https://yourapp.com/api/Auth/confirm-email?userId=xxx&token=xxx

Important: This confirmation link will expire in 24 hours for security reasons.

If you didn't create an account with Warranty App, you can safely ignore this email.

© 2025 Warranty App. All rights reserved.
This is an automated email. Please do not reply to this message.
```

## Configuration

### App Settings

Add the base URL to `appsettings.json`:

```json
{
  "AppSettings": {
    "BaseUrl": "https://localhost:7156"
  }
}
```

For production, update this to your production URL:

```json
{
  "AppSettings": {
    "BaseUrl": "https://your-production-domain.com"
  }
}
```

### Email Settings

Ensure SMTP settings are configured in `appsettings.json`:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@yourapp.com",
    "FromName": "Warranty App",
    "UseSsl": true
  }
}
```

See [11-email-sms-notifications.md](11-email-sms-notifications.md) for detailed SMTP configuration instructions.

## Testing with Postman/Swagger

### Step-by-Step Testing

1. **Register a New User:**
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
   
2. **Check Your Email:**
   - Look for an email from "Warranty App"
   - Check spam folder if not in inbox
   - Click the confirmation link or copy the URL

3. **Confirm Email (Manual):**
   Extract `userId` and `token` from the link and call:
   ```
   GET /api/Auth/confirm-email?userId={userId}&token={token}
   ```

4. **Check Status:**
   ```
   GET /api/Auth/email-status
   Authorization: Bearer <your_token>
   ```

5. **Test Resend:**
   If needed, request a new confirmation email:
   ```json
   POST /api/Auth/resend-confirmation-email
   {
     "email": "test@example.com"
   }
   ```

## Security Features

1. **Secure Token Generation**: Uses ASP.NET Core Identity's built-in cryptographically secure token generation
2. **Token Expiration**: Tokens automatically expire after 24 hours
3. **URL Encoding**: Tokens are properly Base64Url encoded for safe URL transmission
4. **No Email Enumeration**: Resend endpoint doesn't reveal if email exists
5. **Idempotent Confirmation**: Confirming an already-confirmed email is safe
6. **Logging**: All confirmation attempts are logged for security auditing

## Optional: Require Email Confirmation for Login

To enforce email confirmation before allowing login, update `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Add this line
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

With this setting, users must confirm their email before they can log in.

## Database Schema

Email confirmation uses ASP.NET Core Identity's built-in fields:

- `EmailConfirmed` (bool): Whether the email has been verified
- `Email` (string): The user's email address
- `NormalizedEmail` (string): Uppercase version for lookups

No additional migrations are required.

## Common Issues

### Issue: Email not received
**Solutions:**
- Check spam/junk folder
- Verify SMTP settings are correct
- Check server logs for email sending errors
- Ensure firewall allows SMTP traffic
- Try resending the confirmation email
- For Gmail, ensure "Less secure app access" or App Password is configured

### Issue: Token expired
**Solutions:**
- Request a new confirmation email using the resend endpoint
- Tokens are valid for 24 hours by default

### Issue: Invalid token error
**Solutions:**
- Ensure the entire URL is copied correctly
- Don't manually modify the token
- Check if token has expired
- Request a new confirmation email

### Issue: Email already confirmed
**Solutions:**
- This is not an error - email was already verified
- User can proceed to login normally

## Development Tips

1. **Local Testing**: Use a service like [Mailtrap](https://mailtrap.io/) or [Ethereal Email](https://ethereal.email/) for development
2. **Email Preview**: Test email templates before sending to real users
3. **Monitoring**: Set up alerts for failed email sends
4. **Rate Limiting**: Consider adding rate limiting to prevent abuse of resend endpoint

## Future Enhancements

Potential improvements for the email confirmation system:

- [ ] Configurable token expiration time
- [ ] Magic link login (passwordless)
- [ ] Email change confirmation (verify new email)
- [ ] Welcome email after confirmation
- [ ] Reminder emails for unconfirmed accounts
- [ ] Admin panel to manually confirm emails
- [ ] Bulk email confirmation status export
- [ ] Email delivery status tracking

## References

- [ASP.NET Core Identity Email Confirmation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm)
- [Token Providers in ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-custom-storage-providers)
- [OWASP Email Verification](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#email-verification)
