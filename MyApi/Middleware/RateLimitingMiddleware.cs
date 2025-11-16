using System.Net;

namespace MyApi.Middleware;

/// <summary>
/// Rate limiting middleware to prevent API abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly Dictionary<string, List<DateTime>> _requestLog = new();
    private static readonly object _lock = new();
    private const int MaxRequestsPerMinute = 60;
    private const int MaxRequestsPerHour = 1000;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User.Identity?.Name ?? clientIp;
        var identifier = $"{userId}_{clientIp}";

        // Skip rate limiting for health checks
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        bool limitExceeded = false;
        string errorMessage = string.Empty;
        string retryAfter = string.Empty;

        lock (_lock)
        {
            if (!_requestLog.ContainsKey(identifier))
            {
                _requestLog[identifier] = new List<DateTime>();
            }

            var now = DateTime.UtcNow;
            var requests = _requestLog[identifier];

            // Remove requests older than 1 hour
            requests.RemoveAll(time => (now - time).TotalHours > 1);

            // Check rate limits
            var requestsLastMinute = requests.Count(time => (now - time).TotalMinutes <= 1);
            var requestsLastHour = requests.Count;

            if (requestsLastMinute >= MaxRequestsPerMinute)
            {
                _logger.LogWarning("Rate limit exceeded (per minute) for {Identifier}", identifier);
                limitExceeded = true;
                errorMessage = "Rate limit exceeded. Maximum 60 requests per minute.";
                retryAfter = "60";
            }
            else if (requestsLastHour >= MaxRequestsPerHour)
            {
                _logger.LogWarning("Rate limit exceeded (per hour) for {Identifier}", identifier);
                limitExceeded = true;
                errorMessage = "Rate limit exceeded. Maximum 1000 requests per hour.";
                retryAfter = "3600";
            }
            else
            {
                // Add current request
                requests.Add(now);
            }
        }

        if (limitExceeded)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = retryAfter;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"message\":\"{errorMessage}\"}}");
            return;
        }

        await _next(context);
    }
}

