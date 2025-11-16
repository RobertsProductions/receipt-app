# Receipt Upload and Management Feature

## Overview

The receipt upload feature allows authenticated users to upload, store, and manage receipt images (JPG, PNG) and PDF documents. All receipts are stored in persistent storage and associated with warranty information.

## Features

- ✅ Upload receipt images (JPG, JPEG, PNG) or PDFs
- ✅ Store receipt metadata (merchant, amount, purchase date, warranty info)
- ✅ Automatic warranty expiration calculation
- ✅ Download original receipts
- ✅ List all user receipts with pagination
- ✅ Delete receipts and associated files
- ✅ User-isolated storage (users can only access their own receipts)

## API Endpoints

All endpoints require authentication (JWT Bearer token).

### 1. Upload Receipt

**POST** `/api/receipts/upload`

Upload a receipt file with optional metadata.

**Request:**
- Content-Type: `multipart/form-data`
- Authorization: `Bearer {token}`

**Form Fields:**
```json
{
  "File": "<file>",              // Required: JPG, JPEG, PNG, or PDF (max 10MB)
  "Description": "string",       // Optional: Description of the receipt
  "PurchaseDate": "2024-01-15",  // Optional: Date of purchase
  "Merchant": "string",          // Optional: Store/merchant name
  "Amount": 99.99,               // Optional: Purchase amount
  "ProductName": "string",       // Optional: Product name
  "WarrantyMonths": 12,          // Optional: Warranty period in months
  "Notes": "string"              // Optional: Additional notes
}
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "receipt-2024-01-15.pdf",
  "fileType": "application/pdf",
  "fileSizeBytes": 245678,
  "description": "Electronics purchase",
  "uploadedAt": "2024-01-15T10:30:00Z",
  "purchaseDate": "2024-01-15",
  "merchant": "Best Buy",
  "amount": 599.99,
  "productName": "Laptop",
  "warrantyMonths": 12,
  "warrantyExpirationDate": "2025-01-15",
  "notes": "Extended warranty included",
  "downloadUrl": "/api/receipts/3fa85f64-5717-4562-b3fc-2c963f66afa6/download"
}
```

### 2. Get Receipt by ID

**GET** `/api/receipts/{id}`

Retrieve metadata for a specific receipt.

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "receipt-2024-01-15.pdf",
  ...
}
```

### 3. List All Receipts

**GET** `/api/receipts?page=1&pageSize=20`

List all receipts for the authenticated user with pagination.

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)

**Response:** `200 OK`
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fileName": "receipt-2024-01-15.pdf",
    ...
  },
  ...
]
```

### 4. Download Receipt File

**GET** `/api/receipts/{id}/download`

Download the original receipt file.

**Response:** `200 OK`
- Content-Type: Original file type (image/jpeg, application/pdf, etc.)
- Content-Disposition: attachment with original filename

### 5. Delete Receipt

**DELETE** `/api/receipts/{id}`

Delete a receipt and its associated file.

**Response:** `204 No Content`

## File Storage

### Local File Storage

By default, receipts are stored in the local file system:

**Storage Structure:**
```
uploads/
  receipts/
    {userId}/
      {guid}.jpg
      {guid}.pdf
      ...
```

**Configuration (appsettings.json):**
```json
{
  "FileStorage": {
    "BasePath": "uploads/receipts"
  }
}
```

### Storage Path Customization

To change the storage location, update `appsettings.json` or set environment variable:

```bash
# Absolute path
export FileStorage__BasePath="/var/app/receipts"

# Relative path (from project root)
export FileStorage__BasePath="storage/receipts"
```

## File Validation

### Allowed File Types
- Images: `.jpg`, `.jpeg`, `.png`
- Documents: `.pdf`

### Size Limits
- Maximum file size: **10MB**
- Configurable in `ReceiptsController.cs` (`MaxFileSize` constant)

### Validation Rules
- File must not be empty
- File extension must be in allowed list
- File size must be under limit
- Content-Type is validated

## Database Schema

### Receipts Table

```sql
CREATE TABLE Receipts (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileType NVARCHAR(100) NOT NULL,
    FileSizeBytes BIGINT NOT NULL,
    StoragePath NVARCHAR(500) NOT NULL,
    Description NVARCHAR(500) NULL,
    UploadedAt DATETIME2 NOT NULL,
    PurchaseDate DATETIME2 NULL,
    Merchant NVARCHAR(200) NULL,
    Amount DECIMAL(18,2) NULL,
    ProductName NVARCHAR(200) NULL,
    WarrantyMonths INT NULL,
    WarrantyExpirationDate DATETIME2 NULL,
    Notes NVARCHAR(2000) NULL,
    LastModifiedAt DATETIME2 NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Receipts_UserId ON Receipts(UserId);
CREATE INDEX IX_Receipts_UploadedAt ON Receipts(UploadedAt);
```

## Usage Examples

### Upload Receipt with cURL

```bash
curl -X POST "https://localhost:5001/api/receipts/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "File=@/path/to/receipt.pdf" \
  -F "Description=Laptop purchase" \
  -F "PurchaseDate=2024-01-15" \
  -F "Merchant=Best Buy" \
  -F "Amount=599.99" \
  -F "ProductName=Dell XPS 15" \
  -F "WarrantyMonths=12" \
  -F "Notes=Extended warranty included"
```

### Upload Receipt with PowerShell

```powershell
$token = "YOUR_JWT_TOKEN"
$headers = @{ "Authorization" = "Bearer $token" }

$form = @{
    File = Get-Item "C:\path\to\receipt.pdf"
    Description = "Laptop purchase"
    PurchaseDate = "2024-01-15"
    Merchant = "Best Buy"
    Amount = 599.99
    ProductName = "Dell XPS 15"
    WarrantyMonths = 12
}

Invoke-RestMethod -Uri "https://localhost:5001/api/receipts/upload" `
    -Method Post `
    -Headers $headers `
    -Form $form
```

### List Receipts

```bash
curl -X GET "https://localhost:5001/api/receipts?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Download Receipt

```bash
curl -X GET "https://localhost:5001/api/receipts/{id}/download" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -o receipt.pdf
```

## Security Considerations

### Authentication & Authorization
- All endpoints require valid JWT token
- Users can only access their own receipts
- File paths are sanitized to prevent directory traversal

### File Upload Security
- File type validation (extension and MIME type)
- File size limits enforced
- Unique file names (GUID-based) prevent overwrites
- User-isolated directories

### Best Practices
- Store receipts in a location outside the web root
- Use antivirus scanning for uploaded files (future enhancement)
- Implement rate limiting for upload endpoints
- Consider encryption at rest for sensitive data

## Testing

### Using Swagger UI

1. Start the application:
   ```bash
   cd AppHost
   dotnet run
   ```

2. Navigate to Swagger UI (URL shown in console)

3. Authenticate:
   - Use `/api/auth/login` or `/api/auth/register`
   - Copy the JWT token from response
   - Click "Authorize" button
   - Enter: `Bearer {your-token}`

4. Test upload:
   - Expand `/api/receipts/upload`
   - Click "Try it out"
   - Upload a file and fill in metadata
   - Click "Execute"

### Manual Testing

```bash
# 1. Register a user
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# 2. Login
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
# Save the token from response

# 3. Upload receipt
curl -X POST "https://localhost:5001/api/receipts/upload" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "File=@receipt.pdf" \
  -F "Merchant=Test Store"

# 4. List receipts
curl -X GET "https://localhost:5001/api/receipts" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Troubleshooting

### Upload Fails with 413 Payload Too Large

Increase the request size limit in `Program.cs`:

```csharp
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
```

### File Not Found When Downloading

- Check that the storage directory exists
- Verify file permissions on the storage directory
- Check logs for the actual storage path being used
- Ensure the `FileStorage:BasePath` is configured correctly

### Receipts Not Showing After Upload

- Check database connection
- Verify migration was applied: `dotnet ef database update`
- Check logs for errors during save

## Future Enhancements

- [ ] Cloud storage support (Azure Blob Storage, AWS S3)
- [ ] OCR integration for automatic data extraction from receipts
- [ ] Receipt image thumbnails
- [ ] Search and filter receipts
- [ ] Warranty expiration notifications
- [ ] Receipt categories/tags
- [ ] Export receipts (PDF report, CSV)
- [ ] Receipt sharing between users
- [ ] Mobile app integration

## Related Documentation

- [04 - Authentication & Authorization](04-authentication-authorization.md)
- [06 - Docker Database Setup](06-docker-database-setup.md)
- [07 - Connection Fixes](07-connection-fixes.md)
