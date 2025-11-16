namespace MyApi.Services;

/// <summary>
/// Service interface for caching user-specific data to improve performance.
/// </summary>
public interface IUserCacheService
{
    /// <summary>
    /// Loads and caches user data including receipts and preferences.
    /// </summary>
    /// <param name="userId">The user ID to cache data for</param>
    Task PreloadUserDataAsync(string userId);

    /// <summary>
    /// Clears cached data for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to clear cache for</param>
    void ClearUserCache(string userId);

    /// <summary>
    /// Invalidates the receipt cache for a specific user (after receipt changes).
    /// </summary>
    /// <param name="userId">The user ID to invalidate receipt cache for</param>
    void InvalidateReceiptCache(string userId);
}
