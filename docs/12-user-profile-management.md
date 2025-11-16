# User Profile Management API

## Overview

The User Profile Management API provides comprehensive endpoints for users to manage their profile information, phone numbers, and notification preferences. All endpoints are authenticated and user-isolated, ensuring each user can only access and modify their own data.

## Features

- ✅ Get and update user profile (FirstName, LastName)
- ✅ Manage phone number for SMS notifications
- ✅ Configure notification preferences (channel, threshold, opt-out)
- ✅ User-specific notification thresholds (1-90 days)
- ✅ Flexible notification channels (None, Email, SMS, Both)
- ✅ Opt-out functionality for all notifications
- ✅ Phone number validation and masking
- ✅ Automatic preference validation
- ✅ Comprehensive error handling

## Notification Preferences

### Notification Channels

Users can choose how they want to receive warranty expiration notifications:

| Channel | Value | Description |
|---------|-------|-------------|
| **None** | 0 | No notifications (effectively opted out) |
| **EmailOnly** | 1 | Receive notifications via email only |
| **SmsOnly** | 2 | Receive notifications via SMS only (requires phone number) |
| **EmailAndSms** | 3 | Receive notifications via both channels (default) |

### Notification Threshold

Users can customize when they want to be notified before warranty expiration:
- **Range**: 1-90 days
- **Default**: 7 days
- **Example**: Set to 30 days to get notifications a month before expiration

### Opt-Out

Users can completely opt out of all notifications without deleting their account:
- Sets `OptOutOfNotifications = true`
- Overrides all other notification settings
- Can be re-enabled at any time

## API Endpoints

All endpoints require authentication (JWT Bearer token).

### 1. Get User Profile

**GET** `/api/userprofile`

Returns complete user profile including notification preferences.

**Request:**
```http
GET /api/userprofile
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "id": "user-id",
  "email": "user@example.com",
  "userName": "username",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+15551234567",
  "phoneNumberConfirmed": false,
  "emailConfirmed": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "lastLoginAt": "2024-11-16T12:00:00Z",
  "notificationPreferences": {
    "notificationChannel": "EmailAndSms",
    "notificationThresholdDays": 7,
    "optOutOfNotifications": false
  }
}
```

### 2. Update Profile

**PUT** `/api/userprofile`

Update user's first and last name.

**Request:**
```http
PUT /api/userprofile
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith"
}
```

**Response:** `200 OK`
```json
{
  "message": "Profile updated successfully"
}
```

**Validation:**
- FirstName: Max 50 characters (optional)
- LastName: Max 50 characters (optional)

### 3. Update Phone Number

**PUT** `/api/userprofile/phone`

Update or clear phone number for SMS notifications.

**Request:**
```http
PUT /api/userprofile/phone
Authorization: Bearer {token}
Content-Type: application/json

{
  "phoneNumber": "+15551234567"
}
```

**Response:** `200 OK`
```json
{
  "message": "Phone number updated successfully",
  "phoneNumber": "+15551234567",
  "phoneNumberConfirmed": false
}
```

**To Clear Phone Number:**
```json
{
  "phoneNumber": ""
}
```

**Validation:**
- Must be valid phone number format
- Max 20 characters
- Recommended format: E.164 (+1234567890)
- Setting phone number resets `phoneNumberConfirmed` to false

**Privacy:**
- Phone numbers are masked in logs (shows only last 4 digits)
- Example log: `User updated phone number to ****4567`

### 4. Get Notification Preferences

**GET** `/api/userprofile/preferences`

Get current notification preferences.

**Request:**
```http
GET /api/userprofile/preferences
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "notificationChannel": "EmailAndSms",
  "notificationThresholdDays": 7,
  "optOutOfNotifications": false
}
```

### 5. Update Notification Preferences

**PUT** `/api/userprofile/preferences`

Update notification channel, threshold, or opt-out status.

**Request:**
```http
PUT /api/userprofile/preferences
Authorization: Bearer {token}
Content-Type: application/json

{
  "notificationChannel": "EmailOnly",
  "notificationThresholdDays": 30,
  "optOutOfNotifications": false
}
```

**Response:** `200 OK`
```json
{
  "message": "Notification preferences updated successfully",
  "preferences": {
    "notificationChannel": "EmailOnly",
    "notificationThresholdDays": 30,
    "optOutOfNotifications": false
  }
}
```

**Validation:**
- `notificationChannel`: Must be None, EmailOnly, SmsOnly, or EmailAndSms
- `notificationThresholdDays`: Must be 1-90
- `optOutOfNotifications`: Boolean
- SMS channels require phone number configured

**Error Response (SMS without phone):** `400 Bad Request`
```json
{
  "message": "Cannot enable SMS notifications without a phone number configured",
  "hint": "Please add your phone number first via PUT /api/userprofile/phone"
}
```

### 6. Delete Account (Opt-Out)

**DELETE** `/api/userprofile`

Opt out of all notifications (soft delete).

**Request:**
```http
DELETE /api/userprofile
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "message": "Account deletion requested. You have been opted out of notifications.",
  "note": "To fully delete your account, please contact support."
}
```

**Note**: This endpoint currently only opts out of notifications. For GDPR compliance and full account deletion, implement proper data removal logic.

## Database Schema

### ApplicationUser Extensions

New fields added to `AspNetUsers` table:

```sql
ALTER TABLE AspNetUsers ADD
    NotificationChannel INT NOT NULL DEFAULT 3,        -- 0=None, 1=Email, 2=SMS, 3=Both
    NotificationThresholdDays INT NOT NULL DEFAULT 7,   -- 1-90 days
    OptOutOfNotifications BIT NOT NULL DEFAULT 0        -- true=opted out
```

### Migration

The migration `AddUserNotificationPreferences` adds these fields with default values:
- `NotificationChannel`: EmailAndSms (3)
- `NotificationThresholdDays`: 7 days
- `OptOutOfNotifications`: false

Existing users will automatically get default values.

## Integration with Notification System

The notification system respects user preferences:

### Preference Checks

1. **Opt-Out Check**: If `OptOutOfNotifications = true`, no notifications sent
2. **Channel Check**: Only sends via enabled channels (Email, SMS, or both)
3. **Threshold Check**: Only notifies if warranty expires within user's threshold
4. **Phone Validation**: SMS only sent if phone number configured

### Example Flow

```
User has warranty expiring in 15 days
User preference: NotificationThresholdDays = 30, Channel = EmailAndSms

1. Check: 15 days <= 30 days? YES → Continue
2. Check: OptOutOfNotifications? NO → Continue
3. Check: Channel includes Email? YES → Send email
4. Check: Channel includes SMS? YES → Check phone number
5. Check: Phone number configured? YES → Send SMS
```

### Example: Email Only

```
User preference: Channel = EmailOnly

1. Check: Channel includes Email? YES → Send email
2. Check: Channel includes SMS? NO → Skip SMS
```

## Usage Examples

### Frontend Integration

#### React Example

```javascript
// Fetch user profile
const fetchProfile = async () => {
  const response = await fetch('/api/userprofile', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  const profile = await response.json();
  return profile;
};

// Update notification preferences
const updatePreferences = async (preferences) => {
  const response = await fetch('/api/userprofile/preferences', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      notificationChannel: preferences.channel,
      notificationThresholdDays: preferences.threshold,
      optOutOfNotifications: preferences.optOut
    })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }
  
  return await response.json();
};

// Update phone number
const updatePhone = async (phoneNumber) => {
  const response = await fetch('/api/userprofile/phone', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ phoneNumber })
  });
  
  return await response.json();
};
```

#### Settings Page Component

```jsx
function NotificationSettings() {
  const [preferences, setPreferences] = useState({
    channel: 'EmailAndSms',
    threshold: 7,
    optOut: false
  });

  const handleSave = async () => {
    try {
      await updatePreferences(preferences);
      toast.success('Preferences saved!');
    } catch (error) {
      toast.error(error.message);
    }
  };

  return (
    <div>
      <h2>Notification Preferences</h2>
      
      <label>
        Notification Method:
        <select 
          value={preferences.channel}
          onChange={(e) => setPreferences({...preferences, channel: e.target.value})}
        >
          <option value="None">None (Disable notifications)</option>
          <option value="EmailOnly">Email Only</option>
          <option value="SmsOnly">SMS Only</option>
          <option value="EmailAndSms">Email and SMS</option>
        </select>
      </label>

      <label>
        Notify me when warranty expires in:
        <input 
          type="number" 
          min="1" 
          max="90"
          value={preferences.threshold}
          onChange={(e) => setPreferences({...preferences, threshold: parseInt(e.target.value)})}
        />
        days
      </label>

      <label>
        <input 
          type="checkbox"
          checked={preferences.optOut}
          onChange={(e) => setPreferences({...preferences, optOut: e.target.checked})}
        />
        Opt out of all notifications
      </label>

      <button onClick={handleSave}>Save Preferences</button>
    </div>
  );
}
```

## Testing

### Test Scenarios

#### 1. Update Profile
```bash
# Login
POST /api/auth/login
{
  "email": "test@example.com",
  "password": "Test123!"
}

# Get profile
GET /api/userprofile
Authorization: Bearer {token}

# Update profile
PUT /api/userprofile
{
  "firstName": "John",
  "lastName": "Updated"
}
```

#### 2. Configure SMS Notifications
```bash
# Add phone number
PUT /api/userprofile/phone
{
  "phoneNumber": "+15551234567"
}

# Enable SMS notifications
PUT /api/userprofile/preferences
{
  "notificationChannel": "SmsOnly",
  "notificationThresholdDays": 14,
  "optOutOfNotifications": false
}
```

#### 3. Test Preference Validation
```bash
# Try to enable SMS without phone number (should fail)
PUT /api/userprofile/preferences
{
  "notificationChannel": "SmsOnly",
  "notificationThresholdDays": 7,
  "optOutOfNotifications": false
}

# Expected: 400 Bad Request
# "Cannot enable SMS notifications without a phone number configured"
```

#### 4. Opt Out
```bash
# Opt out of notifications
PUT /api/userprofile/preferences
{
  "notificationChannel": "EmailAndSms",
  "notificationThresholdDays": 7,
  "optOutOfNotifications": true
}

# Or use the delete endpoint
DELETE /api/userprofile
```

### Verify Notification Behavior

1. Create receipt with warranty expiring in 15 days
2. Set user threshold to 30 days
3. Wait for notification check cycle
4. Verify notification is sent

5. Change threshold to 7 days
6. Wait for next check cycle
7. Verify notification is NOT sent (already past threshold)

## Security Considerations

### Authentication
- All endpoints require valid JWT token
- Users can only access/modify their own profile
- `GetUserId()` extracts user ID from token claims

### Validation
- Phone numbers validated with `[Phone]` attribute
- Notification channels validated against enum
- Threshold range validated (1-90 days)
- SMS channels require phone number

### Privacy
- Phone numbers masked in logs
- Only last 4 digits shown in logs
- Example: `+15551234567` logged as `****4567`

### Rate Limiting

Consider implementing rate limiting for profile updates:
```csharp
[RateLimit(PermitLimit = 10, Window = 60)] // 10 requests per minute
public async Task<ActionResult> UpdatePreferences(...)
```

## Migration Guide

### For Existing Users

When deploying this feature to production with existing users:

1. **Migration runs automatically** on app startup
2. **Default values applied**:
   - NotificationChannel: EmailAndSms
   - NotificationThresholdDays: 7
   - OptOutOfNotifications: false
3. **Existing behavior preserved**: Users get notifications as before
4. **Users can customize** at any time via the API

### Rollback Plan

If issues occur, you can roll back the migration:

```bash
cd MyApi
dotnet ef database update PreviousMigrationName
```

Then remove the migration:
```bash
dotnet ef migrations remove
```

## Troubleshooting

### Issue: Cannot enable SMS notifications

**Error**: "Cannot enable SMS notifications without a phone number configured"

**Solution**: Add phone number first:
```bash
PUT /api/userprofile/phone
{ "phoneNumber": "+15551234567" }
```

### Issue: Invalid notification channel

**Error**: "Invalid notification channel"

**Solution**: Use valid values:
- None
- EmailOnly
- SmsOnly
- EmailAndSms

(Case-insensitive)

### Issue: Notifications not respecting preferences

**Check**:
1. User preferences saved correctly (`GET /api/userprofile/preferences`)
2. `OptOutOfNotifications` is false
3. Notification channel is not `None`
4. Warranty expires within user's threshold
5. Background service is running

### Issue: Phone number format invalid

**Solution**: Use E.164 format:
- Include country code: `+1` for US
- No spaces or dashes: `+15551234567`
- Not: `555-123-4567` or `(555) 123-4567`

## Future Enhancements

- [ ] Phone number verification (SMS code)
- [ ] Email verification required for notifications
- [ ] Notification history tracking
- [ ] Quiet hours (don't notify during sleep hours)
- [ ] Multiple notification thresholds (7, 14, 30 days)
- [ ] Custom notification message templates
- [ ] Notification frequency limits (max per day)
- [ ] Two-factor authentication
- [ ] Account activity log
- [ ] Export user data (GDPR compliance)
- [ ] Full account deletion workflow

## Related Documentation

- [Authentication & Authorization](04-authentication-authorization.md)
- [Warranty Expiration Notifications](10-warranty-expiration-notifications.md)
- [Email and SMS Notifications](11-email-sms-notifications.md)

---

**Note**: This API provides comprehensive user profile management with flexible notification preferences. Users have full control over how and when they receive warranty expiration notifications.
