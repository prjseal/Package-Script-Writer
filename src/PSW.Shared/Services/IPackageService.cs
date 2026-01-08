using PSW.Shared.Models;
using PSW.Shared.Models.NuGet;
using static PSW.Shared.Enums.GlobalEnums;

namespace PSW.Shared.Services;

public interface IPackageService
{
    Task<List<string>> GetNugetPackageVersionsAsync(string packageUrl);
    Task<List<string>> GetPackageVersionsAsync(string packageUrl);
    Task<List<PagedPackagesPackage>> GetAllPackagesFromUmbracoAsync();
    Task<List<PagedPackagesPackage>> GetFromUmbracoMarketplaceAsync(UmbracoMarketplaceQueryType umbracoMarketplaceQueryType);
    Task<List<PagedPackagesPackage>> GetAllTemplatesFromUmbracoAsync();
    Task<List<NuGetSearchResult>> SearchNuGetPackagesAsync(string searchTerm, int take = 20);
}