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
    private readonly ILogger<ReceiptsController> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public ReceiptsController(
        ApplicationDbContext context,
        IFileStorageService fileStorage,
        ILogger<ReceiptsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
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
