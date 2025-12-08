using PSW.Shared.Models;
using static PSW.Shared.Enums.GlobalEnums;

namespace PSW.Shared.Services;

public interface IPackageService
{
    List<string> GetNugetPackageVersions(string packageUrl);
    public List<string> GetPackageVersions(string packageUrl);
    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco();
    List<PagedPackagesPackage> GetFromUmbracoMarketplace(UmbracoMarketplaceQueryType umbracoMarketplaceQueryType);
    List<PagedPackagesPackage> GetAllTemplatesFromUmbraco();
}