using Microsoft.Extensions.Caching.Memory;

using PSW.Shared.Configuration;
using PSW.Shared.Constants;

namespace PSW.Shared.Services;

public class UmbracoVersionService : IUmbracoVersionService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IPackageService _packageService;

    public UmbracoVersionService(IMemoryCache memoryCache, IPackageService packageService)
    {
        _memoryCache = memoryCache;
        _packageService = packageService;
    }

    public string? GetLatestLTSVersion(PSWConfig pswConfig)
    {
        var midnightTonight = DateTime.Now.AddDays(1).Date;

        if (pswConfig?.UmbracoVersions == null || !pswConfig.UmbracoVersions.Any()) return null;

        var latestLTSMajor = pswConfig.UmbracoVersions.LastOrDefault(
                x => x.ReleaseType == "LTS" && x.ReleaseDate < midnightTonight
                && x.SecurityPhase >= midnightTonight);

        var umbracoVersions = GetUmbracoVersionsFromCache(pswConfig);

        return umbracoVersions?.FirstOrDefault(x => x.StartsWith(latestLTSMajor.Version.ToString()) && !x.Contains('-'));
    }

    public List<string>? GetUmbracoVersionsFromCache(PSWConfig pswConfig)
    {
        var umbracoVersions = new List<string>();
        if (!string.IsNullOrWhiteSpace(GlobalConstants.TEMPLATE_NAME_UMBRACO))
        {
            umbracoVersions = _memoryCache.GetOrCreate(
                $"{GlobalConstants.TEMPLATE_NAME_UMBRACO}_Versions",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(pswConfig.CachingTimeInMins);
                    return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{GlobalConstants.TEMPLATE_NAME_UMBRACO.ToLower()}/index.json");
                });
        }
        return umbracoVersions;
    }
}