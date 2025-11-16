# Phone Number Verification

This document describes the phone number verification feature that allows users to securely verify their phone numbers via SMS verification codes.

## Overview

The phone number verification system enables users to:
- Add a phone number to their profile
- Receive a 6-digit verification code via SMS
- Verify their phone number to enable SMS notifications
- Update or remove their phone number

## Architecture

### Components

1. **IPhoneVerificationService** - Service interface for phone verification
2. **PhoneVerificationService** - Implementation handling code generation, storage, and validation
3. **UserProfileController** - REST API endpoints for verification workflow
4. **SmsNotificationService** - Sends SMS messages via Twilio (or simulated)

### Verification Flow

```
User → Update Phone → Send Verification Code → Receive SMS → Submit Code → Verify → Confirmed
```

## API Endpoints

### 1. Update Phone Number

```http
PUT /api/userprofile/phone
Authorization: Bearer {token}
Content-Type: application/json

{
  "phoneNumber": "+1234567890"
}
```

**Response:**
```json
{
  "message": "Phone number updated successfully",
  "phoneNumber": "+1234567890",
  "phoneNumberConfirmed": false
}
```

**Notes:**
- Setting phone number automatically sets `PhoneNumberConfirmed` to `false`
- Clear phone number by sending `null` or empty string

### 2. Send Verification Code

```http
POST /api/userprofile/phone/verify/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "phoneNumber": "+1234567890"
}
```

**Response:**
```json
{
  "message": "Verification code sent successfully",
  "phoneNumber": "******7890",
  "expiresIn": "5 minutes"
}
```

**Notes:**
- Generates a 6-digit verification code
- Code expires after 5 minutes
- Only 3 verification attempts allowed per code
- Requires SMS notifications to be configured (Twilio)

### 3. Verify Phone Number

```http
POST /api/userprofile/phone/verify/confirm
Authorization: Bearer {token}
Content-Type: application/json

{
  "verificationCode": "123456"
}
```

**Success Response:**
```json
{
  "message": "Phone number verified successfully",
  "phoneNumber": "******7890",
  "phoneNumberConfirmed": true
}
```

**Error Response:**
```json
{
  "message": "Invalid or expired verification code",
  "hint": "Please request a new code if yours has expired or you've exceeded the maximum attempts (3)"
}
```

## Security Features

### Code Generation
- 6-digit random numeric code (100000-999999)
- Cryptographically secure random generation

### Expiration
- Codes expire after 5 minutes
- Automatic cleanup of expired codes

### Rate Limiting
- Maximum 3 verification attempts per code
- Code invalidated after max attempts

### Storage
- In-memory storage (development)
- Recommendation: Use Redis or database in production for:
  - Persistence across app restarts
  - Distributed system support
  - Better scalability

## Configuration

### SMS Provider (Twilio)

Add to `appsettings.json` or user secrets:

```json
{
  "Twilio": {
    "AccountSid": "your-account-sid",
    "AuthToken": "your-auth-token",
    "FromPhoneNumber": "+1234567890"
  }
}
```

**Setup via User Secrets:**
```powershell
cd MyApi
dotnet user-secrets set "Twilio:AccountSid" "your-sid"
dotnet user-secrets set "Twilio:AuthToken" "your-token"
dotnet user-secrets set "Twilio:FromPhoneNumber" "+1234567890"
```

**Using .NET Aspire:**
```powershell
.\ConfigureSms.ps1  # If created for Aspire integration
```

### SMS Message Format

```
Your warranty app verification code is: {CODE}. Valid for 5 minutes.
```

## Development Mode

When Twilio is not configured:
- SMS sending is simulated
- Verification codes are logged to console
- Verification flow still works for testing
- Check application logs for generated codes

## Production Considerations

### 1. Persistent Storage
Replace in-memory dictionary with:
- **Redis** - Fast, distributed cache
- **Database** - Verification codes table with expiration
- **Azure Cache** - Managed service

Example Redis implementation:
```csharp
public class RedisPhoneVerificationService : IPhoneVerificationService
{
    private readonly IDistributedCache _cache;
    
    public async Task<(bool Success, string Message)> SendVerificationCodeAsync(string userId, string phoneNumber)
    {
        var code = GenerateVerificationCode();
        var entry = new VerificationCodeEntry { ... };
        
        await _cache.SetStringAsync(
            $"phone-verify:{userId}",
            JsonSerializer.Serialize(entry),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
        );
        
        // Send SMS...
    }
}
```

### 2. Rate Limiting
Add rate limiting to prevent abuse:
```csharp
[RateLimit("phone-verify", 5, 3600)] // 5 requests per hour
[HttpPost("phone/verify/send")]
public async Task<ActionResult> SendPhoneVerification(...)
```

### 3. Phone Number Validation
Enhanced validation:
- E.164 format validation
- Country code validation
- Carrier lookup
- Phone number type (mobile vs landline)

### 4. Two-Factor Authentication
Extend for 2FA:
- Require phone verification for sensitive operations
- Add backup codes
- Support authenticator apps

### 5. Monitoring
Track metrics:
- Verification success rate
- Failed attempts
- SMS delivery failures
- Average verification time

## Integration with Notifications

Phone verification is required for SMS notifications:

```csharp
// User must verify phone before enabling SMS
if (channel == NotificationChannel.SmsOnly || channel == NotificationChannel.EmailAndSms)
{
    if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        return BadRequest("Phone number required for SMS notifications");
    
    if (!user.PhoneNumberConfirmed)
        return BadRequest("Phone number must be verified before enabling SMS notifications");
}
```

## Testing

### Manual Testing

1. **Add Phone Number:**
```bash
curl -X PUT https://localhost:7156/api/userprofile/phone \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber":"+1234567890"}'
```

2. **Request Verification Code:**
```bash
curl -X POST https://localhost:7156/api/userprofile/phone/verify/send \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"phoneNumber":"+1234567890"}'
```

3. **Check Logs for Code** (development mode):
```
[INF] SMS (simulated) sent to ****7890: Your warranty app verification code is: 123456. Valid for 5 minutes.
```

4. **Verify Code:**
```bash
curl -X POST https://localhost:7156/api/userprofile/phone/verify/confirm \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"verificationCode":"123456"}'
```

### Automated Testing

```csharp
[Fact]
public async Task VerifyPhone_WithValidCode_ShouldSucceed()
{
    // Arrange
    var userId = "test-user";
    var phoneNumber = "+1234567890";
    
    // Act
    var (success, _) = await _verificationService.SendVerificationCodeAsync(userId, phoneNumber);
    var code = GetGeneratedCode(); // Extract from logs or test interface
    var isValid = await _verificationService.VerifyCodeAsync(userId, code);
    
    // Assert
    Assert.True(success);
    Assert.True(isValid);
}
```

## Error Handling

| Error | HTTP Status | Resolution |
|-------|-------------|------------|
| Phone number already exists | 400 | Use a different phone number or clear existing one |
| SMS not configured | 400 | Configure Twilio credentials |
| Invalid verification code | 400 | Request a new code |
| Code expired | 400 | Request a new code |
| Max attempts exceeded | 400 | Wait before requesting a new code |
| No verification code found | 400 | Request verification code first |

## Future Enhancements

1. **Multi-Region SMS Providers** - Fallback providers for better delivery rates
2. **Voice Verification** - Call-based verification for accessibility
3. **International Support** - Better international number handling
4. **Analytics Dashboard** - Verification metrics and insights
5. **Fraud Detection** - Detect suspicious verification patterns
6. **WhatsApp Integration** - Alternative to SMS using WhatsApp Business API

## Related Documentation

- [11-email-sms-notifications.md](11-email-sms-notifications.md) - SMS notification configuration
- [12-user-profile-management.md](12-user-profile-management.md) - User profile management
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity) - Built-in phone number support

## Change Log

- **2025-11-16**: Initial implementation with SMS verification codes
