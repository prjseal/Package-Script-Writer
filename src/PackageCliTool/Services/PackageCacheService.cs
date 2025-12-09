using Microsoft.Extensions.Logging;
using System.Text.Json;
using PSW.Shared.Models;

namespace PackageCliTool.Services;

/// <summary>
/// Service for managing file-based package caching
/// </summary>
public class PackageCacheService
{
    private readonly string _cacheFile;
    private readonly ILogger? _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public PackageCacheService(ILogger? logger = null)
    {
        _logger = logger;

        // Store cache file next to the executable
        var baseDir = AppContext.BaseDirectory;
        _cacheFile = Path.Combine(baseDir, "packages-cache.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        _logger?.LogDebug("Package cache file location: {CacheFile}", _cacheFile);
    }

    /// <summary>
    /// Checks if the cache file exists
    /// </summary>
    public bool CacheExists()
    {
        return File.Exists(_cacheFile);
    }

    /// <summary>
    /// Gets the cached packages from file
    /// </summary>
    public List<PagedPackagesPackage>? GetCachedPackages()
    {
        if (!File.Exists(_cacheFile))
        {
            _logger?.LogDebug("Package cache file not found");
            return null;
        }

        try
        {
            var json = File.ReadAllText(_cacheFile);
            var packages = JsonSerializer.Deserialize<List<PagedPackagesPackage>>(json, _jsonOptions);

            _logger?.LogInformation("Loaded {Count} packages from cache file", packages?.Count ?? 0);
            return packages;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to read package cache file");
            return null;
        }
    }

    /// <summary>
    /// Saves packages to the cache file
    /// </summary>
    public void SavePackages(List<PagedPackagesPackage> packages)
    {
        try
        {
            var json = JsonSerializer.Serialize(packages, _jsonOptions);
            File.WriteAllText(_cacheFile, json);

            _logger?.LogInformation("Saved {Count} packages to cache file: {CacheFile}", packages.Count, _cacheFile);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save package cache file");
        }
    }

    /// <summary>
    /// Clears the package cache file
    /// </summary>
    public void ClearCache()
    {
        try
        {
            if (File.Exists(_cacheFile))
            {
                File.Delete(_cacheFile);
                _logger?.LogInformation("Deleted package cache file: {CacheFile}", _cacheFile);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete package cache file");
        }
    }

    /// <summary>
    /// Gets the cache file path
    /// </summary>
    public string GetCacheFilePath()
    {
        return _cacheFile;
    }
}
