namespace PackageCliTool.Models.Cache;

/// <summary>
/// Represents a single cache entry with expiration tracking
/// </summary>
/// <typeparam name="T">Type of data being cached</typeparam>
public class CacheEntry<T>
{
    /// <summary>
    /// The cached data
    /// </summary>
    public T Data { get; set; } = default!;

    /// <summary>
    /// When this entry was cached
    /// </summary>
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this entry expires (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Cache key identifier
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// Checks if this cache entry is still valid
    /// </summary>
    public bool IsValid()
    {
        return DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Gets the remaining time until expiration
    /// </summary>
    public TimeSpan TimeRemaining()
    {
        var remaining = ExpiresAt - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
