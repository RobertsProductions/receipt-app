namespace MyApi.DTOs;

public class ReceiptResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Merchant { get; set; }
    public decimal? Amount { get; set; }
    public string? ProductName { get; set; }
    public int? WarrantyMonths { get; set; }
    public DateTime? WarrantyExpirationDate { get; set; }
    public string? Notes { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}
