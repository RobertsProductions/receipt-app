using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

/// <summary>
/// Receipt management endpoints for uploading, viewing, downloading, and processing receipts with OCR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOcrService _ocrService;
    private readonly ILogger<ReceiptsController> _logger;
    private readonly IUserCacheService _userCacheService;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ReceiptsController(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        IOcrService ocrService,
        ILogger<ReceiptsController> logger,
        IUserCacheService userCacheService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _ocrService = ocrService;
        _logger = logger;
        _userCacheService = userCacheService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Uploads a receipt image or PDF file with optional OCR processing.
    /// </summary>
    /// <param name="dto">Receipt details including file, metadata, and OCR option</param>
    /// <returns>Uploaded receipt details with OCR-extracted data if requested</returns>
    /// <remarks>
    /// Supports JPG, PNG, and PDF files up to 10MB. OCR can automatically extract merchant, amount, date, and product information.
    /// </remarks>
    [HttpPost("upload")]
    public async Task<ActionResult<ReceiptResponseDto>> UploadReceipt([FromForm] UploadReceiptDto dto)
    {
        var userId = GetUserId();

        // Validate file
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        if (dto.File.Length > MaxFileSize)
        {
            return BadRequest(new { message = $"File size exceeds maximum allowed size of {MaxFileSize / 1024 / 1024}MB" });
        }

        var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"File type not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}" });
        }

        try
        {
            // Perform OCR if requested and file is an image
            OcrResult? ocrResult = null;
            if (dto.UseOcr && (extension == ".jpg" || extension == ".jpeg" || extension == ".png"))
            {
                _logger.LogInformation("OCR requested for file: {FileName}, extension: {Extension}", dto.File.FileName, extension);
                _logger.LogInformation("Performing OCR on uploaded receipt");
                
                // Reset stream position for OCR
                dto.File.OpenReadStream().Position = 0;
                using var ocrStream = dto.File.OpenReadStream();
                ocrResult = await _ocrService.ExtractReceiptDataAsync(ocrStream, dto.File.FileName);

                if (ocrResult.Success)
                {
                    _logger.LogInformation("OCR successful. Extracted data - Merchant: {Merchant}, Amount: {Amount}, Date: {Date}, Product: {Product}",
                        ocrResult.Merchant ?? "null", 
                        ocrResult.Amount?.ToString() ?? "null", 
                        ocrResult.PurchaseDate?.ToString("yyyy-MM-dd") ?? "null",
                        ocrResult.ProductName ?? "null");
                    
                    // Use OCR results if fields aren't manually provided (treat empty strings as null)
                    if (string.IsNullOrWhiteSpace(dto.Merchant) && !string.IsNullOrWhiteSpace(ocrResult.Merchant))
                    {
                        dto.Merchant = ocrResult.Merchant;
                    }
                    
                    if (!dto.Amount.HasValue && ocrResult.Amount.HasValue)
                    {
                        dto.Amount = ocrResult.Amount;
                    }
                    
                    if (!dto.PurchaseDate.HasValue && ocrResult.PurchaseDate.HasValue)
                    {
                        dto.PurchaseDate = ocrResult.PurchaseDate;
                    }
                    
                    if (string.IsNullOrWhiteSpace(dto.ProductName) && !string.IsNullOrWhiteSpace(ocrResult.ProductName))
                    {
                        dto.ProductName = ocrResult.ProductName;
                    }
                    
                    // Append OCR extracted text to notes if present
                    if (!string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
                    {
                        dto.Notes = string.IsNullOrWhiteSpace(dto.Notes)
                            ? $"OCR: {ocrResult.ExtractedText}"
                            : $"{dto.Notes}\n\nOCR: {ocrResult.ExtractedText}";
                    }

                    _logger.LogInformation("OCR data applied to DTO. Final values - Merchant: {Merchant}, Amount: {Amount}, Date: {Date}, Product: {Product}",
                        dto.Merchant ?? "null", 
                        dto.Amount?.ToString() ?? "null", 
                        dto.PurchaseDate?.ToString("yyyy-MM-dd") ?? "null",
                        dto.ProductName ?? "null");
                }
                else
                {
                    _logger.LogWarning("OCR failed: {Error}", ocrResult.ErrorMessage);
                }
            }
            else if (dto.UseOcr)
            {
                _logger.LogWarning("OCR was requested but file type {Extension} is not supported for OCR", extension);
            }

            // Save file to storage
            var storagePath = await _fileStorage.SaveFileAsync(dto.File, userId);

            // Calculate warranty expiration if warranty months provided
            DateTime? warrantyExpiration = null;
            if (dto.WarrantyMonths.HasValue && dto.PurchaseDate.HasValue)
            {
                warrantyExpiration = dto.PurchaseDate.Value.AddMonths(dto.WarrantyMonths.Value);
            }

            // Create receipt record
            var receipt = new Receipt
            {
                UserId = userId,
                FileName = dto.File.FileName,
                FileType = dto.File.ContentType,
                FileSizeBytes = dto.File.Length,
                StoragePath = storagePath,
                Description = dto.Description,
                PurchaseDate = dto.PurchaseDate,
                Merchant = dto.Merchant,
                Amount = dto.Amount,
                ProductName = dto.ProductName,
                WarrantyMonths = dto.WarrantyMonths,
                WarrantyExpirationDate = warrantyExpiration,
                Notes = dto.Notes
            };

            _context.Receipts.Add(receipt);
            await _context.SaveChangesAsync();

            // Invalidate receipt cache after adding new receipt
            _userCacheService.InvalidateReceiptCache(userId);

            _logger.LogInformation("User {UserId} uploaded receipt {ReceiptId}", userId, receipt.Id);

            var response = MapToResponseDto(receipt);
            return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading receipt for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while uploading the receipt" });
        }
    }

    /// <summary>
    /// Retrieves a specific receipt by ID.
    /// </summary>
    /// <param name="id">Receipt unique identifier</param>
    /// <returns>Receipt details including metadata and download URL</returns>
    [HttpGet("{id}")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })] // Cache for 5 minutes
    public async Task<ActionResult<ReceiptResponseDto>> GetReceipt(Guid id)
    {
        var userId = GetUserId();

        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found" });
        }

        return MapToResponseDto(receipt);
    }

    /// <summary>
    /// Retrieves a paginated list of receipts for the authenticated user.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of receipts per page (default: 20, max: 100)</param>
    /// <returns>List of receipts ordered by upload date (newest first)</returns>
    [HttpGet]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "page", "pageSize" })] // Cache for 1 minute
    public async Task<ActionResult<IEnumerable<ReceiptResponseDto>>> GetReceipts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var receipts = await _context.Receipts
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = receipts.Select(MapToResponseDto);
        return Ok(response);
    }

    /// <summary>
    /// Downloads the original receipt file (image or PDF).
    /// </summary>
    /// <param name="id">Receipt unique identifier</param>
    /// <returns>Receipt file with original filename and content type</returns>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadReceipt(Guid id)
    {
        var userId = GetUserId();

        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found" });
        }

        try
        {
            var stream = await _fileStorage.GetFileAsync(receipt.StoragePath);
            return File(stream, receipt.FileType, receipt.FileName);
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("File not found for receipt {ReceiptId}", id);
            return NotFound(new { message = "Receipt file not found in storage" });
        }
    }

    /// <summary>
    /// Deletes a receipt and its associated file from storage.
    /// </summary>
    /// <param name="id">Receipt unique identifier</param>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceipt(Guid id)
    {
        var userId = GetUserId();

        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found" });
        }

        try
        {
            await _fileStorage.DeleteFileAsync(receipt.StoragePath);
            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();

            // Invalidate receipt cache after deleting receipt
            _userCacheService.InvalidateReceiptCache(userId);

            _logger.LogInformation("User {UserId} deleted receipt {ReceiptId}", userId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {ReceiptId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the receipt" });
        }
    }

    /// <summary>
    /// Performs OCR on an existing receipt to extract data (merchant, amount, date, product).
    /// </summary>
    /// <param name="id">Receipt unique identifier</param>
    /// <returns>Updated receipt with OCR-extracted data appended to existing fields</returns>
    /// <remarks>
    /// OCR only works on image files (JPG, PNG). Extracted data only fills empty fields to preserve manual entries.
    /// </remarks>
    [HttpPost("{id}/ocr")]
    public async Task<ActionResult<ReceiptResponseDto>> PerformOcr(Guid id)
    {
        var userId = GetUserId();

        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found" });
        }

        // Check if file is an image
        var extension = Path.GetExtension(receipt.FileName).ToLowerInvariant();
        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            return BadRequest(new { message = "OCR is only supported for image files (JPG, PNG)" });
        }

        try
        {
            // Get the file and perform OCR
            using var fileStream = await _fileStorage.GetFileAsync(receipt.StoragePath);
            var ocrResult = await _ocrService.ExtractReceiptDataAsync(fileStream, receipt.FileName);

            if (!ocrResult.Success)
            {
                return BadRequest(new { message = $"OCR failed: {ocrResult.ErrorMessage}" });
            }

            // Update receipt with OCR results (only update null fields)
            var updated = false;

            if (receipt.Merchant == null && !string.IsNullOrWhiteSpace(ocrResult.Merchant))
            {
                receipt.Merchant = ocrResult.Merchant;
                updated = true;
            }

            if (receipt.Amount == null && ocrResult.Amount.HasValue)
            {
                receipt.Amount = ocrResult.Amount;
                updated = true;
            }

            if (receipt.PurchaseDate == null && ocrResult.PurchaseDate.HasValue)
            {
                receipt.PurchaseDate = ocrResult.PurchaseDate;
                updated = true;
            }

            if (receipt.ProductName == null && !string.IsNullOrWhiteSpace(ocrResult.ProductName))
            {
                receipt.ProductName = ocrResult.ProductName;
                updated = true;
            }

            // Append OCR extracted text to notes
            if (!string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
            {
                receipt.Notes = string.IsNullOrWhiteSpace(receipt.Notes)
                    ? $"OCR: {ocrResult.ExtractedText}"
                    : $"{receipt.Notes}\n\nOCR: {ocrResult.ExtractedText}";
                updated = true;
            }

            if (updated)
            {
                receipt.LastModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                // Invalidate receipt cache after OCR update
                _userCacheService.InvalidateReceiptCache(userId);
                
                _logger.LogInformation("Updated receipt {ReceiptId} with OCR data", id);
            }

            return MapToResponseDto(receipt);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "Receipt file not found in storage" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing OCR on receipt {ReceiptId}", id);
            return StatusCode(500, new { message = "An error occurred during OCR processing" });
        }
    }

    /// <summary>
    /// Performs OCR on multiple receipts in a single batch operation.
    /// </summary>
    /// <param name="request">List of receipt IDs to process</param>
    /// <returns>Batch processing results showing success, failure, and skipped counts with details</returns>
    /// <remarks>
    /// Processes multiple receipts efficiently. Each receipt is processed independently; failures don't affect other receipts.
    /// </remarks>
    [HttpPost("batch-ocr")]
    public async Task<ActionResult<BatchOcrResultDto>> PerformBatchOcr([FromBody] BatchOcrRequestDto request)
    {
        var userId = GetUserId();

        if (request.ReceiptIds == null || !request.ReceiptIds.Any())
        {
            return BadRequest(new { message = "No receipt IDs provided" });
        }

        var result = new BatchOcrResultDto
        {
            TotalRequested = request.ReceiptIds.Count
        };

        _logger.LogInformation("User {UserId} requested batch OCR for {Count} receipts", userId, request.ReceiptIds.Count);

        // Fetch all receipts belonging to the user
        var receipts = await _context.Receipts
            .Where(r => request.ReceiptIds.Contains(r.Id) && r.UserId == userId)
            .ToListAsync();

        foreach (var receiptId in request.ReceiptIds)
        {
            var receipt = receipts.FirstOrDefault(r => r.Id == receiptId);
            var receiptResult = new ReceiptOcrResultDto
            {
                ReceiptId = receiptId,
                FileName = receipt?.FileName ?? "Unknown"
            };

            if (receipt == null)
            {
                receiptResult.Success = false;
                receiptResult.ErrorMessage = "Receipt not found or does not belong to user";
                result.Skipped++;
                result.Results.Add(receiptResult);
                continue;
            }

            // Check if file is an image
            var extension = Path.GetExtension(receipt.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                receiptResult.Success = false;
                receiptResult.ErrorMessage = "OCR is only supported for image files (JPG, PNG)";
                result.Skipped++;
                result.Results.Add(receiptResult);
                continue;
            }

            try
            {
                // Get the file and perform OCR
                using var fileStream = await _fileStorage.GetFileAsync(receipt.StoragePath);
                var ocrResult = await _ocrService.ExtractReceiptDataAsync(fileStream, receipt.FileName);

                if (!ocrResult.Success)
                {
                    receiptResult.Success = false;
                    receiptResult.ErrorMessage = $"OCR failed: {ocrResult.ErrorMessage}";
                    result.Failed++;
                    result.Results.Add(receiptResult);
                    continue;
                }

                // Update receipt with OCR results (only update null fields)
                var updated = false;

                if (receipt.Merchant == null && !string.IsNullOrWhiteSpace(ocrResult.Merchant))
                {
                    receipt.Merchant = ocrResult.Merchant;
                    updated = true;
                }

                if (receipt.Amount == null && ocrResult.Amount.HasValue)
                {
                    receipt.Amount = ocrResult.Amount;
                    updated = true;
                }

                if (receipt.PurchaseDate == null && ocrResult.PurchaseDate.HasValue)
                {
                    receipt.PurchaseDate = ocrResult.PurchaseDate;
                    updated = true;
                }

                if (receipt.ProductName == null && !string.IsNullOrWhiteSpace(ocrResult.ProductName))
                {
                    receipt.ProductName = ocrResult.ProductName;
                    updated = true;
                }

                // Append OCR extracted text to notes
                if (!string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
                {
                    receipt.Notes = string.IsNullOrWhiteSpace(receipt.Notes)
                        ? $"OCR: {ocrResult.ExtractedText}"
                        : $"{receipt.Notes}\n\nOCR: {ocrResult.ExtractedText}";
                    updated = true;
                }

                if (updated)
                {
                    receipt.LastModifiedAt = DateTime.UtcNow;
                }

                receiptResult.Success = true;
                receiptResult.UpdatedReceipt = MapToResponseDto(receipt);
                result.SuccessfullyProcessed++;
                result.Results.Add(receiptResult);

                _logger.LogInformation("Successfully processed OCR for receipt {ReceiptId}", receiptId);
            }
            catch (FileNotFoundException)
            {
                receiptResult.Success = false;
                receiptResult.ErrorMessage = "Receipt file not found in storage";
                result.Failed++;
                result.Results.Add(receiptResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing OCR on receipt {ReceiptId}", receiptId);
                receiptResult.Success = false;
                receiptResult.ErrorMessage = "An unexpected error occurred during OCR processing";
                result.Failed++;
                result.Results.Add(receiptResult);
            }
        }

        // Save all changes at once
        if (result.SuccessfullyProcessed > 0)
        {
            try
            {
                await _context.SaveChangesAsync();
                
                // Invalidate receipt cache after batch OCR
                _userCacheService.InvalidateReceiptCache(userId);
                
                _logger.LogInformation("Batch OCR completed. Total: {Total}, Success: {Success}, Failed: {Failed}, Skipped: {Skipped}",
                    result.TotalRequested, result.SuccessfullyProcessed, result.Failed, result.Skipped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving batch OCR changes");
                return StatusCode(500, new { message = "Batch OCR completed but failed to save changes" });
            }
        }

        return Ok(result);
    }

    private ReceiptResponseDto MapToResponseDto(Receipt receipt)
    {
        return new ReceiptResponseDto
        {
            Id = receipt.Id,
            FileName = receipt.FileName,
            FileType = receipt.FileType,
            FileSizeBytes = receipt.FileSizeBytes,
            Description = receipt.Description,
            UploadedAt = receipt.UploadedAt,
            PurchaseDate = receipt.PurchaseDate,
            Merchant = receipt.Merchant,
            Amount = receipt.Amount,
            ProductName = receipt.ProductName,
            WarrantyMonths = receipt.WarrantyMonths,
            WarrantyExpirationDate = receipt.WarrantyExpirationDate,
            Notes = receipt.Notes,
            DownloadUrl = Url.Action(nameof(DownloadReceipt), new { id = receipt.Id }) ?? string.Empty
        };
    }
}
