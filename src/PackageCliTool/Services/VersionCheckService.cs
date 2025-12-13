using Microsoft.Extensions.Logging;
using PackageCliTool.Configuration;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PackageCliTool.Services;

/// <summary>
/// Service for checking if a newer version of the CLI tool is available
/// </summary>
public class VersionCheckService
{
    private const string NuGetApiBaseUrl = "https://api.nuget.org/v3/registration5-semver1";
    private const string PackageId = "PackageScriptWriter.Cli";
    private readonly HttpClient _httpClient;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionCheckService"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for NuGet API calls</param>
    /// <param name="logger">Optional logger instance</param>
    public VersionCheckService(HttpClient httpClient, ILogger? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a newer version is available on NuGet
    /// </summary>
    /// <returns>Version information if a newer version is available, null otherwise</returns>
    public async Task<VersionCheckResult?> CheckForUpdateAsync()
    {
        try
        {
            _logger?.LogDebug("Checking for updates to {PackageId}", PackageId);

            var url = $"{NuGetApiBaseUrl}/{PackageId.ToLowerInvariant()}/index.json";
            var response = await _httpClient.GetFromJsonAsync<NuGetRegistrationResponse>(url);

            if (response?.Items == null || response.Items.Count == 0)
            {
                _logger?.LogWarning("No package information found on NuGet");
                return null;
            }

            // Get all versions from the catalog
            var allVersions = new List<string>();
            foreach (var item in response.Items)
            {
                if (item.Items != null)
                {
                    allVersions.AddRange(item.Items.Select(v => v.CatalogEntry?.Version).Where(v => v != null)!);
                }
            }

            if (allVersions.Count == 0)
            {
                _logger?.LogWarning("No versions found in NuGet package catalog");
                return null;
            }

            // Get the latest stable version (exclude prerelease)
            var latestVersion = allVersions
                .Where(v => !v.Contains("-"))
                .OrderByDescending(v => ParseVersion(v))
                .FirstOrDefault();

            // If no stable version, get latest prerelease
            if (latestVersion == null)
            {
                latestVersion = allVersions
                    .OrderByDescending(v => ParseVersion(v))
                    .FirstOrDefault();
            }

            if (latestVersion == null)
            {
                _logger?.LogWarning("Could not determine latest version");
                return null;
            }

            // get current version from current assembly
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            _logger?.LogDebug("Current version: {Current}, Latest version: {Latest}", currentVersion, latestVersion);

            // Compare versions
            if (IsNewerVersion(latestVersion, currentVersion))
            {
                _logger?.LogInformation("Newer version available: {Latest}", latestVersion);
                return new VersionCheckResult
                {
                    IsUpdateAvailable = true,
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    UpdateCommand = $"dotnet tool update -g {PackageId}"
                };
            }

            _logger?.LogDebug("No update available. Current version is up to date.");
            return new VersionCheckResult
            {
                IsUpdateAvailable = false,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                UpdateCommand = $"dotnet tool update -g {PackageId}"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogDebug(ex, "Failed to check for updates (network error)");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "Failed to check for updates");
            return null;
        }
    }

    /// <summary>
    /// Compares two version strings to determine if the first is newer than the second
    /// </summary>
    private bool IsNewerVersion(string version1, string version2)
    {
        var v1 = ParseVersion(version1);
        var v2 = ParseVersion(version2);

        // Compare major.minor.patch
        if (v1.Major != v2.Major) return v1.Major > v2.Major;
        if (v1.Minor != v2.Minor) return v1.Minor > v2.Minor;
        if (v1.Patch != v2.Patch) return v1.Patch > v2.Patch;

        // If both are stable, they're equal
        if (string.IsNullOrEmpty(v1.Prerelease) && string.IsNullOrEmpty(v2.Prerelease))
            return false;

        // Stable version is newer than prerelease
        if (string.IsNullOrEmpty(v1.Prerelease) && !string.IsNullOrEmpty(v2.Prerelease))
            return true;

        if (!string.IsNullOrEmpty(v1.Prerelease) && string.IsNullOrEmpty(v2.Prerelease))
            return false;

        // Both are prerelease, compare prerelease strings
        return string.Compare(v1.Prerelease, v2.Prerelease, StringComparison.OrdinalIgnoreCase) > 0;
    }

    /// <summary>
    /// Parses a version string into its components
    /// </summary>
    private (int Major, int Minor, int Patch, string Prerelease) ParseVersion(string version)
    {
        var parts = version.Split('-', 2);
        var versionPart = parts[0];
        var prereleasePart = parts.Length > 1 ? parts[1] : string.Empty;

        var versionNumbers = versionPart.Split('.');
        var major = versionNumbers.Length > 0 ? int.TryParse(versionNumbers[0], out var m) ? m : 0 : 0;
        var minor = versionNumbers.Length > 1 ? int.TryParse(versionNumbers[1], out var n) ? n : 0 : 0;
        var patch = versionNumbers.Length > 2 ? int.TryParse(versionNumbers[2], out var p) ? p : 0 : 0;

        return (major, minor, patch, prereleasePart);
    }
}

/// <summary>
/// Result of a version check operation
/// </summary>
public class VersionCheckResult
{
    /// <summary>Gets or sets whether a newer version is available</summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>Gets or sets the current installed version</summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the latest available version</summary>
    public string LatestVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the command to update to the latest version</summary>
    public string UpdateCommand { get; set; } = string.Empty;
}

/// <summary>
/// NuGet API response models
/// </summary>
internal class NuGetRegistrationResponse
{
    [JsonPropertyName("items")]
    public List<NuGetRegistrationPage>? Items { get; set; }
}

internal class NuGetRegistrationPage
{
    [JsonPropertyName("items")]
    public List<NuGetRegistrationLeaf>? Items { get; set; }
}

internal class NuGetRegistrationLeaf
{
    [JsonPropertyName("catalogEntry")]
    public NuGetCatalogEntry? CatalogEntry { get; set; }
}

internal class NuGetCatalogEntry
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
