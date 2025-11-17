# Email and SMS Notifications

## Overview

The application supports sending warranty expiration notifications via email and SMS. Email addresses are retrieved from ASP.NET Core Identity (required for all users), and phone numbers are optional (using Identity's built-in `PhoneNumber` field).

## Features

- ✅ Email notifications with HTML formatting and urgency levels
- ✅ Optional SMS notifications (if user has phone number configured)
- ✅ Composite notification service (sends both email and SMS)
- ✅ Configurable SMTP settings for email (Gmail, Outlook, SendGrid, custom)
- ✅ Twilio integration for SMS (optional)
- ✅ Graceful degradation (works without SMS if not configured)
- ✅ Secure credential storage via user secrets
- ✅ No additional database fields required (uses Identity's Email and PhoneNumber)

## Notification Services

### 1. EmailNotificationService

Sends beautifully formatted HTML emails via SMTP.

**Features:**
- HTML email templates with urgency color coding
- Supports multiple SMTP providers (Gmail, Outlook, SendGrid, etc.)
- Configurable from/reply-to addresses
- SSL/TLS support
- Comprehensive error handling and logging

**Urgency Levels:**
- **URGENT** (Red): 3 days or less
- **Important** (Yellow): 4-7 days
- **Notice** (Blue): 8+ days

### 2. SmsNotificationService

Sends SMS notifications via Twilio (optional).

**Features:**
- Concise SMS messages optimized for character limits
- Phone number masking in logs for privacy
- Graceful handling when Twilio not configured
- Only sends if user has phone number in profile

**Note:** Currently logs SMS messages. To enable actual SMS sending:
1. Add Twilio NuGet package: `dotnet add package Twilio`
2. Uncomment the Twilio API code in `SmsNotificationService.cs`
3. Configure Twilio credentials

### 3. CompositeNotificationService

Combines email and SMS notifications.

**Features:**
- Fetches user's email and phone number from Identity
- Sends both notifications in parallel
- Continues if one notification type fails
- Logs all notification attempts

### 4. LogNotificationService

Original service that logs to console (useful for testing).

## Configuration

### Email (SMTP) Configuration

#### Option A: Interactive Script (Recommended)

```powershell
.\ConfigureEmail.ps1
```

The script will guide you through:
1. Choosing your email provider (Gmail, Outlook, SendGrid, custom)
2. Entering SMTP credentials
3. Configuring from/reply-to settings
4. Storing credentials securely in user secrets

#### Option B: Manual Configuration

Set user secrets manually:

```bash
cd MyApi
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:Username" "your-email@gmail.com"
dotnet user-secrets set "Smtp:Password" "your-app-password"
dotnet user-secrets set "Smtp:FromEmail" "noreply@yourapp.com"
dotnet user-secrets set "Smtp:FromName" "Warranty App"
dotnet user-secrets set "Smtp:UseSsl" "true"
```

#### Gmail Setup

1. Enable 2-factor authentication on your Google account
2. Create an App Password at https://myaccount.google.com/apppasswords
3. Use the App Password (16 characters, no spaces) as the SMTP password

#### SendGrid Setup

1. Create a SendGrid account and API key
2. Set:
   - Username: `apikey` (literal string)
   - Password: Your SendGrid API key

### SMS (Twilio) Configuration (Optional)

SMS notifications are optional. If Twilio is not configured, only email notifications will be sent.

```bash
cd MyApi
dotnet user-secrets set "Twilio:AccountSid" "your-account-sid"
dotnet user-secrets set "Twilio:AuthToken" "your-auth-token"
dotnet user-secrets set "Twilio:FromPhoneNumber" "+1234567890"
```

**To enable Twilio:**
1. Create Twilio account at https://www.twilio.com/
2. Get Account SID and Auth Token from dashboard
3. Purchase a phone number or use trial number
4. Add Twilio NuGet package: `dotnet add package Twilio`
5. Uncomment Twilio API code in `SmsNotificationService.cs`

### Choosing Notification Service

In `Program.cs`, choose ONE registration:

**Development/Testing (Logs only):**
```csharp
builder.Services.AddSingleton<INotificationService, LogNotificationService>();
```

**Email Only:**
```csharp
builder.Services.AddSingleton<EmailNotificationService>();
builder.Services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<EmailNotificationService>());
```

**Email + SMS (Recommended for Production):**
```csharp
builder.Services.AddSingleton<EmailNotificationService>();
builder.Services.AddSingleton<SmsNotificationService>();
builder.Services.AddScoped<INotificationService, CompositeNotificationService>();
```

## User Phone Number Management

Phone numbers are stored in ASP.NET Core Identity's built-in `PhoneNumber` field. No additional database migration needed!

### Adding Phone Number via API

Users can update their phone number through Identity's user management:

```csharp
// In a new UserProfileController
[HttpPut("profile/phone")]
public async Task<IActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneDto dto)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userManager.FindByIdAsync(userId);
    
    if (user == null)
        return NotFound();
    
    user.PhoneNumber = dto.PhoneNumber;
    var result = await _userManager.UpdateAsync(user);
    
    if (result.Succeeded)
        return Ok(new { message = "Phone number updated" });
    
    return BadRequest(result.Errors);
}
```

### Phone Number Format

- Recommended format: E.164 format (+1234567890)
- Twilio requires country code prefix
- Example: US number +1-555-123-4567 becomes +15551234567

## Testing

### Test Email Notifications

1. Configure SMTP settings (use `ConfigureEmail.ps1`)

2. Create a test receipt with warranty expiring soon:
```bash
POST /api/receipts/upload
Authorization: Bearer {token}

Form Data:
- File: receipt.jpg
- ProductName: "Test Product"
- PurchaseDate: "2024-11-16"
- WarrantyMonths: 1  # Expires in ~30 days
```

3. For faster testing, set `CheckIntervalHours` to `0.016667` (1 minute) in `appsettings.Development.json`

4. Wait for notification check cycle

5. Check your email inbox for warranty expiration notification

### Test SMS Notifications

1. Configure Twilio settings in user secrets

2. Update your phone number:
```bash
# Manually in database or via API
UPDATE AspNetUsers SET PhoneNumber = '+15551234567' WHERE Id = 'your-user-id'
```

3. Follow the same steps as email testing

4. Check your phone for SMS message

### Verify Configuration

```bash
cd MyApi
dotnet user-secrets list
```

Should show:
```
Smtp:Host = smtp.gmail.com
Smtp:Port = 587
Smtp:Username = your-email@gmail.com
Smtp:Password = ****************
... (other SMTP settings)
Twilio:AccountSid = AC***************
... (if configured)
```

## Email Template

The email notification includes:

- **Header** with urgency level and color coding
- **Large countdown** showing days until expiration
- **Expiration date** prominently displayed
- **Action items** checklist (review warranty, contact manufacturer, etc.)
- **Receipt ID** for reference
- **Professional branding** with app name and copyright

## Troubleshooting

### Email Not Sending

**Check:**
1. SMTP credentials are correct in user secrets
2. Gmail users: Using App Password, not regular password
3. Firewall/antivirus not blocking SMTP port 587
4. Check application logs for specific error messages
5. Test SMTP connection manually:
   ```bash
   telnet smtp.gmail.com 587
   ```

### SMS Not Sending

**Check:**
1. Twilio is configured in user secrets
2. Twilio NuGet package is installed
3. Twilio API code is uncommented in `SmsNotificationService.cs`
4. Phone number is in E.164 format (+1234567890)
5. User has phone number configured in profile
6. Check Twilio dashboard for delivery logs

### Receiving Error: "SMTP credentials not configured"

**Solution:**
Run `ConfigureEmail.ps1` script or manually set SMTP user secrets

### Gmail: "Username and Password not accepted"

**Solution:**
1. Enable 2-factor authentication on Google account
2. Generate an App Password (not regular password)
3. Use the 16-character App Password in SMTP configuration

### SendGrid: Connection issues

**Solution:**
1. Ensure username is exactly `apikey` (not your email)
2. Password should be your SendGrid API key (starts with `SG.`)
3. Check SendGrid dashboard for API key status

## Security Best Practices

1. **Never commit credentials** to source control
2. **Use user secrets** for development
3. **Use Azure Key Vault or similar** for production
4. **Rotate credentials** periodically
5. **Use App Passwords** instead of actual passwords where possible
6. **Enable 2FA** on email accounts
7. **Monitor email sending** for unusual activity
8. **Rate limit** notifications to prevent spam
9. **Mask phone numbers** in logs (already implemented)
10. **Use HTTPS** for all API endpoints

## Production Considerations

### Email Deliverability

1. **Use dedicated email service** (SendGrid, AWS SES, Mailgun) instead of Gmail
2. **Configure SPF, DKIM, DMARC** records for your domain
3. **Use verified sender domain** to avoid spam filters
4. **Monitor bounce rates** and email reputation
5. **Implement unsubscribe** functionality for compliance
6. **Add email verification** before sending notifications

### SMS Considerations

1. **Verify phone numbers** before sending SMS
2. **Implement opt-in/opt-out** for SMS notifications
3. **Monitor SMS costs** (Twilio charges per message)
4. **Consider SMS rate limits** and quotas
5. **Test in multiple countries** if supporting international numbers
6. **Handle Twilio errors** gracefully (invalid numbers, etc.)

### Notification Preferences

Consider adding user preferences:
- Notification frequency (immediate, daily digest, weekly)
- Notification channels (email only, SMS only, both)
- Threshold customization (notify 7/14/30 days before)
- Quiet hours (don't send notifications at night)

### Scaling

For high-volume applications:
- **Use message queue** (Azure Service Bus, RabbitMQ) for async sending
- **Implement retry logic** with exponential backoff
- **Batch notifications** to reduce API calls
- **Use dedicated worker** service for sending
- **Monitor send rates** and API limits

## Alternative Providers

### Email Providers

| Provider | Pros | Cons | Pricing |
|----------|------|------|---------|
| Gmail | Easy setup, free tier | Lower limits, not for production | Free (limited) |
| SendGrid | Reliable, great deliverability | Requires setup | 100 emails/day free |
| AWS SES | Scalable, cheap | Complex setup | $0.10/1000 emails |
| Mailgun | Easy API, good docs | Paid only | $0.80/1000 emails |
| Outlook 365 | Enterprise ready | Requires O365 | Included with O365 |

### SMS Providers

| Provider | Pros | Cons | Pricing |
|----------|------|------|---------|
| Twilio | Most popular, reliable | More expensive | ~$0.0075/SMS |
| AWS SNS | Cheap, scalable | Basic features | ~$0.00645/SMS |
| Vonage (Nexmo) | Good documentation | Limited features | ~$0.0073/SMS |
| Plivo | Competitive pricing | Less popular | ~$0.0040/SMS |

## Code Example: Custom Email Template

To customize the email template, edit `GenerateEmailBody()` in `EmailNotificationService.cs`:

```csharp
private string GenerateEmailBody(...)
{
    return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        /* Your custom styles */
        .urgent {{ background-color: #dc3545; }}
        .important {{ background-color: #ffc107; }}
    </style>
</head>
<body>
    <!-- Your custom HTML -->
    <h1>Warranty Alert</h1>
    <p>{productName} warranty expires in {daysUntilExpiration} days</p>
</body>
</html>";
}
```

## Support

For issues with email/SMS notifications:
1. Check application logs for error messages
2. Verify credentials with provider's test tools
3. Review provider's documentation and status pages
4. Check firewall and network settings
5. Test with provider's test numbers/addresses first

---

**Note**: This implementation provides production-ready email notifications and a foundation for SMS. Twilio integration requires adding the Twilio NuGet package and uncommenting the API code.
