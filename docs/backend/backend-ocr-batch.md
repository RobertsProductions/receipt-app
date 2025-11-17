# Batch OCR Processing

This document describes the batch OCR processing feature that allows users to process multiple receipts at once.

## Overview

The batch OCR feature enables users to submit multiple receipt IDs and have OCR processing applied to all of them in a single API call. This is useful when users have accumulated several receipts without OCR data and want to process them all at once.

## Implementation Details

### API Endpoint

**POST** `/api/Receipts/batch-ocr`

Process multiple receipts with OCR in a single batch operation.

**Request Body:**
```json
{
  "receiptIds": [
    "receipt-guid-1",
    "receipt-guid-2",
    "receipt-guid-3"
  ]
}
```

**Response:**
```json
{
  "totalRequested": 3,
  "successfullyProcessed": 2,
  "failed": 1,
  "skipped": 0,
  "results": [
    {
      "receiptId": "receipt-guid-1",
      "fileName": "receipt1.jpg",
      "success": true,
      "errorMessage": null,
      "updatedReceipt": {
        "id": "receipt-guid-1",
        "fileName": "receipt1.jpg",
        "merchant": "Walmart",
        "amount": 45.99,
        "purchaseDate": "2025-11-10",
        "productName": "Laptop",
        ...
      }
    },
    {
      "receiptId": "receipt-guid-2",
      "fileName": "receipt2.jpg",
      "success": true,
      "errorMessage": null,
      "updatedReceipt": { ... }
    },
    {
      "receiptId": "receipt-guid-3",
      "fileName": "document.pdf",
      "success": false,
      "errorMessage": "OCR is only supported for image files (JPG, PNG)",
      "updatedReceipt": null
    }
  ]
}
```

### DTOs

**BatchOcrRequestDto**
```csharp
public class BatchOcrRequestDto
{
    public List<Guid> ReceiptIds { get; set; } = new();
}
```

**BatchOcrResultDto**
```csharp
public class BatchOcrResultDto
{
    public int TotalRequested { get; set; }
    public int SuccessfullyProcessed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<ReceiptOcrResultDto> Results { get; set; } = new();
}
```

**ReceiptOcrResultDto**
```csharp
public class ReceiptOcrResultDto
{
    public Guid ReceiptId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public ReceiptResponseDto? UpdatedReceipt { get; set; }
}
```

## Processing Logic

1. **Validation**: The endpoint validates that receipt IDs are provided in the request
2. **Authorization**: Fetches only receipts belonging to the authenticated user
3. **File Type Check**: Skips non-image files (only JPG and PNG supported)
4. **OCR Processing**: Performs OCR on each valid image receipt
5. **Data Update**: Updates only null fields in the receipt record:
   - Merchant
   - Amount
   - Purchase Date
   - Product Name
   - Notes (appends OCR extracted text)
6. **Batch Save**: All successful updates are saved in a single database transaction
7. **Response**: Returns detailed results for each receipt including success/failure status

## Error Handling

The batch endpoint handles various error scenarios gracefully:

- **Receipt Not Found**: Receipt doesn't exist or doesn't belong to the user → Skipped
- **Unsupported File Type**: File is not JPG/PNG → Skipped
- **File Not Found**: Receipt file missing from storage → Failed
- **OCR Failure**: OpenAI OCR service fails → Failed
- **Unexpected Errors**: Any other exception → Failed

Each error is logged and reported in the response with a specific error message.

## Usage Examples

### Example 1: Process Multiple Receipts

```bash
curl -X POST "https://localhost:7156/api/Receipts/batch-ocr" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "receiptIds": [
      "b4c0739f-332e-4f1c-a8de-aec3ab05c374",
      "7f8e9a3b-421c-4d5e-b6a7-9c8d1e2f3a4b",
      "c5d6e7f8-9a0b-1c2d-3e4f-5a6b7c8d9e0f"
    ]
  }'
```

### Example 2: Check Results

The response provides detailed information about each receipt:
- **successfullyProcessed**: Count of receipts updated with OCR data
- **failed**: Count of receipts that encountered errors during OCR
- **skipped**: Count of receipts that were skipped (wrong file type, not found, etc.)
- **results**: Array with individual result for each receipt ID

## Performance Considerations

- **Sequential Processing**: Receipts are processed sequentially to avoid overwhelming the OpenAI API
- **Batch Database Save**: All changes are saved in a single transaction at the end
- **Error Isolation**: Failure of one receipt doesn't affect others
- **Cost Management**: Users should be aware that batch OCR can consume multiple OpenAI API credits

## Benefits

1. **Efficiency**: Process multiple receipts in one API call
2. **Convenience**: No need to manually trigger OCR for each receipt
3. **Detailed Feedback**: Know exactly which receipts succeeded or failed
4. **Cost Tracking**: Clear visibility of how many receipts were processed
5. **Selective Updates**: Only updates fields that are currently null

## Integration with Existing Features

The batch OCR feature complements the existing single-receipt OCR endpoint (`POST /api/Receipts/{id}/ocr`) by providing a more efficient way to process multiple receipts. Users can:

1. Upload receipts without OCR (using `useOcr: false`)
2. Later batch process them all at once
3. Review and manually correct any fields as needed

## Future Enhancements

Potential improvements for the batch OCR feature:

1. **Parallel Processing**: Process multiple receipts concurrently with rate limiting
2. **Background Processing**: Queue batch jobs for asynchronous processing
3. **Progress Tracking**: WebSocket or polling endpoint to check batch job status
4. **Selective Field Updates**: Allow users to specify which fields should be updated by OCR
5. **Cost Estimation**: Preview estimated OpenAI API cost before processing
