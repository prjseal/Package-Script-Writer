using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PSW.Models;
using System.Diagnostics;
using static PSW.Models.PackageFeed;

namespace PSW.Controllers;
public class HomeController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly IPackageService _packageService;
    private readonly IQueryStringService _queryStringService;

    public HomeController(IMemoryCache memoryCache,
        IScriptGeneratorService scriptGeneratorService, IPackageService packageService,
        IQueryStringService queryStringService)
    {
        _memoryCache = memoryCache;
        _scriptGeneratorService = scriptGeneratorService;
        _packageService = packageService;
        _queryStringService = queryStringService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        PackagesViewModel packageOptions = _queryStringService.LoadModelFromQueryString(Request);

        var allPackages = new List<PagedPackagesPackage>();

        int cacheTime = 60;

        allPackages = _memoryCache.GetOrCreate(
            "allPackages",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetAllPackagesFromUmbraco();
            });

        var umbracoVersions = new List<string>();
        umbracoVersions = _memoryCache.GetOrCreate(
            "umbracoVersions",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetNugetPackageVersions("https://api.nuget.org/v3-flatcontainer/umbraco.templates/index.json");
            });

        PopulatePackageVersions(packageOptions, allPackages, cacheTime);

        packageOptions.AllPackages = allPackages;
        packageOptions.UmbracoVersions = umbracoVersions;

        var output = _scriptGeneratorService.GenerateScript(packageOptions);

        packageOptions.Output = output;

        return View(packageOptions);
    }

    private void PopulatePackageVersions(PackagesViewModel packageOptions, List<PagedPackagesPackage> allPackages, int cacheTime)
    {
        List<string> pickedPackageIds = new List<string>();
        List<PackageWithVersion> pickedPackages = new List<PackageWithVersion>();
        var packages = packageOptions.Packages.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (packages != null && packages.Any())
        {
            foreach (var package in packages)
            {
                var packageParts = package.Split('|');
                var pwv = new PackageWithVersion()
                {
                    PackageId = packageParts[0]
                };
                if (packageParts.Length > 1)
                {
                    pwv.PackageVersion = packageParts[1];
                }
                pickedPackages.Add(pwv);
            }
            pickedPackageIds = pickedPackages.Select(x => x.PackageId).ToList();

            foreach (var package in allPackages)
            {
                if (pickedPackageIds.Contains(package.NuGetPackageId))
                {
                    var packageWithVersion = pickedPackages.FirstOrDefault(x => x.PackageId == package.NuGetPackageId);
                    var thisPackageVersions = _memoryCache.GetOrCreate(
                        package.NuGetPackageId + "_Versions",
                        cacheEntry =>
                        {
                            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                            return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{package.NuGetPackageId.ToLower()}/index.json");
                        });

                    package.PackageVersions = thisPackageVersions;
                    package.SelectedVersion = packageWithVersion.PackageVersion;
                }
            }
        }
    }

    [HttpPost]
    public IActionResult Index(PackagesViewModel model)
    {
        var queryString = _queryStringService.GenerateQueryStringFromModel(model);
        return Redirect("/" + queryString);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
