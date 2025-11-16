namespace MyApi.DTOs;

public class BatchOcrResultDto
{
    public int TotalRequested { get; set; }
    public int SuccessfullyProcessed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<ReceiptOcrResultDto> Results { get; set; } = new();
}

public class ReceiptOcrResultDto
{
    public Guid ReceiptId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public ReceiptResponseDto? UpdatedReceipt { get; set; }
}
