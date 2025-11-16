using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

/// <summary>
/// Request to share a receipt with another user
/// </summary>
public class ShareReceiptDto
{
    /// <summary>
    /// The email address or username of the user to share with
    /// </summary>
    [Required]
    public string ShareWithIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Optional note about why this receipt is being shared
    /// </summary>
    [MaxLength(500)]
    public string? ShareNote { get; set; }
}

/// <summary>
/// Response when a receipt is shared
/// </summary>
public class ShareReceiptResponseDto
{
    public Guid ShareId { get; set; }
    public Guid ReceiptId { get; set; }
    public string ReceiptFileName { get; set; } = string.Empty;
    public string SharedWithUserId { get; set; } = string.Empty;
    public string SharedWithEmail { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public string? ShareNote { get; set; }
}

/// <summary>
/// Details about a shared receipt
/// </summary>
public class SharedReceiptDto
{
    public Guid ShareId { get; set; }
    public Guid ReceiptId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Merchant { get; set; }
    public decimal? Amount { get; set; }
    public string? ProductName { get; set; }
    public int? WarrantyMonths { get; set; }
    public DateTime? WarrantyExpirationDate { get; set; }
    public string? Notes { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime SharedAt { get; set; }
    public string? ShareNote { get; set; }
    
    // Owner information
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    
    // Download URL
    public string DownloadUrl { get; set; } = string.Empty;
}

/// <summary>
/// Information about who a receipt is shared with
/// </summary>
public class ReceiptShareInfoDto
{
    public Guid ShareId { get; set; }
    public string SharedWithUserId { get; set; } = string.Empty;
    public string SharedWithEmail { get; set; } = string.Empty;
    public string SharedWithName { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public string? ShareNote { get; set; }
}
