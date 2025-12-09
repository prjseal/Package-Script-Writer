using PSW.Shared.Configuration;

namespace PSW.Shared.Services;

public interface IUmbracoVersionService
{
    string? GetLatestLTSVersion(PSWConfig pswConfig);
    Task<List<string>?> GetUmbracoVersionsFromCache(PSWConfig pswConfig);
}