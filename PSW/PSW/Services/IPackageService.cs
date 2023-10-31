using PSW.Models;
using PSW.Models.NuGet;
using static PSW.Enums.GlobalEnums;

namespace PSW.Services;

public interface IPackageService
{
    List<string> GetNugetPackageVersions(string packageUrl);
    public List<string> GetPackageVersions(string packageUrl);
    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco();
    List<PagedPackagesPackage> GetFromUmbracoMarketplace(UmbracoMarketplaceQueryType umbracoMarketplaceQueryType);
    List<PagedPackagesPackage> GetAllTemplatesFromUmbraco();
}