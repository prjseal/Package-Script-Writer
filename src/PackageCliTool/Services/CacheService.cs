using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PackageCliTool.Models.Cache;

namespace PackageCliTool.Services;

/// <summary>
/// Service for managing API response caching
/// </summary>
public class CacheService
{
    private readonly string _cacheFile;
    private readonly ILogger? _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private CachedData _cache;
    private readonly int _ttlHours;
    private readonly bool _enabled;

    public CacheService(int ttlHours = 1, bool enabled = true, string? cacheDirectory = null, ILogger? logger = null)
    {
        _ttlHours = ttlHours;
        _enabled = enabled;
        _logger = logger;

        // Default to ~/.psw/cache/
        var cacheDir = cacheDirectory ?? GetDefaultCacheDirectory();
        _cacheFile = Path.Combine(cacheDir, "cache.yaml");

        // Initialize YAML serializer/deserializer
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        EnsureDirectoryExists(cacheDir);
        _cache = LoadCache();
    }

    /// <summary>
    /// Gets the default cache directory path
    /// </summary>
    private static string GetDefaultCacheDirectory()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".psw", "cache");
    }

    /// <summary>
    /// Ensures the cache directory exists
    /// </summary>
    private void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger?.LogInformation("Created cache directory: {Directory}", directory);
        }
    }

    /// <summary>
    /// Loads cache from disk
    /// </summary>
    private CachedData LoadCache()
    {
        if (!_enabled)
        {
            _logger?.LogDebug("Cache is disabled");
            return new CachedData();
        }

        if (!File.Exists(_cacheFile))
        {
            _logger?.LogDebug("Cache file not found, creating new cache");
            return new CachedData();
        }

        try
        {
            var yaml = File.ReadAllText(_cacheFile);
            var cache = _deserializer.Deserialize<CachedData>(yaml);

            // Clean expired entries on load
            CleanExpiredEntries(cache);

            _logger?.LogDebug("Loaded cache with {PackageCount} package entries, {TemplateCount} template entries",
                cache.PackageCache.Count, cache.TemplateVersionCache.Count);

            return cache;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load cache file, creating new cache");
            return new CachedData();
        }
    }

    /// <summary>
    /// Saves cache to disk
    /// </summary>
    private void SaveCache()
    {
        if (!_enabled)
        {
            return;
        }

        try
        {
            _cache.LastModified = DateTime.UtcNow;
            var yaml = _serializer.Serialize(_cache);
            File.WriteAllText(_cacheFile, yaml);
            _logger?.LogDebug("Saved cache to disk");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save cache file");
        }
    }

    /// <summary>
    /// Cleans expired entries from cache
    /// </summary>
    private void CleanExpiredEntries(CachedData cache)
    {
        var originalPackageCount = cache.PackageCache.Count;
        var originalTemplateCount = cache.TemplateVersionCache.Count;
        var originalGenericCount = cache.GenericCache.Count;

        cache.PackageCache = cache.PackageCache
            .Where(kvp => kvp.Value.IsValid())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        cache.TemplateVersionCache = cache.TemplateVersionCache
            .Where(kvp => kvp.Value.IsValid())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        cache.GenericCache = cache.GenericCache
            .Where(kvp => kvp.Value.IsValid())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var removedCount = (originalPackageCount - cache.PackageCache.Count) +
                          (originalTemplateCount - cache.TemplateVersionCache.Count) +
                          (originalGenericCount - cache.GenericCache.Count);

        if (removedCount > 0)
        {
            _logger?.LogDebug("Cleaned {Count} expired cache entries", removedCount);
        }
    }

    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    public string? Get(string key, CacheType type = CacheType.Generic)
    {
        if (!_enabled)
        {
            return null;
        }

        var cache = GetCacheForType(type);

        if (cache.TryGetValue(key, out var entry) && entry.IsValid())
        {
            _logger?.LogDebug("Cache hit for key: {Key} (expires in {Minutes} minutes)",
                key, entry.TimeRemaining().TotalMinutes);
            return entry.Data;
        }

        _logger?.LogDebug("Cache miss for key: {Key}", key);
        return null;
    }

    /// <summary>
    /// Sets a cached value with TTL
    /// </summary>
    public void Set(string key, string value, CacheType type = CacheType.Generic, int? ttlHours = null)
    {
        if (!_enabled)
        {
            return;
        }

        var effectiveTtl = ttlHours ?? _ttlHours;
        var cache = GetCacheForType(type);

        var entry = new CacheEntry<string>
        {
            Key = key,
            Data = value,
            CachedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(effectiveTtl)
        };

        cache[key] = entry;
        SaveCache();

        _logger?.LogDebug("Cached value for key: {Key} (expires at {ExpiresAt})", key, entry.ExpiresAt);
    }

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    public void Clear()
    {
        _cache.PackageCache.Clear();
        _cache.TemplateVersionCache.Clear();
        _cache.GenericCache.Clear();
        SaveCache();
        _logger?.LogInformation("Cleared all cache entries");
    }

    /// <summary>
    /// Clears cache of a specific type
    /// </summary>
    public void Clear(CacheType type)
    {
        var cache = GetCacheForType(type);
        var count = cache.Count;
        cache.Clear();
        SaveCache();
        _logger?.LogInformation("Cleared {Count} {Type} cache entries", count, type);
    }

    /// <summary>
    /// Gets the appropriate cache dictionary for the given type
    /// </summary>
    private Dictionary<string, CacheEntry<string>> GetCacheForType(CacheType type)
    {
        return type switch
        {
            CacheType.Package => _cache.PackageCache,
            CacheType.TemplateVersion => _cache.TemplateVersionCache,
            CacheType.Generic => _cache.GenericCache,
            _ => _cache.GenericCache
        };
    }

    /// <summary>
    /// Checks if cache is enabled
    /// </summary>
    public bool IsEnabled => _enabled;
}

/// <summary>
/// Type of cache storage
/// </summary>
public enum CacheType
{
    Package,
    TemplateVersion,
    Generic
}