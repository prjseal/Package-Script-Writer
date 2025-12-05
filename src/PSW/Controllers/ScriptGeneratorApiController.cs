using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

using PSW.Models;

namespace PSW.Controllers;

/// <summary>
/// API controller for generating package installation scripts and managing package versions
/// </summary>
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

    /// <summary>
    /// Generates a script for installing packages based on the provided configuration
    /// </summary>
    /// <param name="apiRequest">The request containing package and project configuration details</param>
    /// <returns>A generated script as plain text</returns>
    /// <response code="200">Returns the generated script</response>
    /// <response code="400">If the request is invalid</response>
    [Route("generatescript")]
    [HttpPost]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult GenerateScript([FromBody] GeneratorApiRequest apiRequest)
    {
        if (apiRequest.IsEmpty)
        {
            apiRequest.IncludeStarterKit = true;
            apiRequest.IncludeDockerfile = false;
            apiRequest.IncludeDockerCompose = false;
            apiRequest.CreateSolutionFile = true;
            apiRequest.UseUnattendedInstall = true;
            apiRequest.OnelinerOutput = false;
        }

        apiRequest.StarterKitPackage = !string.IsNullOrWhiteSpace(apiRequest.StarterKitPackage) ? apiRequest.StarterKitPackage : DefaultValues.StarterKitPackage;
        apiRequest.ProjectName = !string.IsNullOrWhiteSpace(apiRequest.ProjectName) ? apiRequest.ProjectName : (apiRequest.CreateSolutionFile || apiRequest.TemplateName?.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) == true ? PSW.Constants.DefaultValues.ProjectName : "");
        apiRequest.SolutionName = !string.IsNullOrWhiteSpace(apiRequest.SolutionName) ? apiRequest.SolutionName : DefaultValues.SolutionName;
        apiRequest.DatabaseType = !string.IsNullOrWhiteSpace(apiRequest.DatabaseType) ? apiRequest.DatabaseType : DefaultValues.DatabaseType;
        apiRequest.ConnectionString = !string.IsNullOrWhiteSpace(apiRequest.ConnectionString) ? apiRequest.ConnectionString : DefaultValues.ConnectionString;
        apiRequest.UserFriendlyName = !string.IsNullOrWhiteSpace(apiRequest.UserFriendlyName) ? apiRequest.UserFriendlyName : DefaultValues.UserFriendlyName;
        apiRequest.UserEmail = !string.IsNullOrWhiteSpace(apiRequest.UserEmail) ? apiRequest.UserEmail : DefaultValues.UserEmail;
        apiRequest.UserPassword = !string.IsNullOrWhiteSpace(apiRequest.UserPassword) ? apiRequest.UserPassword : DefaultValues.UserPassword;

        var model = new PackagesViewModel(apiRequest);

        return Ok(_scriptGeneratorService.GenerateScript(model));
    }

    /// <summary>
    /// Retrieves available versions for a specific NuGet package
    /// </summary>
    /// <param name="apiRequest">The request containing the package ID</param>
    /// <returns>A list of available package versions</returns>
    /// <response code="200">Returns the list of package versions</response>
    /// <response code="400">If the package ID is invalid</response>
    [Route("getpackageversions")]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Test endpoint to verify API connectivity
    /// </summary>
    /// <returns>A greeting message with current timestamp</returns>
    /// <response code="200">Returns a test message with timestamp</response>
    [Route("test")]
    [HttpGet]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public ActionResult Test()
    {
        var model = "Hello, world!. The time is " + DateTime.Now.ToString();
        return Ok(model);
    }

    /// <summary>
    /// Clears the application cache for packages and templates
    /// </summary>
    /// <returns>A confirmation message with timestamp</returns>
    /// <response code="200">Returns a confirmation message</response>
    [Route("clearcache")]
    [HttpGet]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public ActionResult ClearCache()
    {
        _memoryCache.Remove("allPackages");
        _memoryCache.Remove($"{GlobalConstants.TEMPLATE_NAME_UMBRACO}_Versions");
        var message = "Cache Cleared at " + DateTime.Now.ToString();
        return Ok(message);
    }
}
