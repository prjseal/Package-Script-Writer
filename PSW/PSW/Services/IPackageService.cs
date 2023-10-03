using PSW.Models;
using PSW.Models.NuGet;

namespace PSW.Services;

public interface IPackageService
{
    List<string> GetNugetPackageVersions(string packageUrl);
    public List<string> GetPackageVersions(string packageUrl);
    public List<PagedPackagesPackage> GetAllPackagesFromUmbraco();
}