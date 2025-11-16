using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyApi.Data;

namespace MyApi.Services;

public class WarrantyExpirationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<WarrantyExpirationService> _logger;
    private readonly TimeSpan _checkInterval;
    private readonly int _notificationDaysThreshold;
    private const string CacheKey = "warranty_expiration_cache";

    public WarrantyExpirationService(
        IServiceProvider serviceProvider,
        IMemoryCache cache,
        ILogger<WarrantyExpirationService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _logger = logger;
        
        // Default: check every 24 hours
        var intervalHours = configuration.GetValue<int>("WarrantyNotification:CheckIntervalHours", 24);
        _checkInterval = TimeSpan.FromHours(intervalHours);
        
        // Default: notify 7 days before expiration
        _notificationDaysThreshold = configuration.GetValue<int>("WarrantyNotification:NotificationDaysThreshold", 7);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Warranty Expiration Service started. Check interval: {Interval} hours, Notification threshold: {Days} days",
            _checkInterval.TotalHours, _notificationDaysThreshold);

        // Wait a bit before first check to allow the application to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyExpiringWarrantiesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking warranty expirations");
            }

            // Wait for the next check interval
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndNotifyExpiringWarrantiesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting warranty expiration check...");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        // Get current date
        var today = DateTime.UtcNow.Date;

        // Query receipts with warranties expiring soon
        // We'll use the maximum threshold (90 days) to get all potentially expiring receipts
        // Then filter by user-specific preferences
        var maxThreshold = 90;
        var maxThresholdDate = today.AddDays(maxThreshold);

        var expiringReceipts = await dbContext.Receipts
            .Include(r => r.User)
            .Where(r => r.WarrantyExpirationDate.HasValue 
                     && r.WarrantyExpirationDate.Value.Date > today
                     && r.WarrantyExpirationDate.Value.Date <= maxThresholdDate)
            .ToListAsync(cancellationToken);

        if (!expiringReceipts.Any())
        {
            _logger.LogInformation("No warranties expiring within {Days} days", maxThreshold);
            UpdateCache(new List<WarrantyNotification>());
            return;
        }

        _logger.LogInformation("Found {Count} warranties expiring within {Days} days (before user preference filtering)", 
            expiringReceipts.Count, maxThreshold);

        // Get previously notified receipts from cache
        var notifiedReceipts = _cache.Get<HashSet<Guid>>("notified_receipts") ?? new HashSet<Guid>();
        var notifications = new List<WarrantyNotification>();

        foreach (var receipt in expiringReceipts)
        {
            var daysUntilExpiration = (receipt.WarrantyExpirationDate!.Value.Date - today).Days;
            var user = receipt.User;

            // Skip if user has opted out
            if (user?.OptOutOfNotifications == true)
            {
                _logger.LogDebug("Skipping receipt {ReceiptId} - user {UserId} opted out", receipt.Id, receipt.UserId);
                continue;
            }

            // Check if warranty is within user's notification threshold
            var userThreshold = user?.NotificationThresholdDays ?? _notificationDaysThreshold;
            if (daysUntilExpiration > userThreshold)
            {
                _logger.LogDebug("Skipping receipt {ReceiptId} - expires in {Days} days, user threshold is {Threshold} days",
                    receipt.Id, daysUntilExpiration, userThreshold);
                continue;
            }
            
            // Create notification object for cache
            var notification = new WarrantyNotification
            {
                UserId = receipt.UserId,
                UserEmail = receipt.User?.Email ?? "unknown@example.com",
                ProductName = receipt.ProductName ?? receipt.Description ?? "Product",
                ExpirationDate = receipt.WarrantyExpirationDate.Value,
                ReceiptId = receipt.Id,
                DaysUntilExpiration = daysUntilExpiration
            };
            notifications.Add(notification);

            // Only send notification if we haven't notified for this receipt yet
            if (!notifiedReceipts.Contains(receipt.Id))
            {
                await notificationService.SendWarrantyExpirationNotificationAsync(
                    receipt.UserId,
                    receipt.User?.Email ?? "unknown@example.com",
                    receipt.ProductName ?? receipt.Description ?? "Product",
                    receipt.WarrantyExpirationDate.Value,
                    receipt.Id);

                notifiedReceipts.Add(receipt.Id);
                
                _logger.LogInformation("Sent notification for receipt {ReceiptId} - {Product} expiring in {Days} days",
                    receipt.Id, notification.ProductName, daysUntilExpiration);
            }
            else
            {
                _logger.LogDebug("Skipping notification for receipt {ReceiptId} - already notified", receipt.Id);
            }
        }

        // Update cache with notified receipts (expire after 30 days to clean up old entries)
        _cache.Set("notified_receipts", notifiedReceipts, TimeSpan.FromDays(30));
        
        // Update warranty expiration cache
        UpdateCache(notifications);

        _logger.LogInformation("Warranty expiration check completed. {NewNotifications} new notifications sent, {Total} total expiring",
            expiringReceipts.Count(r => !notifiedReceipts.Contains(r.Id)), 
            notifications.Count);
    }

    private void UpdateCache(List<WarrantyNotification> notifications)
    {
        // Cache the current list of expiring warranties for API access
        _cache.Set(CacheKey, notifications, TimeSpan.FromHours(25)); // Cache slightly longer than check interval
        
        _logger.LogDebug("Updated warranty expiration cache with {Count} items", notifications.Count);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Warranty Expiration Service is stopping");
        return base.StopAsync(cancellationToken);
    }
}
