# OCR Feature - OpenAI Integration

## Overview

The receipt upload feature now includes OpenAI-powered OCR (Optical Character Recognition) to automatically extract information from receipt images, including merchant name, purchase amount, date, and product details.

## Features

- ✅ Automatic extraction of merchant, amount, purchase date, and product name
- ✅ Works with JPG, JPEG, and PNG image files
- ✅ Optional OCR during upload or on-demand for existing receipts
- ✅ Extracted data auto-fills empty fields while preserving manually entered data
- ✅ OCR extracted text appended to receipt notes

## Setup

### 1. Configure OpenAI API Key

The OCR service requires an OpenAI API key. There are three ways to configure it:

#### Option A: Via Aspire Dashboard (Recommended for Testing)

1. Start your Aspire application
2. Open the Aspire Dashboard (usually at https://localhost:17191)
3. Navigate to the Parameters section
4. Find `openai-apikey` parameter
5. Enter your OpenAI API key (starts with `sk-`)
6. Save and restart the API service

#### Option B: Via User Secrets (Recommended for Development)

```bash
cd MyAspireSolution/MyApi
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key-here"
```

#### Option C: Via AppHost User Secrets (For Aspire-managed Configuration)

```bash
cd MyAspireSolution/AppHost
dotnet user-secrets set "Parameters:openai-apikey" "your-openai-api-key-here"
```

**Important:** Never commit API keys to source control. User secrets and Aspire parameters are stored locally and not included in the repository.

### 2. Verify Configuration

The API will throw an error on startup if the OpenAI API key is not configured. Check the logs for:
```
OpenAI API key not configured. Set it in user secrets with: dotnet user-secrets set "OpenAI:ApiKey" "your-key"
```

## API Endpoints

### Upload Receipt with OCR

**POST** `/api/receipts/upload`

Add `useOcr=true` to the form data to enable automatic OCR during upload.

**Request:**
```
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form Fields:
- File: <image file> (required)
- UseOcr: true (optional, default: false)
- Description: "string" (optional)
- PurchaseDate: "2024-01-15" (optional - OCR will fill if empty)
- Merchant: "string" (optional - OCR will fill if empty)
- Amount: 99.99 (optional - OCR will fill if empty)
- ProductName: "string" (optional - OCR will fill if empty)
- WarrantyMonths: 12 (optional)
- Notes: "string" (optional)
```

**Example with cURL:**
```bash
curl -X POST "https://localhost:7156/api/receipts/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "File=@receipt.jpg" \
  -F "UseOcr=true" \
  -F "WarrantyMonths=12"
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "receipt.jpg",
  "merchant": "Best Buy",
  "amount": 599.99,
  "purchaseDate": "2024-01-15",
  "productName": "Laptop Computer",
  "notes": "OCR: Receipt shows purchase of laptop with warranty",
  "warrantyExpirationDate": "2025-01-15",
  ...
}
```

### Run OCR on Existing Receipt

**POST** `/api/receipts/{id}/ocr`

Run OCR analysis on a previously uploaded receipt image.

**Request:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "merchant": "Best Buy",
  "amount": 599.99,
  "purchaseDate": "2024-01-15",
  "productName": "Laptop Computer",
  "notes": "Original notes\n\nOCR: Receipt shows purchase of laptop with warranty",
  ...
}
```

**Error Responses:**
- `404 Not Found` - Receipt not found or file missing
- `400 Bad Request` - File is not an image (PDF not supported for OCR)
- `400 Bad Request` - OCR processing failed

## How It Works

1. **Image Processing**: Receipt image is converted to base64 and sent to OpenAI's GPT-4o-mini vision model
2. **Data Extraction**: The AI model extracts structured data (merchant, amount, date, product) from the receipt
3. **Smart Merging**: OCR results only fill empty fields - manually entered data is never overwritten
4. **Note Appending**: Raw extracted text is appended to the notes field for reference

## Model Used

- **Model**: `gpt-4o-mini` (GPT-4 Optimized Mini)
- **Temperature**: 0.1 (low temperature for consistent, factual extraction)
- **Max Tokens**: 500
- **Capabilities**: Vision + structured JSON output

## Limitations

- Only supports image files (JPG, JPEG, PNG)
- PDF files cannot be processed with OCR (requires different approach)
- Accuracy depends on receipt image quality and clarity
- Requires active internet connection to OpenAI API
- API usage incurs costs based on OpenAI pricing

## Cost Considerations

OpenAI API charges per token. The OCR feature uses:
- ~1000-2000 tokens per image (base64 encoding + response)
- GPT-4o-mini pricing (as of 2024): ~$0.00015 per image

Monitor your OpenAI usage at: https://platform.openai.com/usage

## Troubleshooting

### OCR Not Working

1. **Check API Key**: Verify the key is set in user secrets
   ```bash
   cd MyAspireSolution/MyApi
   dotnet user-secrets list
   ```

2. **Check Logs**: Look for OCR-related errors in application logs
   ```
   OCR failed: OpenAI API error: 401
   ```

3. **Test API Key**: Verify your key works with a simple cURL test
   ```bash
   curl https://api.openai.com/v1/models \
     -H "Authorization: Bearer YOUR_API_KEY"
   ```

### Poor Extraction Quality

- Ensure receipt image is clear and well-lit
- Try higher resolution images
- Avoid blurry or rotated receipt photos
- Ensure text is readable in the image

### OpenAI API Errors

- **401 Unauthorized**: Invalid API key
- **429 Too Many Requests**: Rate limit exceeded
- **500 Internal Server Error**: OpenAI service issue

## Example Workflow

```bash
# 1. Set up API key (one time)
cd MyAspireSolution/MyApi
dotnet user-secrets set "OpenAI:ApiKey" "sk-..."

# 2. Register and login to get JWT token
curl -X POST "https://localhost:7156/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Pass123!","username":"user"}'

curl -X POST "https://localhost:7156/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"user@example.com","password":"Pass123!"}'

# 3. Upload receipt with OCR
curl -X POST "https://localhost:7156/api/receipts/upload" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "File=@receipt.jpg" \
  -F "UseOcr=true" \
  -F "WarrantyMonths=24"

# 4. Or run OCR on existing receipt
curl -X POST "https://localhost:7156/api/receipts/{receipt-id}/ocr" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Security Notes

- API keys are stored in user secrets, not in appsettings.json
- Keys are never logged or exposed in API responses
- Each user can only OCR their own receipts (authorization enforced)
- File size limits prevent abuse (10MB max)

## Future Enhancements

- Support for PDF OCR using different OpenAI models
- Batch OCR processing for multiple receipts
- Configurable AI model selection
- Local OCR fallback (Tesseract)
- Receipt category classification
- Duplicate receipt detection
