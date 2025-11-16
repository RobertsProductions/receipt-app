using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyApi.Models;

public class Receipt
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [Required]
    [MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PurchaseDate { get; set; }

    [MaxLength(200)]
    public string? Merchant { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Amount { get; set; }

    [MaxLength(200)]
    public string? ProductName { get; set; }

    public int? WarrantyMonths { get; set; }

    public DateTime? WarrantyExpirationDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime? LastModifiedAt { get; set; }
}
