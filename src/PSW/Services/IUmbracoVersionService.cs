using PSW.Configuration;

namespace PSW.Services;

public interface IUmbracoVersionService
{
    string? GetLatestLTSVersion(PSWConfig pswConfig);
    List<string>? GetUmbracoVersionsFromCache(PSWConfig pswConfig);
}