using static PSW.Models.PackageFeed;

namespace PSW.Services;

public interface IPackageService
{
    public List<string> GetPackageVersions(string packageUrl);
    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco();
}