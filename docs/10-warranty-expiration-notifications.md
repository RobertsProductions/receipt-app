# Warranty Expiration Notification System

## Overview

The warranty expiration notification system is a background service that monitors receipts with warranty information and sends notifications to users when their warranties are about to expire. The system uses an in-memory cache for efficient access to upcoming expirations.

## Features

- ✅ Background service runs continuously to check warranty expirations
- ✅ Configurable check interval (default: 24 hours)
- ✅ Configurable notification threshold (default: 7 days before expiration)
- ✅ In-memory caching for fast API access
- ✅ Prevents duplicate notifications using cache
- ✅ RESTful API endpoints to query expiring warranties
- ✅ User-isolated notifications (users only see their own)
- ✅ Extensible notification system (currently logs, easily replaced with email/SMS)

## Architecture

### Components

1. **WarrantyExpirationService** (Background Service)
   - Runs as a hosted service in the ASP.NET Core application
   - Checks database periodically for expiring warranties
   - Updates cache with current expiring warranties
   - Tracks notified receipts to prevent duplicates
   - Sends notifications via INotificationService

2. **INotificationService** (Interface)
   - Abstraction for sending notifications
   - Allows easy swapping of notification implementations

3. **LogNotificationService** (Implementation)
   - Current implementation that logs notifications
   - Production-ready placeholder for email/SMS services

4. **WarrantyNotificationsController** (API)
   - Provides endpoints to query expiring warranties
   - Returns user-specific data from cache

5. **In-Memory Cache**
   - Stores current list of expiring warranties
   - Stores set of already-notified receipt IDs
   - Auto-expires to clean up old data

## Configuration

### appsettings.json

```json
{
  "WarrantyNotification": {
    "CheckIntervalHours": 24,
    "NotificationDaysThreshold": 7
  }
}
```

**CheckIntervalHours**: How often the service checks for expiring warranties (default: 24 hours)
**NotificationDaysThreshold**: Send notification if warranty expires within this many days (default: 7)

### Development Configuration

For testing, use a shorter interval in `appsettings.Development.json`:

```json
{
  "WarrantyNotification": {
    "CheckIntervalHours": 0.016667,
    "NotificationDaysThreshold": 7
  }
}
```

Note: 0.016667 hours = 1 minute (useful for testing)

## API Endpoints

All endpoints require authentication (JWT Bearer token).

### 1. Get Expiring Warranties

**GET** `/api/warrantynotifications/expiring`

Returns all warranties expiring soon for the current user, ordered by expiration date.

**Request:**
```http
GET /api/warrantynotifications/expiring
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
[
  {
    "userId": "user-id",
    "userEmail": "user@example.com",
    "productName": "Laptop Computer",
    "expirationDate": "2025-11-23T00:00:00Z",
    "receiptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "daysUntilExpiration": 7
  },
  {
    "userId": "user-id",
    "userEmail": "user@example.com",
    "productName": "Television",
    "expirationDate": "2025-11-20T00:00:00Z",
    "receiptId": "8bc12345-1234-1234-1234-123456789abc",
    "daysUntilExpiration": 4
  }
]
```

### 2. Get Expiring Warranties Count

**GET** `/api/warrantynotifications/expiring/count`

Returns the count of warranties expiring soon for the current user.

**Request:**
```http
GET /api/warrantynotifications/expiring/count
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "count": 2,
  "userId": "user-id"
}
```

## How It Works

### Service Lifecycle

1. **Startup**
   - Service starts 1 minute after application launch
   - Allows other services to initialize first

2. **Check Cycle**
   - Query database for receipts with `WarrantyExpirationDate` between today and threshold date
   - Filter out already-notified receipts from cache
   - Send notifications for new expiring warranties
   - Update cache with notification list and notified receipt IDs
   - Wait for configured interval before next check

3. **Notification Logic**
   - Receipt is included if: `today < expirationDate <= today + thresholdDays`
   - Each receipt is notified only once (tracked in cache)
   - Cache expires after 30 days to clean up old entries

4. **Cache Management**
   - Warranty list cached for 25 hours (slightly longer than check interval)
   - Notified receipts cached for 30 days
   - API endpoints read from cache for instant response

### Data Flow

```
┌─────────────────────────────────────────────────────────────┐
│                   Background Service                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  Every 24 hours (configurable)                       │  │
│  │                                                       │  │
│  │  1. Query receipts with expiring warranties          │  │
│  │  2. Check against notified cache                     │  │
│  │  3. Send new notifications via INotificationService  │  │
│  │  4. Update warranty cache                            │  │
│  │  5. Update notified receipts cache                   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │   Memory Cache   │
                    │                  │
                    │ - Warranty List  │
                    │ - Notified IDs   │
                    └──────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │   API Endpoint   │
                    │                  │
                    │ GET /expiring    │
                    └──────────────────┘
```

## Extending with Email/SMS

To add email or SMS notifications, create a new implementation of `INotificationService`:

### Example: Email Notification Service

```csharp
public class EmailNotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IEmailSender emailSender, ILogger<EmailNotificationService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendWarrantyExpirationNotificationAsync(
        string userId, 
        string userEmail, 
        string productName, 
        DateTime expirationDate, 
        Guid receiptId)
    {
        var daysUntilExpiration = (expirationDate.Date - DateTime.UtcNow.Date).Days;
        
        var subject = $"Warranty Expiring Soon: {productName}";
        var body = $@"
            <h2>Warranty Expiration Notice</h2>
            <p>Your warranty for <strong>{productName}</strong> will expire in <strong>{daysUntilExpiration} days</strong> on {expirationDate:yyyy-MM-dd}.</p>
            <p>Receipt ID: {receiptId}</p>
            <p>Please consider whether you need to renew or extend the warranty.</p>
        ";

        await _emailSender.SendEmailAsync(userEmail, subject, body);
        
        _logger.LogInformation("Sent email notification to {Email} for receipt {ReceiptId}", userEmail, receiptId);
    }
}
```

### Register in Program.cs

```csharp
// Replace LogNotificationService with EmailNotificationService
builder.Services.AddSingleton<INotificationService, EmailNotificationService>();
```

## Testing

### Setup Test Data

1. Create a receipt with a warranty expiring within 7 days:

```bash
POST /api/receipts/upload
Authorization: Bearer {token}

Form Data:
- File: receipt.jpg
- ProductName: "Test Product"
- PurchaseDate: "2024-11-16"
- WarrantyMonths: 1  # Expires 2024-12-16
```

2. Wait for the background service to run (1 minute in development, 24 hours in production)

3. Check logs for notification:
```
WARRANTY EXPIRATION NOTIFICATION: User user-123 (user@example.com) - Product 'Test Product' warranty expires in 7 days on 2024-12-16. Receipt ID: abc123...
```

4. Query the API:
```bash
GET /api/warrantynotifications/expiring
Authorization: Bearer {token}
```

### Development Testing (Fast Iteration)

Set `CheckIntervalHours` to a small value in `appsettings.Development.json`:

```json
{
  "WarrantyNotification": {
    "CheckIntervalHours": 0.016667,  // 1 minute
    "NotificationDaysThreshold": 30   // Increase for more test hits
  }
}
```

This allows you to test the notification system without waiting 24 hours.

## Monitoring

### Log Messages

The service logs at various levels:

**Information**
- Service start/stop
- Check cycle start/completion
- Notifications sent

**Warning**
- Actual notification messages (easy to spot)

**Debug**
- Cache updates
- Skipped notifications (already notified)

**Error**
- Exceptions during check cycle

### Key Metrics to Monitor

- Number of expiring warranties found per check
- Number of new notifications sent per check
- Cache size and hit rate
- Service uptime and check cycle timing

## Troubleshooting

### Issue: No notifications appearing

**Check:**
1. Background service is running (logs show startup message)
2. Receipts exist with `WarrantyExpirationDate` within threshold
3. `WarrantyExpirationDate` is correctly calculated (PurchaseDate + WarrantyMonths)
4. Check interval has passed (wait or reduce interval for testing)

### Issue: Duplicate notifications

**Check:**
1. Cache is not being cleared unexpectedly
2. Multiple instances of the app aren't running (distributed cache needed for multiple instances)

### Issue: API returns empty list

**Check:**
1. Background service has run at least once
2. Cache key is consistent
3. User authentication is working (correct user ID in token)
4. Receipts belong to the authenticated user

## Production Considerations

### Scalability

**Single Instance**
- Current implementation works well for single-instance deployments
- In-memory cache is sufficient

**Multiple Instances (Load Balanced)**
- Replace `IMemoryCache` with distributed cache (Redis, SQL Server, etc.)
- Ensure only one instance runs the background service (use distributed locking)
- Or run background service as a separate application/worker

### Email Rate Limiting

When implementing email notifications:
- Add rate limiting to prevent spam
- Batch notifications per user (daily digest)
- Implement retry logic for failed sends
- Track email delivery status

### Database Performance

- Index `WarrantyExpirationDate` column for faster queries
- Consider archiving old notifications
- Monitor query performance as data grows

### Notification Tracking

For production, consider storing notifications in database:
- Track when notification was sent
- Track delivery status (for email/SMS)
- Allow users to mark as read/dismissed
- Provide notification history

## Future Enhancements

- [ ] Email notification implementation
- [ ] SMS notification implementation
- [ ] User notification preferences (opt-in/opt-out, frequency)
- [ ] Push notifications for mobile apps
- [ ] Notification history in database
- [ ] Customizable notification templates
- [ ] Multiple notification thresholds (7 days, 30 days, etc.)
- [ ] Batch notifications (daily digest emails)
- [ ] Distributed cache for multi-instance deployments
- [ ] Admin dashboard for notification monitoring
- [ ] Notification analytics and reporting

## Example Integration

### Frontend Usage

```javascript
// React example - fetch expiring warranties
const fetchExpiringWarranties = async () => {
  const response = await fetch('/api/warrantynotifications/expiring', {
    headers: {
      'Authorization': `Bearer ${userToken}`
    }
  });
  
  const warranties = await response.json();
  
  // Show badge with count
  setBadgeCount(warranties.length);
  
  // Display warnings
  warranties.forEach(w => {
    if (w.daysUntilExpiration <= 3) {
      toast.error(`Warranty for ${w.productName} expires in ${w.daysUntilExpiration} days!`);
    } else {
      toast.warning(`Warranty for ${w.productName} expires in ${w.daysUntilExpiration} days`);
    }
  });
};

// Poll every 5 minutes for updates
useEffect(() => {
  fetchExpiringWarranties();
  const interval = setInterval(fetchExpiringWarranties, 5 * 60 * 1000);
  return () => clearInterval(interval);
}, []);
```

## Security Considerations

- Notifications contain user data - ensure proper authorization
- Cache keys should not be guessable
- Email/SMS credentials should be stored securely (user secrets, Azure Key Vault)
- Rate limit API endpoints to prevent abuse
- Validate user owns the receipts before sending notifications
- Consider GDPR/privacy requirements for notification data storage

---

**Note**: This system is designed to be production-ready but currently uses log-based notifications. Implement `INotificationService` with your preferred notification provider (SendGrid, Twilio, etc.) for actual email/SMS delivery.
