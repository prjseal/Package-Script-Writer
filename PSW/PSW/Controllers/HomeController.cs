using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

using PSW.Models;

using static PSW.Models.PackageFeed;

using PagedPackagesPackage = PSW.Models.PagedPackagesPackage;

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

        packageOptions.HasQueryString = HttpContext.Request.Query.Count > 0;

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
        if (!string.IsNullOrWhiteSpace(packageOptions.TemplateName))
        {
            umbracoVersions = _memoryCache.GetOrCreate(
                $"{packageOptions.TemplateName}_Versions",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                    return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{packageOptions.TemplateName.ToLower()}/index.json");
                });
        }

        PopulatePackageVersions(packageOptions, allPackages, cacheTime);

        var umbracoTemplates = new List<SelectListItem>()
        {
            new SelectListItem("(none)", ""),
            new SelectListItem(GlobalConstants.TEMPLATE_NAME_UMBRACO, GlobalConstants.TEMPLATE_NAME_UMBRACO, packageOptions.TemplateName?.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) == true),
        };

        umbracoTemplates.AddRange(_memoryCache.GetOrCreate(
            "allTemplates",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetAllTemplatesFromUmbraco().Select(x => new SelectListItem(x.NuGetPackageId, x.NuGetPackageId, packageOptions.TemplateName?.Equals(x.NuGetPackageId) == true));
            }));

        packageOptions.AllPackages = allPackages;
        packageOptions.UmbracoVersions = umbracoVersions;
        packageOptions.TemplateNames = umbracoTemplates;

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