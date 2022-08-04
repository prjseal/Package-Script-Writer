using PSW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace PSW.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptGeneratorApiController : ControllerBase
{
    private readonly IScriptGeneratorService _scriptGeneratorService;
    private readonly IMemoryCache _memoryCache;
    private readonly IPackageService _packageService;

    public ScriptGeneratorApiController(IScriptGeneratorService scriptGeneratorService,
        IMemoryCache memoryCache, IPackageService packageService)
    {
        _scriptGeneratorService = scriptGeneratorService;
        _memoryCache = memoryCache;
        _packageService = packageService;
    }

    [Route("generatescript")]
    [HttpPost]
    public ActionResult GenerateScript([FromBody] GeneratorApiRequest apiRequest)
    {
        if (apiRequest.IsEmpty)
        {
            apiRequest.IncludeStarterKit = true;
            apiRequest.InstallUmbracoTemplate = true;
            apiRequest.CreateSolutionFile = true;
            apiRequest.UseUnattendedInstall = true;
        }

        apiRequest.StarterKitPackage = !string.IsNullOrWhiteSpace(apiRequest.StarterKitPackage) ? apiRequest.StarterKitPackage : DefaultValues.StarterKitPackage;
        apiRequest.ProjectName = !string.IsNullOrWhiteSpace(apiRequest.ProjectName) ? apiRequest.ProjectName : (apiRequest.CreateSolutionFile || apiRequest.InstallUmbracoTemplate ? PSW.Constants.DefaultValues.ProjectName : "");
        apiRequest.SolutionName = !string.IsNullOrWhiteSpace(apiRequest.SolutionName) ? apiRequest.SolutionName : DefaultValues.SolutionName;
        apiRequest.DatabaseType = !string.IsNullOrWhiteSpace(apiRequest.DatabaseType) ? apiRequest.DatabaseType : DefaultValues.DatabaseType;
        apiRequest.ConnectionString = !string.IsNullOrWhiteSpace(apiRequest.ConnectionString) ? apiRequest.ConnectionString : DefaultValues.ConnectionString;
        apiRequest.UserFriendlyName = !string.IsNullOrWhiteSpace(apiRequest.UserFriendlyName) ? apiRequest.UserFriendlyName : DefaultValues.UserFriendlyName;
        apiRequest.UserEmail = !string.IsNullOrWhiteSpace(apiRequest.UserEmail) ? apiRequest.UserEmail : DefaultValues.UserEmail;
        apiRequest.UserPassword = !string.IsNullOrWhiteSpace(apiRequest.UserPassword) ? apiRequest.UserPassword : DefaultValues.UserPassword;

        var model = new PackagesViewModel(apiRequest);
        return Ok(_scriptGeneratorService.GenerateScript(model));
    }

    [Route("getpackageversions")]
    [HttpPost]
    public ActionResult GetPackageVersions([FromBody] PackageVersionsApiRequest apiRequest)
    {
        int cacheTime = 60;
        var packageUniqueId = apiRequest.PackageId.ToLower();
        var packageVersions = new List<string>();
        packageVersions = _memoryCache.GetOrCreate(
            apiRequest.PackageId + "_Versions",
            cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTime);
                return _packageService.GetNugetPackageVersions($"https://api.nuget.org/v3-flatcontainer/{packageUniqueId}/index.json");
            });
        return Ok(packageVersions);
    }

    [Route("test")]
    [HttpGet]
    public ActionResult test()
    {
        var model = "Hello, world!. The time is " + DateTime.Now.ToString();
        return Ok(model);
    }

    [Route("clearcache")]
    [HttpGet]
    public ActionResult ClearCache()
    {
        _memoryCache.Remove("allPackages");
        _memoryCache.Remove("umbracoVersions");
        var message = "Cache Cleared at " + DateTime.Now.ToString();
        return Ok(message);
    }
}