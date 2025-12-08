using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Services;

/// <summary>
/// API client for communicating with the Package Script Writer API
/// </summary>
public class ApiClient
{
    private readonly ResilientHttpClient _resilientClient;
    private readonly string _baseUrl;
    private readonly ILogger? _logger;
    private readonly CacheService? _cacheService;

    public ApiClient(string baseUrl, ILogger? logger = null, CacheService? cacheService = null)
    {
        _baseUrl = baseUrl;
        _logger = logger;
        _cacheService = cacheService;

        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(90)
        };

        _resilientClient = new ResilientHttpClient(httpClient, logger, maxRetries: 3);
    }

    /// <summary>
    /// Fetches available versions for a specific package from the API
    /// </summary>
    public async Task<List<string>> GetPackageVersionsAsync(string packageId, bool includePrerelease = false)
    {
        // Generate cache key
        var cacheKey = $"package_versions_{packageId}_{includePrerelease}";

        // Check cache first
        var cachedResponse = _cacheService?.Get(cacheKey, CacheType.Package);
        if (cachedResponse != null)
        {
            _logger?.LogDebug("Using cached package versions for {PackageId}", packageId);
            try
            {
                var cachedVersions = JsonSerializer.Deserialize<List<string>>(cachedResponse);
                if (cachedVersions != null)
                {
                    return cachedVersions;
                }
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize cached package versions, fetching fresh data");
            }
        }

        _logger?.LogInformation("Fetching package versions for {PackageId} from API", packageId);

        var request = new PackageVersionRequest
        {
            PackageId = packageId,
            IncludePrerelease = includePrerelease
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _resilientClient.PostAsync("/api/scriptgeneratorapi/getpackageversions", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger?.LogDebug("Received package versions response with length {Length}", responseContent.Length);

        List<string> versions;

        // Try to deserialize as a raw array first
        try
        {
            var deserializedVersions = JsonSerializer.Deserialize<List<string>>(responseContent);
            if (deserializedVersions != null)
            {
                versions = deserializedVersions;
                _logger?.LogInformation("Found {Count} versions for package {PackageId}", versions.Count, packageId);
            }
            else
            {
                versions = new List<string>();
            }
        }
        catch (JsonException)
        {
            // Fallback to object with 'versions' property
            try
            {
                var result = JsonSerializer.Deserialize<PackageVersionResponse>(responseContent);
                versions = result?.Versions ?? new List<string>();
                _logger?.LogInformation("Found {Count} versions for package {PackageId}", versions.Count, packageId);
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to deserialize package versions response");
                throw new ApiException(
                    $"Invalid response format when fetching versions for '{packageId}'",
                    ex,
                    null,
                    "The API returned data in an unexpected format. Try again later or contact support."
                );
            }
        }

        // Cache the successful response
        if (versions.Any())
        {
            var versionsJson = JsonSerializer.Serialize(versions);
            _cacheService?.Set(cacheKey, versionsJson, CacheType.Package);
        }

        return versions;
    }

    /// <summary>
    /// Generates an installation script using the API
    /// </summary>
    public async Task<string> GenerateScriptAsync(ScriptModel request)
    {
        _logger?.LogInformation("Generating installation script via API");

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _resilientClient.PostAsync("/api/scriptgeneratorapi/generatescript", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger?.LogDebug("Received script response with length {Length}", responseContent.Length);

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            _logger?.LogWarning("Received empty script response from API");
            throw new ApiException(
                "API returned an empty script",
                null,
                "The API did not return a valid script. Try again or check your configuration."
            );
        }

        return responseContent;
    }

    /// <summary>
    /// Retrieves all available Umbraco packages from the marketplace
    /// </summary>
    public async Task<List<PagedPackagesPackage>> GetAllPackagesAsync()
    {
        // Generate cache key
        var cacheKey = "all_packages";

        // Check cache first
        var cachedResponse = _cacheService?.Get(cacheKey, CacheType.Package);
        if (cachedResponse != null)
        {
            _logger?.LogDebug("Using cached package list");
            try
            {
                var cachedPackages = JsonSerializer.Deserialize<List<PagedPackagesPackage>>(cachedResponse,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (cachedPackages != null)
                {
                    _logger?.LogInformation("Loaded {Count} packages from cache", cachedPackages.Count);
                    return cachedPackages;
                }
            }
            catch (JsonException ex)
            {
                _logger?.LogWarning(ex, "Failed to deserialize cached packages, fetching fresh data");
            }
        }

        _logger?.LogInformation("Fetching all packages from marketplace API");

        var response = await _resilientClient.GetAsync("/api/scriptgeneratorapi/getallpackages");
        var responseContent = await response.Content.ReadAsStringAsync();

        _logger?.LogDebug("Received packages response with length {Length}", responseContent.Length);

        try
        {
            var packages = JsonSerializer.Deserialize<List<PagedPackagesPackage>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var packageList = packages ?? new List<PagedPackagesPackage>();
            var distinctPackages = packageList
                .GroupBy(p => p.PackageId)
                .Select(g => g.First())
                .ToList();
            _logger?.LogInformation("Successfully fetched {Count} packages from marketplace", distinctPackages.Count);

            // Cache the successful response
            if (distinctPackages.Any())
            {
                var packagesJson = JsonSerializer.Serialize(distinctPackages,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                _cacheService?.Set(cacheKey, packagesJson, CacheType.Package);
            }

            return distinctPackages;
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex, "Failed to deserialize packages response");
            throw new ApiException(
                "Invalid response format when fetching packages",
                ex,
                null,
                "The API returned data in an unexpected format. Try again later or contact support."
            );
        }
    }
}
