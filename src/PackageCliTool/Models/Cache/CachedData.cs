namespace PackageCliTool.Models.Cache;

/// <summary>
/// Root container for all cached data
/// </summary>
public class CachedData
{
    /// <summary>
    /// Cache format version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Default TTL in hours
    /// </summary>
    public int DefaultTtlHours { get; set; } = 1;

    /// <summary>
    /// Package list cache entries (keyed by API endpoint/query)
    /// </summary>
    public Dictionary<string, CacheEntry<string>> PackageCache { get; set; } = new();

    /// <summary>
    /// Template version cache entries
    /// </summary>
    public Dictionary<string, CacheEntry<string>> TemplateVersionCache { get; set; } = new();

    /// <summary>
    /// Generic string cache entries for other API responses
    /// </summary>
    public Dictionary<string, CacheEntry<string>> GenericCache { get; set; } = new();

    /// <summary>
    /// When the cache file was last modified
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
}
