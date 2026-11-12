using System.IO.Compression;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NuGet.Versioning;

using PSW.Shared.Models.NuGet;

namespace PSW.Shared.Services;

/// <summary>
/// Resolves package versions compatible with an Umbraco version by reading package dependencies
/// from the NuGet registration API. A package version is compatible when one of its Umbraco.Cms.*
/// dependency ranges accepts the Umbraco version. Meta-packages without a direct Umbraco.Cms.*
/// dependency (e.g. uSync) are resolved through their own dependencies.
/// <para>
/// Many packages declare open-ended ranges (e.g. Clean 8 depends on Umbraco [18.3.1, )), so an older package major
/// also "accepts" a newer Umbraco major. A version built for the same Umbraco major (its range's lower bound has the
/// same major as the Umbraco version) is therefore preferred: stable first, then a prerelease, even when the Umbraco
/// version itself is stable. Only when no version matches the major does it fall back to the newest version whose
/// range accepts the Umbraco version, considering prereleases only for a prerelease Umbraco version.
/// </para>
/// </summary>
public class PackageCompatibilityService : IPackageCompatibilityService
{
    private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2/";
    private const int MaxDependencyDepth = 2;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    private readonly IHttpClientFactory _clientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PackageCompatibilityService> _logger;

    public PackageCompatibilityService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<PackageCompatibilityService>? logger = null)
    {
        _clientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger ?? NullLogger<PackageCompatibilityService>.Instance;
    }

    public async Task<PackageCompatibilityResult> GetCompatibleVersionAsync(string packageId, string umbracoVersion)
    {
        if (string.IsNullOrWhiteSpace(packageId) || !NuGetVersion.TryParse(umbracoVersion, out var umbraco))
        {
            return PackageCompatibilityResult.Unknown;
        }

        var versions = await GetPackageVersionsAsync(packageId);

        // Stable versions first, so a stable same-major match always beats a prerelease one
        var candidates = versions
            .Where(x => x.Listed)
            .OrderBy(x => x.Version.IsPrerelease)
            .ThenByDescending(x => x.Version);

        PackageVersionInfo? fallback = null;
        var foundIncompatible = false;
        foreach (var candidate in candidates)
        {
            var compatibility = await IsCompatibleAsync(candidate, umbraco, 0);
            if (compatibility == Compatibility.CompatibleSameMajor)
            {
                return new PackageCompatibilityResult(PackageCompatibilityStatus.Compatible, candidate.OriginalVersion);
            }

            // Fallback for when no version targets the Umbraco major: prereleases only for a prerelease Umbraco
            if (compatibility == Compatibility.Compatible && fallback == null
                && (!candidate.Version.IsPrerelease || umbraco.IsPrerelease))
            {
                fallback = candidate;
            }

            foundIncompatible |= compatibility == Compatibility.Incompatible;
        }

        if (fallback != null)
        {
            return new PackageCompatibilityResult(PackageCompatibilityStatus.Compatible, fallback.OriginalVersion);
        }

        return foundIncompatible ? PackageCompatibilityResult.NoCompatibleVersion : PackageCompatibilityResult.Unknown;
    }

    /// <returns>Whether an Umbraco.Cms dependency accepts the Umbraco version, and whether it was built for the same
    /// Umbraco major. Unknown when no Umbraco.Cms dependency was found.</returns>
    private async Task<Compatibility> IsCompatibleAsync(PackageVersionInfo packageVersion, NuGetVersion umbraco, int depth)
    {
        var umbracoDependencies = packageVersion.Dependencies.Where(x => IsUmbracoCmsPackage(x.Id)).ToList();
        if (umbracoDependencies.Count > 0)
        {
            var satisfying = umbracoDependencies.Where(x => x.Range.Satisfies(umbraco)).ToList();
            if (satisfying.Count == 0) return Compatibility.Incompatible;

            return satisfying.Any(x => x.Range.HasLowerBound && x.Range.MinVersion!.Major == umbraco.Major)
                ? Compatibility.CompatibleSameMajor
                : Compatibility.Compatible;
        }

        if (depth >= MaxDependencyDepth) return Compatibility.Unknown;

        foreach (var dependency in packageVersion.Dependencies.Where(x => !IsFrameworkPackage(x.Id)))
        {
            // NuGet resolves the lowest version that satisfies the range
            var dependencyVersions = await GetPackageVersionsAsync(dependency.Id);
            var resolved = dependencyVersions
                .Where(x => dependency.Range.Satisfies(x.Version))
                .MinBy(x => x.Version);

            if (resolved == null) continue;

            var compatibility = await IsCompatibleAsync(resolved, umbraco, depth + 1);
            if (compatibility != Compatibility.Unknown) return compatibility;
        }

        return Compatibility.Unknown;
    }

    private enum Compatibility
    {
        /// <summary>No Umbraco.Cms dependency was found, so compatibility can't be determined.</summary>
        Unknown,
        Incompatible,
        /// <summary>A dependency range accepts the Umbraco version, but was built for an older Umbraco major.</summary>
        Compatible,
        /// <summary>A dependency range accepts the Umbraco version and its lower bound has the same major.</summary>
        CompatibleSameMajor
    }

    private static bool IsUmbracoCmsPackage(string packageId) =>
        packageId.StartsWith("Umbraco.Cms", StringComparison.OrdinalIgnoreCase);

    private static bool IsFrameworkPackage(string packageId) =>
        packageId.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
        || packageId.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
        || packageId.Equals("NETStandard.Library", StringComparison.OrdinalIgnoreCase);

    private async Task<List<PackageVersionInfo>> GetPackageVersionsAsync(string packageId)
    {
        var cacheKey = $"nuget-registration-{packageId.ToLowerInvariant()}";
        if (_memoryCache.TryGetValue(cacheKey, out List<PackageVersionInfo>? cached) && cached != null)
        {
            return cached;
        }

        var (versions, complete) = await FetchPackageVersionsAsync(packageId);

        // Don't cache failed or partial lookups, so a transient NuGet outage doesn't stick for an hour
        if (complete)
        {
            _memoryCache.Set(cacheKey, versions, CacheDuration);
        }

        return versions;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Compatibility lookup is best effort; failures fall back to an unpinned package")]
    /// <returns>The versions found, and whether the index and every page were read. A partial list is still usable
    /// (any version it pins is compatible, if not the newest) but must not be cached.</returns>
    private async Task<(List<PackageVersionInfo> Versions, bool Complete)> FetchPackageVersionsAsync(string packageId)
    {
        try
        {
            var client = _clientFactory.CreateClient();
            var index = await GetJsonAsync<NuGetRegistrationIndex>(client, packageId, $"{RegistrationBaseUrl}{packageId.ToLowerInvariant()}/index.json");
            if (index == null) return (new List<PackageVersionInfo>(), false);

            var pages = await Task.WhenAll(index.Items.Select(async page =>
                page.Items ?? (await GetJsonAsync<NuGetRegistrationPage>(client, packageId, page.Id))?.Items));

            var versions = pages
                .SelectMany(x => x ?? new List<NuGetRegistrationLeaf>())
                .Select(x => PackageVersionInfo.FromCatalogEntry(x.CatalogEntry))
                .OfType<PackageVersionInfo>()
                .ToList();

            return (versions, pages.All(x => x != null));
        }
        catch (Exception ex)
        {
            LogFailure(packageId, $"{ex.GetType().Name}: {ex.Message}", ex);
            return (new List<PackageVersionInfo>(), false);
        }
    }

    private async Task<T?> GetJsonAsync<T>(HttpClient client, string packageId, string url)
    {
        using var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            LogFailure(packageId, $"{url} returned {(int)response.StatusCode} {response.StatusCode}");
            return default;
        }

        var stream = await response.Content.ReadAsStreamAsync();

        // The registration5-gz endpoints are always gzipped; decompress unless the handler already did
        if (response.Content.Headers.ContentEncoding.Contains("gzip", StringComparer.OrdinalIgnoreCase))
        {
            stream = new GZipStream(stream, CompressionMode.Decompress);
        }

        await using (stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }

    private void LogFailure(string packageId, string reason, Exception? exception = null) =>
        _logger.LogWarning(exception, "NuGet compatibility lookup for {PackageId} failed: {Reason}", packageId, reason);

    private sealed record PackageVersionInfo(NuGetVersion Version, string OriginalVersion, bool Listed, List<(string Id, VersionRange Range)> Dependencies)
    {
        public static PackageVersionInfo? FromCatalogEntry(NuGetCatalogEntry entry)
        {
            if (!NuGetVersion.TryParse(entry.Version, out var version)) return null;

            var dependencies = (entry.DependencyGroups ?? new List<NuGetDependencyGroup>())
                .SelectMany(x => x.Dependencies ?? new List<NuGetDependency>())
                .Select(x => (x.Id, Range: VersionRange.TryParse(x.Range ?? "", out var range) ? range : VersionRange.All))
                .ToList();

            return new PackageVersionInfo(version, entry.Version, entry.Listed, dependencies);
        }
    }
}
