# Receipt Sharing

## Overview

This document describes the receipt sharing functionality that allows users to share their receipts with other users for read-only access. Shared receipts are included in warranty expiration monitoring for recipients.

## Features

- **Share receipts** with other users by email or username
- **Read-only access** - recipients can view and download but cannot edit
- **View shared receipts** - see all receipts shared with you
- **Manage shares** - view who has access to your receipts
- **Revoke access** - remove sharing at any time
- **Automatic notifications** - recipients are notified via email/SMS when receipt is shared
- **Warranty monitoring** - recipients receive notifications for expiring warranties on shared receipts
- **Audit logging** - all sharing actions are logged for security

## Data Model

### ReceiptShare Entity

```csharp
public class ReceiptShare
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }          // The receipt being shared
    public string OwnerId { get; set; }           // User who owns the receipt
    public string SharedWithUserId { get; set; }  // User receiving access
    public DateTime SharedAt { get; set; }        // When it was shared
    public string? ShareNote { get; set; }        // Optional note from owner
}
```

### Database Schema

- **Table**: `ReceiptShares`
- **Indexes**:
  - Unique index on `(ReceiptId, SharedWithUserId)` to prevent duplicate shares
  - Index on `OwnerId` for efficient owner queries
  - Index on `SharedWithUserId` for efficient recipient queries
  - Index on `SharedAt` for sorting
- **Foreign Keys**:
  - `ReceiptId` → `Receipts.Id` (CASCADE delete)
  - `OwnerId` → `Users.Id` (NO ACTION)
  - `SharedWithUserId` → `Users.Id` (NO ACTION)

## API Endpoints

### Share a Receipt

**POST** `/api/ReceiptSharing/{receiptId}/share`

Shares a receipt with another user by email or username.

**Request:**
```json
{
  "shareWithIdentifier": "user@example.com",
  "shareNote": "Shared warranty for our home appliance"
}
```

**Response (200 OK):**
```json
{
  "shareId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiptFileName": "receipt.jpg",
  "sharedWithUserId": "user123",
  "sharedWithEmail": "user@example.com",
  "sharedAt": "2025-11-16T19:00:00Z",
  "shareNote": "Shared warranty for our home appliance"
}
```

**Error Responses:**
- `400 Bad Request` - User not found, already shared, or sharing with yourself
- `404 Not Found` - Receipt not found or you don't own it

### Get Receipts Shared With Me

**GET** `/api/ReceiptSharing/shared-with-me?page=1&pageSize=20`

Retrieves all receipts that have been shared with the current user.

**Response (200 OK):**
```json
[
  {
    "shareId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "receiptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "receipt.jpg",
    "description": "TV Purchase",
    "purchaseDate": "2024-01-15T00:00:00Z",
    "merchant": "Best Buy",
    "amount": 599.99,
    "productName": "Samsung 55\" TV",
    "warrantyMonths": 24,
    "warrantyExpirationDate": "2026-01-15T00:00:00Z",
    "notes": "Extended warranty included",
    "uploadedAt": "2024-01-15T10:30:00Z",
    "sharedAt": "2024-11-16T19:00:00Z",
    "shareNote": "Shared warranty for our TV",
    "ownerId": "owner123",
    "ownerEmail": "owner@example.com",
    "ownerName": "John Doe",
    "downloadUrl": "/api/ReceiptSharing/3fa85f64-5717-4562-b3fc-2c963f66afa6/download"
  }
]
```

**Query Parameters:**
- `page` (optional, default: 1) - Page number
- `pageSize` (optional, default: 20, max: 100) - Items per page

### Get Receipt Shares

**GET** `/api/ReceiptSharing/{receiptId}/shares`

Gets all users that a specific receipt is shared with (owner only).

**Response (200 OK):**
```json
[
  {
    "shareId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "sharedWithUserId": "user123",
    "sharedWithEmail": "user@example.com",
    "sharedWithName": "Jane Smith",
    "sharedAt": "2024-11-16T19:00:00Z",
    "shareNote": "Shared for reference"
  }
]
```

**Error Responses:**
- `404 Not Found` - Receipt not found or you don't own it

### Revoke Share

**DELETE** `/api/ReceiptSharing/{shareId}`

Revokes a receipt share, removing access for the recipient.

**Response:**
- `204 No Content` - Share successfully revoked
- `404 Not Found` - Share not found or you don't have permission

### Download Shared Receipt

**GET** `/api/ReceiptSharing/{shareId}/download`

Downloads a shared receipt file (read-only access).

**Response:**
- `200 OK` - File download with original filename
- `404 Not Found` - Share not found or you don't have access

### Get Shared Receipt Details

**GET** `/api/ReceiptSharing/{shareId}/details`

Gets detailed information about a specific shared receipt.

**Response (200 OK):**
```json
{
  "shareId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "receiptId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "receipt.jpg",
  "description": "TV Purchase",
  "purchaseDate": "2024-01-15T00:00:00Z",
  "merchant": "Best Buy",
  "amount": 599.99,
  "productName": "Samsung 55\" TV",
  "warrantyMonths": 24,
  "warrantyExpirationDate": "2026-01-15T00:00:00Z",
  "notes": "Extended warranty included",
  "uploadedAt": "2024-01-15T10:30:00Z",
  "sharedAt": "2024-11-16T19:00:00Z",
  "shareNote": "Shared warranty for our TV",
  "ownerId": "owner123",
  "ownerEmail": "owner@example.com",
  "ownerName": "John Doe",
  "downloadUrl": "/api/ReceiptSharing/3fa85f64-5717-4562-b3fc-2c963f66afa6/download"
}
```

## Automatic Notifications

When a receipt is shared, the recipient automatically receives a notification based on their notification preferences.

### Notification Channels

Recipients receive notifications through their configured channels:
- **Email Only** - HTML email with receipt details and share note
- **SMS Only** - Text message with owner name and receipt filename  
- **Email and SMS** - Both notification types
- **None** - No notifications (user opted out)

### Email Notification

The email notification includes:
- **Owner information** - Who shared the receipt
- **Receipt details** - Filename and ID
- **Share note** - Optional message from the owner
- **Action button** - Link to view the shared receipt (when UI is ready)
- **Access information** - Read-only access level

**Example Email:**
```
From: Warranty App <noreply@warrantyapp.com>
Subject: 📄 Receipt Shared With You: receipt.jpg

[Owner Name] has shared a receipt with you:
- Receipt: receipt.jpg
- Note: "Shared warranty for our TV"
- Access: Read-only (view and download)

[View Shared Receipt Button]
```

### SMS Notification

The SMS notification is brief and includes:
- Owner name
- Receipt filename
- Call to action

**Example SMS:**
```
📄 John Doe shared a receipt with you: 'TV_Receipt.jpg'. Check your Warranty App to view it.
```

### Notification Behavior

- **Respects user preferences** - Only sends via user's configured channels
- **Honors opt-out** - Users who opted out don't receive notifications
- **Requires contact info** - SMS requires verified phone number
- **Non-blocking** - Share succeeds even if notification fails
- **Logged** - All notification attempts are logged

Shared receipts are automatically included in the warranty expiration monitoring system:

- **Recipients receive notifications** for expiring warranties on shared receipts
- **Notification preferences respected** - recipients' notification settings are honored
- **Distinct notifications** - shared receipts are labeled with "[Shared]" prefix
- **Separate tracking** - notifications for shared receipts are tracked independently per user

### How It Works

1. The `WarrantyExpirationService` background service checks for expiring warranties
2. For each shared receipt with an expiring warranty:
   - Checks if the recipient has opted out of notifications
   - Respects the recipient's notification threshold preference
   - Sends notification with "[Shared]" prefix to indicate it's a shared item
   - Tracks notifications per user to avoid duplicates

### Example Notification

For a shared receipt, the notification will show:
```
Product: [Shared] Samsung 55" TV
Expiration: January 15, 2026
Days remaining: 7
```

## Security & Access Control

### Authorization Rules

1. **Sharing**: Only the receipt owner can share their receipts
2. **Viewing Shared**: Only the recipient can view/download shared receipts
3. **Revoking**: Only the receipt owner can revoke shares
4. **Listing Shares**: Only the receipt owner can see who has access

### Duplicate Prevention

- Unique index prevents the same receipt from being shared with the same user twice
- Attempting to share twice returns a `400 Bad Request` error

### Audit Logging

All sharing actions are logged with:
- User ID (owner and recipient)
- Receipt ID
- Action (share, revoke, download)
- Timestamp

Example log entry:
```
User abc123 shared receipt def456 with user xyz789
User xyz789 downloaded shared receipt def456 via share ghi012
User abc123 revoked share ghi012 for receipt def456
```

## Migration

### Creating the Database Table

Run the migration to create the `ReceiptShares` table:

```bash
cd MyApi
dotnet ef migrations add AddReceiptSharing
dotnet ef database update
```

### Migration Details

The migration creates:
- `ReceiptShares` table with all columns
- Foreign key constraints
- Indexes for performance
- Unique constraint on (ReceiptId, SharedWithUserId)

## Usage Examples

### Example 1: Share a Receipt

```bash
# Share a receipt with a user
curl -X POST https://localhost:7001/api/ReceiptSharing/abc-123/share \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "shareWithIdentifier": "friend@example.com",
    "shareNote": "Our TV warranty"
  }'
```

### Example 2: View Shared Receipts

```bash
# Get all receipts shared with me
curl https://localhost:7001/api/ReceiptSharing/shared-with-me \
  -H "Authorization: Bearer {token}"
```

### Example 3: Check Who Has Access

```bash
# See who has access to my receipt
curl https://localhost:7001/api/ReceiptSharing/abc-123/shares \
  -H "Authorization: Bearer {token}"
```

### Example 4: Revoke Access

```bash
# Remove access for a user
curl -X DELETE https://localhost:7001/api/ReceiptSharing/share-456 \
  -H "Authorization: Bearer {token}"
```

## Frontend Integration

### Sharing Modal

When implementing the UI, create a sharing modal with:
1. **Input field** for email/username
2. **Optional note** textarea
3. **Share button**
4. **List of current shares** with revoke buttons

### Shared Receipts View

Display shared receipts with:
- **Owner information** (name and email)
- **Share date**
- **Share note** (if provided)
- **Visual indicator** (e.g., shared icon badge)
- **Read-only mode** (no edit/delete buttons)

### Notifications

Show a badge or notification when:
- ✅ **New receipt is shared with you** (implemented - email/SMS)
- Shared receipt warranty is expiring (already implemented)
- Access to a shared receipt is revoked

## Best Practices

### For Receipt Owners

1. **Share with purpose** - Only share receipts that others genuinely need
2. **Add context** - Use the share note to explain why you're sharing
3. **Review regularly** - Periodically check who has access to your receipts
4. **Revoke when done** - Remove access when no longer needed

### For Recipients

1. **Respect privacy** - Don't download or share information unnecessarily
2. **Set preferences** - Configure your notification preferences for shared items
3. **Communicate** - Contact the owner if you have questions about shared receipts

## Troubleshooting

### "User not found" Error

- Verify the email address or username is correct
- Ensure the user has registered an account
- Check for typos in the identifier

### "Already shared" Error

- The receipt is already shared with this user
- Check the shares list with GET `/api/ReceiptSharing/{receiptId}/shares`
- Revoke the existing share if you need to update it

### Notification Not Received

1. Check recipient's notification preferences
2. Verify the warranty expiration date is within the threshold
3. Confirm the recipient hasn't opted out of notifications
4. Check logs for notification sending errors

### Download Not Working

- Verify the share ID is correct
- Ensure you're the authorized recipient
- Check that the original file still exists in storage
- Review logs for file access errors

## Related Documentation

- [08 - Receipt Upload Feature](08-receipt-upload-feature.md)
- [10 - Warranty Expiration Notifications](10-warranty-expiration-notifications.md)
- [11 - Email and SMS Notifications](11-email-sms-notifications.md)
- [12 - User Profile Management](12-user-profile-management.md)

## Future Enhancements

Potential improvements for future releases:

1. **Bulk Sharing** - Share multiple receipts at once
2. **Share Groups** - Share with predefined groups of users
3. **Expiring Shares** - Auto-revoke shares after a specified time
4. **Share Templates** - Save common sharing configurations
5. **Share Analytics** - Track who views/downloads shared receipts
6. **Comments** - Add discussion threads to shared receipts
7. **Revocation Notifications** - Notify when access is revoked
8. **In-app Notifications** - Real-time notifications in the UI

## Implementation Notes

### Notification Service Integration

The sharing feature integrates with the existing `INotificationService` infrastructure:

```csharp
public interface INotificationService
{
    Task SendWarrantyExpirationNotificationAsync(...);
    Task SendReceiptSharedNotificationAsync(
        string recipientUserId,
        string recipientEmail,
        string ownerName,
        string receiptFileName,
        Guid receiptId,
        string? shareNote);
}
```

All three notification service implementations support receipt sharing:
- `EmailNotificationService` - Sends HTML email with details
- `SmsNotificationService` - Sends brief SMS message
- `CompositeNotificationService` - Orchestrates both based on user preferences
- `LogNotificationService` - Logs notifications for development

### Error Handling

Notification failures don't prevent the share operation:
```csharp
try
{
    await _notificationService.SendReceiptSharedNotificationAsync(...);
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Failed to send notification, but share was created");
    // Share succeeds even if notification fails
}
```
