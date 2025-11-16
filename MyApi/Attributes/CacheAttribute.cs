using System;

namespace MyApi.Attributes;

/// <summary>
/// Attribute to enable response caching for GET endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// Cache duration in seconds
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Cache location (Any, Client, None)
    /// </summary>
    public string Location { get; set; } = "Any";

    /// <summary>
    /// Vary by query parameters
    /// </summary>
    public string VaryByQueryKeys { get; set; } = "*";

    public CacheAttribute(int durationSeconds = 60)
    {
        Duration = durationSeconds;
    }
}
