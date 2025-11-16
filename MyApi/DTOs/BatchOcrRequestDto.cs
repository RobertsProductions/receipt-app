namespace MyApi.DTOs;

public class BatchOcrRequestDto
{
    public List<Guid> ReceiptIds { get; set; } = new();
}
