using System.IO.Compression;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;

using NuGet.Versioning;

using PSW.Shared.Models.NuGet;

namespace PSW.Shared.Services;

/// <summary>
/// Resolves package versions compatible with an Umbraco version by reading package dependencies
/// from the NuGet registration API. A package version is compatible when one of its Umbraco.Cms.*
/// dependency ranges accepts the Umbraco version. Meta-packages without a direct Umbraco.Cms.*
/// dependency (e.g. uSync) are resolved through their own dependencies.
/// </summary>
public class PackageCompatibilityService : IPackageCompatibilityService
{
    private const string RegistrationBaseUrl = "https://api.nuget.org/v3/registration5-gz-semver2/";
    private const int MaxDependencyDepth = 2;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);

    private readonly IHttpClientFactory _clientFactory;
    private readonly IMemoryCache _memoryCache;

    public PackageCompatibilityService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _clientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task<PackageCompatibilityResult> GetCompatibleVersionAsync(string packageId, string umbracoVersion)
    {
        if (string.IsNullOrWhiteSpace(packageId) || !NuGetVersion.TryParse(umbracoVersion, out var umbraco))
        {
            return PackageCompatibilityResult.Unknown;
        }

        var versions = await GetPackageVersionsAsync(packageId);

        // Stable versions first; prereleases are only considered for a prerelease Umbraco version
        var candidates = versions
            .Where(x => x.Listed && (!x.Version.IsPrerelease || umbraco.IsPrerelease))
            .OrderBy(x => x.Version.IsPrerelease)
            .ThenByDescending(x => x.Version);

        var foundIncompatible = false;
        foreach (var candidate in candidates)
        {
            var isCompatible = await IsCompatibleAsync(candidate, umbraco, 0);
            if (isCompatible == true)
            {
                return new PackageCompatibilityResult(PackageCompatibilityStatus.Compatible, candidate.OriginalVersion);
            }

            foundIncompatible |= isCompatible == false;
        }

        return foundIncompatible ? PackageCompatibilityResult.NoCompatibleVersion : PackageCompatibilityResult.Unknown;
    }

    /// <returns>true/false when an Umbraco.Cms dependency was found, null when compatibility can't be determined.</returns>
    private async Task<bool?> IsCompatibleAsync(PackageVersionInfo packageVersion, NuGetVersion umbraco, int depth)
    {
        var umbracoDependencies = packageVersion.Dependencies.Where(x => IsUmbracoCmsPackage(x.Id)).ToList();
        if (umbracoDependencies.Count > 0)
        {
            return umbracoDependencies.Any(x => x.Range.Satisfies(umbraco));
        }

        if (depth >= MaxDependencyDepth) return null;

        foreach (var dependency in packageVersion.Dependencies.Where(x => !IsFrameworkPackage(x.Id)))
        {
            // NuGet resolves the lowest version that satisfies the range
            var dependencyVersions = await GetPackageVersionsAsync(dependency.Id);
            var resolved = dependencyVersions
                .Where(x => dependency.Range.Satisfies(x.Version))
                .MinBy(x => x.Version);

            if (resolved == null) continue;

            var isCompatible = await IsCompatibleAsync(resolved, umbraco, depth + 1);
            if (isCompatible != null) return isCompatible;
        }

        return null;
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

        var versions = await FetchPackageVersionsAsync(packageId);

        // Don't cache failures, so a transient NuGet outage doesn't stick for an hour
        if (versions != null)
        {
            _memoryCache.Set(cacheKey, versions, CacheDuration);
        }

        return versions ?? new List<PackageVersionInfo>();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Compatibility lookup is best effort; failures fall back to an unpinned package")]
    private async Task<List<PackageVersionInfo>?> FetchPackageVersionsAsync(string packageId)
    {
        try
        {
            var client = _clientFactory.CreateClient();
            var index = await GetJsonAsync<NuGetRegistrationIndex>(client, $"{RegistrationBaseUrl}{packageId.ToLowerInvariant()}/index.json");
            if (index == null) return null;

            var pages = await Task.WhenAll(index.Items.Select(async page =>
                page.Items ?? (await GetJsonAsync<NuGetRegistrationPage>(client, page.Id))?.Items ?? new List<NuGetRegistrationLeaf>()));

            return pages
                .SelectMany(x => x)
                .Select(x => PackageVersionInfo.FromCatalogEntry(x.CatalogEntry))
                .OfType<PackageVersionInfo>()
                .ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<T?> GetJsonAsync<T>(HttpClient client, string url)
    {
        using var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return default;

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
