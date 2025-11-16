namespace MyApi.Services;

/// <summary>
/// Logs warranty expiration notifications to the console/logs.
/// In production, this can be replaced with EmailNotificationService or SmsNotificationService.
/// </summary>
public class LogNotificationService : INotificationService
{
    private readonly ILogger<LogNotificationService> _logger;

    public LogNotificationService(ILogger<LogNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendWarrantyExpirationNotificationAsync(
        string userId, 
        string userEmail, 
        string productName, 
        DateTime expirationDate, 
        Guid receiptId)
    {
        var daysUntilExpiration = (expirationDate.Date - DateTime.UtcNow.Date).Days;
        
        _logger.LogWarning(
            "WARRANTY EXPIRATION NOTIFICATION: User {UserId} ({Email}) - Product '{Product}' warranty expires in {Days} days on {ExpirationDate}. Receipt ID: {ReceiptId}",
            userId,
            userEmail,
            productName ?? "Unknown Product",
            daysUntilExpiration,
            expirationDate.ToString("yyyy-MM-dd"),
            receiptId);

        // In production, this would send an actual email or SMS
        // For now, we just log it
        
        return Task.CompletedTask;
    }
}
