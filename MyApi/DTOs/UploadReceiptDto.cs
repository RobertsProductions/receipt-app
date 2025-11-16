using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class UploadReceiptDto
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [MaxLength(200)]
    public string? Merchant { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(200)]
    public string? ProductName { get; set; }

    public int? WarrantyMonths { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
