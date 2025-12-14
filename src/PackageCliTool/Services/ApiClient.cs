using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PackageCliTool.Models;
using PackageCliTool.Models.Api;
using PackageCliTool.Exceptions;
using PSW.Shared.Services;

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
    private readonly IPackageService? _packageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient"/> class
    /// </summary>
    /// <param name="baseUrl">The base URL for API requests</param>
    /// <param name="logger">Optional logger instance</param>
    /// <param name="cacheService">Optional cache service for caching API responses</param>
    /// <param name="packageService">Optional package service for NuGet operations</param>
    public ApiClient(string baseUrl, ILogger? logger = null, CacheService? cacheService = null, IPackageService? packageService = null)
    {
        _baseUrl = baseUrl;
        _logger = logger;
        _cacheService = cacheService;
        _packageService = packageService;

        // Create IPv4-only SocketsHttpHandler to avoid IPv6 timeout issues
        // Diagnostic testing showed IPv6 connections timeout after ~42 seconds
        // while IPv4 connections complete in ~150ms
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                // Force IPv4 to avoid ~42 second IPv6 timeout
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                try
                {
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            EnableMultipleHttp2Connections = true
        };

        var httpClient = new HttpClient(handler, disposeHandler: true)
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

        // Cache the successful response (1 hour TTL - default)
        if (versions.Any())
        {
            var versionsJson = JsonSerializer.Serialize(versions);
            _cacheService?.Set(cacheKey, versionsJson, CacheType.Package);
        }

        return versions;
    }

    /// <summary>
    /// Retrieves all available Umbraco packages from the marketplace
    /// </summary>
    public async Task<List<PSW.Shared.Models.PagedPackagesPackage>> GetAllPackagesAsync()
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
                var cachedPackages = JsonSerializer.Deserialize<List<PSW.Shared.Models.PagedPackagesPackage>>(cachedResponse,
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
            var packages = JsonSerializer.Deserialize<List<PSW.Shared.Models.PagedPackagesPackage>>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var packageList = packages ?? new List<PSW.Shared.Models.PagedPackagesPackage>();
            var distinctPackages = packageList
                .GroupBy(p => p.PackageId)
                .Select(g => g.First())
                .ToList();
            _logger?.LogInformation("Successfully fetched {Count} packages from marketplace", distinctPackages.Count);

            // Cache the successful response (24 hour TTL)
            if (distinctPackages.Any())
            {
                var packagesJson = JsonSerializer.Serialize(distinctPackages,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                _cacheService?.Set(cacheKey, packagesJson, CacheType.Package, ttlHours: 24);
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

    /// <summary>
    /// Retrieves available versions for a template from NuGet (includes prerelease versions)
    /// </summary>
    /// <param name="templateId">The template package ID (e.g., "Umbraco.Templates")</param>
    /// <returns>List of available versions including prereleases</returns>
    public async Task<List<string>> GetTemplateVersionsAsync(string templateId)
    {
        if (_packageService == null)
        {
            _logger?.LogWarning("PackageService not available, cannot fetch template versions");
            return new List<string>();
        }

        // Generate cache key
        var cacheKey = $"template_versions_{templateId}";

        // Check cache first
        var cachedResponse = _cacheService?.Get(cacheKey, CacheType.TemplateVersion);
        if (cachedResponse != null)
        {
            _logger?.LogDebug("Using cached template versions for {TemplateId}", templateId);
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
                _logger?.LogWarning(ex, "Failed to deserialize cached template versions, fetching fresh data");
            }
        }

        _logger?.LogInformation("Fetching template versions for {TemplateId} from NuGet (includes prereleases)", templateId);

        try
        {
            var templateUniqueId = templateId.ToLower();
            // NuGet flat container API returns ALL versions including prereleases
            var versions = await _packageService.GetNugetPackageVersionsAsync(
                $"https://api.nuget.org/v3-flatcontainer/{templateUniqueId}/index.json");

            var versionList = versions ?? new List<string>();
            _logger?.LogInformation("Found {Count} versions for template {TemplateId}", versionList.Count, templateId);

            // Cache the successful response (1 hour TTL)
            if (versionList.Any())
            {
                var versionsJson = JsonSerializer.Serialize(versionList);
                _cacheService?.Set(cacheKey, versionsJson, CacheType.TemplateVersion, ttlHours: 1);
            }

            return versionList;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to fetch template versions for {TemplateId}", templateId);
            return new List<string>();
        }
    }
}
