using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MyApi.Services;
using System.Security.Claims;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WarrantyNotificationsController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WarrantyNotificationsController> _logger;
    private const string CacheKey = "warranty_expiration_cache";

    public WarrantyNotificationsController(
        IMemoryCache cache,
        ILogger<WarrantyNotificationsController> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Get warranties expiring soon for the current user
    /// </summary>
    [HttpGet("expiring")]
    public ActionResult<IEnumerable<WarrantyNotification>> GetExpiringWarranties()
    {
        var userId = GetUserId();
        
        var allExpiringWarranties = _cache.Get<List<WarrantyNotification>>(CacheKey) 
            ?? new List<WarrantyNotification>();

        // Filter to only the current user's warranties
        var userWarranties = allExpiringWarranties
            .Where(w => w.UserId == userId)
            .OrderBy(w => w.ExpirationDate)
            .ToList();

        _logger.LogInformation("User {UserId} retrieved {Count} expiring warranties", userId, userWarranties.Count);

        return Ok(userWarranties);
    }

    /// <summary>
    /// Get count of warranties expiring soon for the current user
    /// </summary>
    [HttpGet("expiring/count")]
    public ActionResult<int> GetExpiringWarrantiesCount()
    {
        var userId = GetUserId();
        
        var allExpiringWarranties = _cache.Get<List<WarrantyNotification>>(CacheKey) 
            ?? new List<WarrantyNotification>();

        var count = allExpiringWarranties.Count(w => w.UserId == userId);

        return Ok(new { count, userId });
    }
}
