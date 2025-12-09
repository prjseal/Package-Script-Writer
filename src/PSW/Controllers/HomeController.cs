using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using PSW.Shared.Configuration;
using PSW.Shared.Constants;
using PSW.Shared.Models;
using PSW.Shared.Services;

using static PSW.Shared.Models.PackageFeed;

using PagedPackagesPackage = PSW.Shared.Models.PagedPackagesPackage;

namespace PSW.Controllers;

public class HomeController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly IPackageService _packageService;
    private readonly IQueryStringService _queryStringService;
    private readonly PSWConfig _pswConfig;
    private readonly IUmbracoVersionService _umbracoVersionService;

    public HomeController(IMemoryCache memoryCache,
        IScriptGeneratorService scriptGeneratorService, IPackageService packageService,
        IQueryStringService queryStringService, IOptions<PSWConfig> pswConfig, IUmbracoVersionService umbracoVersionService)
    {
        _memoryCache = memoryCache;
        _scriptGeneratorService = scriptGeneratorService;
        _packageService = packageService;
        _queryStringService = queryStringService;
        _pswConfig = pswConfig.Value;
        _umbracoVersionService = umbracoVersionService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        PackagesViewModel packageOptions = _queryStringService.LoadModelFromQueryString(Request);

        packageOptions.HasQueryString = HttpContext.Request.Query.Count > 0;

        var allPackages = new List<PagedPackagesPackage>();

        int cacheTime = _pswConfig.CachingTimeInMins;

        allPackages = _memoryCache.GetOrCreate(
            "allPackages",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetAllPackagesFromUmbraco();
            });

        var umbracoVersions = _umbracoVersionService.GetUmbracoVersionsFromCache(_pswConfig);

        PopulatePackageVersions(packageOptions, allPackages, cacheTime);

        var umbracoTemplates = new List<SelectListItem>()
        {
            new("(none)", ""),
            new(GlobalConstants.TEMPLATE_NAME_UMBRACO, GlobalConstants.TEMPLATE_NAME_UMBRACO, packageOptions.TemplateName?.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) == true),
        };

        umbracoTemplates.AddRange(_memoryCache.GetOrCreate(
            "allTemplates",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetAllTemplatesFromUmbraco().Select(x => new SelectListItem(x.PackageId, x.PackageId, packageOptions.TemplateName?.Equals(x.PackageId) == true));
            }));

        var latestLTSVersion = _umbracoVersionService.GetLatestLTSVersion(_pswConfig);

        packageOptions.LatestLTSUmbracoVersion = latestLTSVersion;
        packageOptions.AllPackages = allPackages;
        packageOptions.UmbracoVersions = umbracoVersions;
        packageOptions.TemplateNames = umbracoTemplates;

        var output = _scriptGeneratorService.GenerateScript(packageOptions);

        packageOptions.Output = output;

        return View(packageOptions);
    }

    private void PopulatePackageVersions(PackagesViewModel packageOptions, List<PagedPackagesPackage> allPackages, int cacheTime)
    {
        List<string> pickedPackageIds = new();
        List<PackageWithVersion> pickedPackages = new();
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
                if (pickedPackageIds.Contains(package.PackageId))
                {
                    var packageWithVersion = pickedPackages.FirstOrDefault(x => x.PackageId == package.PackageId);
                    var thisPackageVersions = _memoryCache.GetOrCreate(
                        package.PackageId + "_Versions",
                        cacheEntry =>
                        {
                            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                            return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{package.PackageId.ToLower()}/index.json");
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