using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

/// <summary>
/// Receipt sharing endpoints for sharing receipts with other users (read-only access).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptSharingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReceiptSharingController> _logger;
    private readonly INotificationService _notificationService;

    public ReceiptSharingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<ReceiptSharingController> logger,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _notificationService = notificationService;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Shares a receipt with another user by email or username (read-only access).
    /// </summary>
    /// <param name="receiptId">Receipt unique identifier</param>
    /// <param name="dto">Share details including recipient identifier and optional note</param>
    /// <returns>Share confirmation with details</returns>
    [HttpPost("{receiptId}/share")]
    [ProducesResponseType(typeof(ShareReceiptResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShareReceiptResponseDto>> ShareReceipt(
        Guid receiptId, 
        [FromBody] ShareReceiptDto dto)
    {
        var userId = GetUserId();

        // Verify the receipt exists and belongs to the current user
        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found or you don't have permission to share it" });
        }

        // Find the user to share with by email or username
        var shareWithUser = await _userManager.FindByEmailAsync(dto.ShareWithIdentifier)
            ?? await _userManager.FindByNameAsync(dto.ShareWithIdentifier);

        if (shareWithUser == null)
        {
            return BadRequest(new { message = "User not found with the provided email or username" });
        }

        // Don't allow sharing with yourself
        if (shareWithUser.Id == userId)
        {
            return BadRequest(new { message = "You cannot share a receipt with yourself" });
        }

        // Check if already shared with this user
        var existingShare = await _context.ReceiptShares
            .FirstOrDefaultAsync(rs => rs.ReceiptId == receiptId && rs.SharedWithUserId == shareWithUser.Id);

        if (existingShare != null)
        {
            return BadRequest(new { message = "This receipt is already shared with this user" });
        }

        // Create the share
        var share = new ReceiptShare
        {
            ReceiptId = receiptId,
            OwnerId = userId,
            SharedWithUserId = shareWithUser.Id,
            ShareNote = dto.ShareNote
        };

        _context.ReceiptShares.Add(share);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} shared receipt {ReceiptId} with user {SharedWithUserId}", 
            userId, receiptId, shareWithUser.Id);

        // Send notification to the recipient
        try
        {
            var owner = await _userManager.FindByIdAsync(userId);
            var ownerName = owner != null 
                ? $"{owner.FirstName} {owner.LastName}".Trim() 
                : owner?.Email ?? "A user";

            await _notificationService.SendReceiptSharedNotificationAsync(
                shareWithUser.Id,
                shareWithUser.Email!,
                ownerName,
                receipt.FileName,
                receiptId,
                dto.ShareNote);

            _logger.LogInformation("Sent sharing notification to user {SharedWithUserId}", shareWithUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send sharing notification to user {SharedWithUserId}, but share was created", 
                shareWithUser.Id);
            // Don't fail the share operation if notification fails
        }

        return Ok(new ShareReceiptResponseDto
        {
            ShareId = share.Id,
            ReceiptId = receipt.Id,
            ReceiptFileName = receipt.FileName,
            SharedWithUserId = shareWithUser.Id,
            SharedWithEmail = shareWithUser.Email!,
            SharedAt = share.SharedAt,
            ShareNote = share.ShareNote
        });
    }

    /// <summary>
    /// Gets all receipts shared with the current user.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of receipts per page (default: 20, max: 100)</param>
    /// <returns>List of receipts shared with the user</returns>
    [HttpGet("shared-with-me")]
    [ProducesResponseType(typeof(IEnumerable<SharedReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SharedReceiptDto>>> GetSharedWithMe(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var shares = await _context.ReceiptShares
            .Include(rs => rs.Receipt)
            .Include(rs => rs.Owner)
            .Where(rs => rs.SharedWithUserId == userId)
            .OrderByDescending(rs => rs.SharedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = shares.Select(rs => new SharedReceiptDto
        {
            ShareId = rs.Id,
            ReceiptId = rs.Receipt.Id,
            FileName = rs.Receipt.FileName,
            Description = rs.Receipt.Description,
            PurchaseDate = rs.Receipt.PurchaseDate,
            Merchant = rs.Receipt.Merchant,
            Amount = rs.Receipt.Amount,
            ProductName = rs.Receipt.ProductName,
            WarrantyMonths = rs.Receipt.WarrantyMonths,
            WarrantyExpirationDate = rs.Receipt.WarrantyExpirationDate,
            Notes = rs.Receipt.Notes,
            UploadedAt = rs.Receipt.UploadedAt,
            SharedAt = rs.SharedAt,
            ShareNote = rs.ShareNote,
            OwnerId = rs.Owner.Id,
            OwnerEmail = rs.Owner.Email!,
            OwnerName = $"{rs.Owner.FirstName} {rs.Owner.LastName}".Trim(),
            DownloadUrl = Url.Action("DownloadSharedReceipt", new { shareId = rs.Id }) ?? string.Empty
        });

        return Ok(result);
    }

    /// <summary>
    /// Gets all users that a specific receipt is shared with.
    /// </summary>
    /// <param name="receiptId">Receipt unique identifier</param>
    /// <returns>List of users the receipt is shared with</returns>
    [HttpGet("{receiptId}/shares")]
    [ProducesResponseType(typeof(IEnumerable<ReceiptShareInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReceiptShareInfoDto>>> GetReceiptShares(Guid receiptId)
    {
        var userId = GetUserId();

        // Verify the receipt exists and belongs to the current user
        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId);

        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found or you don't have permission to view its shares" });
        }

        var shares = await _context.ReceiptShares
            .Include(rs => rs.SharedWithUser)
            .Where(rs => rs.ReceiptId == receiptId)
            .OrderByDescending(rs => rs.SharedAt)
            .ToListAsync();

        var result = shares.Select(rs => new ReceiptShareInfoDto
        {
            ShareId = rs.Id,
            SharedWithUserId = rs.SharedWithUser.Id,
            SharedWithEmail = rs.SharedWithUser.Email!,
            SharedWithName = $"{rs.SharedWithUser.FirstName} {rs.SharedWithUser.LastName}".Trim(),
            SharedAt = rs.SharedAt,
            ShareNote = rs.ShareNote
        });

        return Ok(result);
    }

    /// <summary>
    /// Revokes a receipt share, removing access for the specified user.
    /// </summary>
    /// <param name="shareId">Share unique identifier</param>
    /// <returns>Success message on revocation</returns>
    [HttpDelete("{shareId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeShare(Guid shareId)
    {
        var userId = GetUserId();

        var share = await _context.ReceiptShares
            .Include(rs => rs.Receipt)
            .FirstOrDefaultAsync(rs => rs.Id == shareId);

        if (share == null)
        {
            return NotFound(new { message = "Share not found" });
        }

        // Only the owner can revoke the share
        if (share.Receipt.UserId != userId)
        {
            return NotFound(new { message = "Share not found or you don't have permission to revoke it" });
        }

        _context.ReceiptShares.Remove(share);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} revoked share {ShareId} for receipt {ReceiptId}", 
            userId, shareId, share.ReceiptId);

        return NoContent();
    }

    /// <summary>
    /// Downloads a shared receipt file (read-only access).
    /// </summary>
    /// <param name="shareId">Share unique identifier</param>
    /// <returns>Receipt file with original filename and content type</returns>
    [HttpGet("{shareId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadSharedReceipt(Guid shareId)
    {
        var userId = GetUserId();

        var share = await _context.ReceiptShares
            .Include(rs => rs.Receipt)
            .FirstOrDefaultAsync(rs => rs.Id == shareId && rs.SharedWithUserId == userId);

        if (share == null)
        {
            return NotFound(new { message = "Shared receipt not found or you don't have access to it" });
        }

        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), share.Receipt.StoragePath);
            
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Shared receipt file not found: {FilePath}", filePath);
                return NotFound(new { message = "Receipt file not found in storage" });
            }

            var stream = System.IO.File.OpenRead(filePath);
            
            _logger.LogInformation("User {UserId} downloaded shared receipt {ReceiptId} via share {ShareId}", 
                userId, share.ReceiptId, shareId);

            return File(stream, share.Receipt.FileType, share.Receipt.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading shared receipt {ReceiptId}", share.ReceiptId);
            return StatusCode(500, new { message = "An error occurred while downloading the shared receipt" });
        }
    }

    /// <summary>
    /// Gets details of a specific shared receipt.
    /// </summary>
    /// <param name="shareId">Share unique identifier</param>
    /// <returns>Shared receipt details</returns>
    [HttpGet("{shareId}/details")]
    [ProducesResponseType(typeof(SharedReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SharedReceiptDto>> GetSharedReceiptDetails(Guid shareId)
    {
        var userId = GetUserId();

        var share = await _context.ReceiptShares
            .Include(rs => rs.Receipt)
            .Include(rs => rs.Owner)
            .FirstOrDefaultAsync(rs => rs.Id == shareId && rs.SharedWithUserId == userId);

        if (share == null)
        {
            return NotFound(new { message = "Shared receipt not found or you don't have access to it" });
        }

        var result = new SharedReceiptDto
        {
            ShareId = share.Id,
            ReceiptId = share.Receipt.Id,
            FileName = share.Receipt.FileName,
            Description = share.Receipt.Description,
            PurchaseDate = share.Receipt.PurchaseDate,
            Merchant = share.Receipt.Merchant,
            Amount = share.Receipt.Amount,
            ProductName = share.Receipt.ProductName,
            WarrantyMonths = share.Receipt.WarrantyMonths,
            WarrantyExpirationDate = share.Receipt.WarrantyExpirationDate,
            Notes = share.Receipt.Notes,
            UploadedAt = share.Receipt.UploadedAt,
            SharedAt = share.SharedAt,
            ShareNote = share.ShareNote,
            OwnerId = share.Owner.Id,
            OwnerEmail = share.Owner.Email!,
            OwnerName = $"{share.Owner.FirstName} {share.Owner.LastName}".Trim(),
            DownloadUrl = Url.Action(nameof(DownloadSharedReceipt), new { shareId = share.Id }) ?? string.Empty
        };

        return Ok(result);
    }
}
