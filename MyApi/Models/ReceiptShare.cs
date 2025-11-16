using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

/// <summary>
/// Represents a receipt shared from one user to another with read-only access.
/// </summary>
public class ReceiptShare
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The receipt being shared
    /// </summary>
    [Required]
    public Guid ReceiptId { get; set; }

    [ForeignKey(nameof(ReceiptId))]
    public Receipt Receipt { get; set; } = null!;

    /// <summary>
    /// The user who owns and is sharing the receipt
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser Owner { get; set; } = null!;

    /// <summary>
    /// The user who is receiving shared access to the receipt
    /// </summary>
    [Required]
    public string SharedWithUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(SharedWithUserId))]
    public ApplicationUser SharedWithUser { get; set; } = null!;

    /// <summary>
    /// When the receipt was shared
    /// </summary>
    public DateTime SharedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional note from the owner about why they're sharing
    /// </summary>
    [MaxLength(500)]
    public string? ShareNote { get; set; }
}
