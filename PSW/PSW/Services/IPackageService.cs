using PSW.Models;
using PSW.Models.NuGet;
using static PSW.Models.PackageFeed;

namespace PSW.Services;

public interface IPackageService
{
    List<string> GetNugetPackageVersions(string packageUrl);
    public List<string> GetPackageVersions(string packageUrl);
    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco();
}