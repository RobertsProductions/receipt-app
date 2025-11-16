namespace MyApi.Services;

public interface IOcrService
{
    Task<OcrResult> ExtractReceiptDataAsync(Stream imageStream, string fileName);
}

public class OcrResult
{
    public string? Merchant { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? ProductName { get; set; }
    public string? ExtractedText { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
