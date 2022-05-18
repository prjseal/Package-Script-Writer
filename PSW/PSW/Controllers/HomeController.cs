using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PSW.Models;
using System.Diagnostics;
using static PSW.Models.PackageFeed;

namespace PSW.Controllers;
public class HomeController : Controller
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<HomeController> _logger;
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly IPackageService _packageService;
    private readonly IQueryStringService _queryStringService;

    public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache,
        IScriptGeneratorService scriptGeneratorService, IPackageService packageService,
        IQueryStringService queryStringService)
    {
        _logger = logger;
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
                return _packageService.GetPackageVersions("https://www.nuget.org/packages/Umbraco.Templates");
            });

        packageOptions.AllPackages = allPackages;
        packageOptions.UmbracoVersions = umbracoVersions;

        var output = _scriptGeneratorService.GeneratePackageScript(packageOptions);

        packageOptions.Output = output;

        return View(packageOptions);
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
