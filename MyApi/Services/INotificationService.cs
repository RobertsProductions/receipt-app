namespace MyApi.Services;

public interface INotificationService
{
    Task SendWarrantyExpirationNotificationAsync(string userId, string userEmail, string productName, DateTime expirationDate, Guid receiptId);
    Task SendReceiptSharedNotificationAsync(string recipientUserId, string recipientEmail, string ownerName, string receiptFileName, Guid receiptId, string? shareNote);
}

public class WarrantyNotification
{
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public Guid ReceiptId { get; set; }
    public int DaysUntilExpiration { get; set; }
}
