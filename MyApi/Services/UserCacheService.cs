using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MyApi.Data;
using MyApi.DTOs;
using MyApi.Models;

namespace MyApi.Services;

/// <summary>
/// Service for caching user-specific data to improve performance on login and subsequent requests.
/// </summary>
public class UserCacheService : IUserCacheService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserCacheService> _logger;
    private const string UserReceiptsCacheKeyPrefix = "user_receipts_";
    private const string UserPreferencesCacheKeyPrefix = "user_preferences_";
    private readonly TimeSpan _defaultCacheExpiration = TimeSpan.FromMinutes(15);

    public UserCacheService(
        IServiceProvider serviceProvider,
        IMemoryCache cache,
        ILogger<UserCacheService> logger)
    {
        _serviceProvider = serviceProvider;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Loads and caches user data including receipts and preferences on login.
    /// </summary>
    /// <param name="userId">The user ID to cache data for</param>
    public async Task PreloadUserDataAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Preloading user data for user {UserId}", userId);

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Load and cache recent receipts (last 30 days or most recent 50)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var receipts = await dbContext.Receipts
                .Where(r => r.UserId == userId && r.UploadedAt >= thirtyDaysAgo)
                .OrderByDescending(r => r.UploadedAt)
                .Take(50)
                .Select(r => new ReceiptResponseDto
                {
                    Id = r.Id,
                    FileName = r.FileName,
                    FileType = r.FileType,
                    FileSizeBytes = r.FileSizeBytes,
                    Merchant = r.Merchant,
                    Amount = r.Amount,
                    PurchaseDate = r.PurchaseDate,
                    ProductName = r.ProductName,
                    WarrantyExpirationDate = r.WarrantyExpirationDate,
                    UploadedAt = r.UploadedAt,
                    WarrantyMonths = r.WarrantyMonths,
                    Notes = r.Notes,
                    Description = r.Description,
                    DownloadUrl = string.Empty // Will be set by controller
                })
                .ToListAsync();

            var receiptsCacheKey = $"{UserReceiptsCacheKeyPrefix}{userId}";
            _cache.Set(receiptsCacheKey, receipts, _defaultCacheExpiration);

            _logger.LogInformation("Cached {Count} receipts for user {UserId}", receipts.Count, userId);

            // Load and cache user preferences
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var preferences = new
                {
                    user.NotificationChannel,
                    user.NotificationThresholdDays,
                    user.OptOutOfNotifications,
                    user.PhoneNumber,
                    user.PhoneNumberConfirmed
                };

                var preferencesCacheKey = $"{UserPreferencesCacheKeyPrefix}{userId}";
                _cache.Set(preferencesCacheKey, preferences, _defaultCacheExpiration);

                _logger.LogInformation("Cached preferences for user {UserId}", userId);
            }

            _logger.LogInformation("Successfully preloaded user data for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preloading user data for user {UserId}", userId);
            // Don't throw - caching is a performance optimization, not critical
        }
    }

    /// <summary>
    /// Clears all cached data for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to clear cache for</param>
    public void ClearUserCache(string userId)
    {
        try
        {
            var receiptsCacheKey = $"{UserReceiptsCacheKeyPrefix}{userId}";
            var preferencesCacheKey = $"{UserPreferencesCacheKeyPrefix}{userId}";

            _cache.Remove(receiptsCacheKey);
            _cache.Remove(preferencesCacheKey);

            _logger.LogInformation("Cleared cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Invalidates the receipt cache for a specific user (after receipt changes).
    /// </summary>
    /// <param name="userId">The user ID to invalidate receipt cache for</param>
    public void InvalidateReceiptCache(string userId)
    {
        try
        {
            var receiptsCacheKey = $"{UserReceiptsCacheKeyPrefix}{userId}";
            _cache.Remove(receiptsCacheKey);

            _logger.LogInformation("Invalidated receipt cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating receipt cache for user {UserId}", userId);
        }
    }
}
