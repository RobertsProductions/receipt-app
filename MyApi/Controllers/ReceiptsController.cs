using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IOcrService _ocrService;
    private readonly ILogger<ReceiptsController> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ReceiptsController(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        IOcrService ocrService,
        ILogger<ReceiptsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _ocrService = ocrService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

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

    [HttpGet("{id}")]
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

    [HttpGet]
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

            _logger.LogInformation("User {UserId} deleted receipt {ReceiptId}", userId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt {ReceiptId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the receipt" });
        }
    }

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
