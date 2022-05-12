using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PSW.Extensions;
using PSW.Models;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using static PSW.Models.PackageFeed;

namespace PSW.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public IActionResult Index()
        {
            bool.TryParse(Request.Query["IncludeStarterKit"], out var includeStarterKit);
            bool.TryParse(Request.Query["InstallUmbracoTemplate"], out var installUmbracoTemplate);
            bool.TryParse(Request.Query["CreateSolutionFile"], out var createSolutionFile);
            bool.TryParse(Request.Query["UseUnattendedInstall"], out var useUnattendedInstall);

            if (Request.Query.Count == 0)
            {
                includeStarterKit = true;
                installUmbracoTemplate = true;
                createSolutionFile = true;
                useUnattendedInstall = true;
            }

            var umbracoTemplateVersion = GetStringFromQueryString("UmbracoTemplateVersion", "");
            var starterKitPackage = GetStringFromQueryString("StarterKitPackage", "Umbraco.TheStarterKit");
            var projectName = GetStringFromQueryString("ProjectName", createSolutionFile || installUmbracoTemplate ? "MyProject" : "");
            var solutionName = GetStringFromQueryString("SolutionName", "MySolution");
            var databaseType = GetStringFromQueryString("DatabaseType", "LocalDb");
            var userFriendlyName = GetStringFromQueryString("UserFriendlyName", "Administrator");
            var userEmail = GetStringFromQueryString("UserEmail", "admin@example.com");
            var userPassword = GetStringFromQueryString("UserPassword", "1234567890");
            var packages = GetStringFromQueryString("Packages", "");

            var allPackages = new List<PagedPackagesPackage>();

            allPackages = _memoryCache.GetOrCreate(
                "allPackages",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                    return GetAllPackagesFromUmbraco();
                });

            var umbracoVersions = new List<string>();
            umbracoVersions = _memoryCache.GetOrCreate(
                "umbracoVersions",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                    return GetPackageVersions("https://www.nuget.org/packages/Umbraco.Templates");
                });

            var packageOptions = new PackagesViewModel()
            {
                Packages = packages,
                InstallUmbracoTemplate = installUmbracoTemplate,
                UmbracoTemplateVersion = umbracoTemplateVersion,
                IncludeStarterKit = includeStarterKit,
                StarterKitPackage = starterKitPackage,
                UseUnattendedInstall = useUnattendedInstall,
                DatabaseType = databaseType,
                UserFriendlyName = userFriendlyName,
                UserPassword = userPassword,
                UserEmail = userEmail,
                ProjectName = projectName,
                CreateSolutionFile = createSolutionFile,
                SolutionName = solutionName,
                AllPackages = allPackages,
                UmbracoVersions = umbracoVersions
            };

            var output = GeneratePackageScript(packageOptions);

            packageOptions.Output = output;

            return View(packageOptions);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PackagesViewModel model)
        {
            var queryString = new QueryString();

            queryString = queryString.Add("InstallUmbracoTemplate", model.InstallUmbracoTemplate.ToString());
            queryString = queryString.AddValueIfNotEmpty("UmbracoTemplateVersion", model.UmbracoTemplateVersion);
            queryString = queryString.Add("IncludeStarterKit", model.IncludeStarterKit.ToString());
            queryString = queryString.AddValueIfNotEmpty("StarterKitPackage", model.StarterKitPackage);
            queryString = queryString.Add("UseUnattendedInstall", model.UseUnattendedInstall.ToString());
            queryString = queryString.AddValueIfNotEmpty("DatabaseType", model.DatabaseType);
            queryString = queryString.AddValueIfNotEmpty("UserEmail", model.UserEmail);
            queryString = queryString.AddValueIfNotEmpty("UserFriendlyName", model.UserFriendlyName);
            queryString = queryString.AddValueIfNotEmpty("UserPassword", model.UserPassword);
            queryString = queryString.AddValueIfNotEmpty("ProjectName", model.ProjectName);
            queryString = queryString.Add("CreateSolutionFile", model.CreateSolutionFile.ToString());
            queryString = queryString.AddValueIfNotEmpty("SolutionName", model.SolutionName);
            queryString = queryString.AddValueIfNotEmpty("Packages", model.Packages);

            return Redirect("/" + queryString);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string GetStringFromQueryString(string keyName, string fallbackValue)
        {
            var returnValue = fallbackValue;

            var rawValue = Request.Query[keyName];
            if (!string.IsNullOrWhiteSpace(rawValue))
            {
                returnValue = rawValue;
            }

            return returnValue;
        }

        private List<PagedPackagesPackage> GetAllPackagesFromUmbraco()
        {

            int pageIndex = 0;
            var pageSize = 1;
            var carryOn = true;
            List<PagedPackagesPackage> allPackages = new List<PagedPackagesPackage>();

            while (carryOn)
            {
                var url = $"https://our.umbraco.com/webapi/packages/v1?pageIndex={pageIndex}&pageSize={pageSize}&category=&query=&order=Latest&version=9.5.0";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = "application/xml";

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PagedPackages));
                    var baseStream = streamReader.BaseStream;
                    if (baseStream == null)
                    {
                        carryOn = false;
                        break;
                    }
                    try
                    {
                        var packageFeed = (PagedPackages)serializer.Deserialize(baseStream);
                        if (packageFeed?.Packages != null)
                        {
                            allPackages.AddRange(packageFeed.Packages);
                            carryOn = true;
                        }
                        else
                        {
                            carryOn = false;
                        }
                    }
                    catch
                    {
                        carryOn = false;
                        break;
                    }
                }
                pageIndex++;
            }
            return allPackages;
        }

        private List<string> GetPackageVersions(string packageUrl)
        {
            List<string> allVersions = new List<string>();

            var url = $"{packageUrl}/atom.xml";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Accept = "application/xml";

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NugetPackageVersionFeed.feed));
                var baseStream = streamReader.BaseStream;
                if (baseStream == null) return allVersions;

                var packageFeed = (NugetPackageVersionFeed.feed)serializer.Deserialize(baseStream);
                if (packageFeed != null)
                {
                    foreach (var entry in packageFeed.entryField)
                    {
                        var parts = entry.id.Split('/');
                        var partCount = parts.Length;
                        var versionNumber = parts[partCount - 1];
                        allVersions.Add(versionNumber);
                    }
                }
            }
            return allVersions;
        }

        private string GeneratePackageScript(PackagesViewModel model)
        {
            StringBuilder sb = new StringBuilder();

            var renderPackageName = !string.IsNullOrWhiteSpace(model.ProjectName);

            if (model.InstallUmbracoTemplate)
            {
                sb.AppendLine("# Ensure we have the latest Umbraco templates");
                if (!string.IsNullOrWhiteSpace(model.UmbracoTemplateVersion))
                {
                    sb.AppendLine($"dotnet new -i Umbraco.Templates::{model.UmbracoTemplateVersion}");
                }
                else
                {
                    sb.AppendLine("dotnet new -i Umbraco.Templates");
                }
                sb.AppendLine();

                if (model.CreateSolutionFile)
                {
                    sb.AppendLine("# Create solution/project");
                    if (!string.IsNullOrWhiteSpace(model.SolutionName))
                    {
                        sb.AppendLine($"dotnet new sln --name \"{model.SolutionName}\"");
                    }
                }
                //Data Source=|DataDirectory|/PaulsShinySQLIteDatabase.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True
                //                sb.AppendLine($"dotnet new umbraco -n {model.ProjectName} --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\" --connection-string \"Data Source = (localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Umbraco.mdf;Integrated Security=True\"");

                if (model.UseUnattendedInstall)
                {
                    var connectionString = "";
                    switch (model.DatabaseType)
                    {
                        case "LocalDb":
                            connectionString = "\"Data Source = (localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\Umbraco.mdf;Integrated Security=True\"";
                            break;
                        case "SQLCE":
                            connectionString = "\"Data Source=|DataDirectory|\\Umbraco.sdf;Flush Interval=1\" -ce";
                            break;
                        case "SQLite":
                            connectionString = "\"Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True\"";
                            break;
                        default:
                            break;
                    }

                    sb.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\" --friendly-name \"{model.UserFriendlyName}\" --email \"{model.UserEmail}\" --password \"{model.UserPassword}\" --connection-string {connectionString}");

                    if (model.DatabaseType == "SQLite")
                    {
                        sb.AppendLine("$env:Umbraco__CMS__Global__InstallMissingDatabase=\"true\"");
                        sb.AppendLine("$env:ConnectionStrings__umbracoDbDSN_ProviderName=\"Microsoft.Data.SQLite\"");
                    }
                }
                else
                {
                    sb.AppendLine($"dotnet new umbraco -n \"{model.ProjectName}\"");
                }

                if (model.CreateSolutionFile && !string.IsNullOrWhiteSpace(model.SolutionName))
                {
                    sb.AppendLine($"dotnet sln add \"{model.ProjectName}\"");
                }
                sb.AppendLine();
            }

            if (model.IncludeStarterKit)
            {
                sb.AppendLine("#Add starter kit");
                if(renderPackageName)
                {
                    sb.AppendLine($"dotnet add \"{model.ProjectName}\" package {model.StarterKitPackage}");
                }
                else
                {
                    sb.AppendLine($"dotnet add package {model.StarterKitPackage}");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(model.Packages))
            {
                var packages = model.Packages.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

                if (packages != null && packages.Length > 0)
                {
                    sb.AppendLine("#Add Packages");

                    foreach (var package in packages)
                    {
                        if(renderPackageName)
                        {
                            sb.AppendLine($"dotnet add \"{model.ProjectName}\" package {package}");
                        }
                        else
                        {
                            sb.AppendLine($"dotnet add package {package}");
                        }
                    }
                }
                sb.AppendLine();
            }

            if(renderPackageName)
            {
                sb.AppendLine($"dotnet run --project \"{model.ProjectName}\"");
            }
            else
            {
                sb.AppendLine($"dotnet run");
            }
            
            sb.AppendLine("#Running");
            return sb.ToString();
        }
    }
}
