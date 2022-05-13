using PSW.Extensions;
using PSW.Models;
using static PSW.Models.PackageFeed;

namespace PSW.Services
{
    public class QueryStringService : IQueryStringService
    {
        public PackagesViewModel LoadModelFromQueryString(HttpRequest request)
        {
            bool.TryParse(request.Query["IncludeStarterKit"], out var includeStarterKit);
            bool.TryParse(request.Query["InstallUmbracoTemplate"], out var installUmbracoTemplate);
            bool.TryParse(request.Query["CreateSolutionFile"], out var createSolutionFile);
            bool.TryParse(request.Query["UseUnattendedInstall"], out var useUnattendedInstall);

            if (request.Query.Count == 0)
            {
                includeStarterKit = true;
                installUmbracoTemplate = true;
                createSolutionFile = true;
                useUnattendedInstall = true;
            }

            var umbracoTemplateVersion = request.Query.GetStringValue("UmbracoTemplateVersion", "");
            var starterKitPackage = request.Query.GetStringValue("StarterKitPackage", "Umbraco.TheStarterKit");
            var projectName = request.Query.GetStringValue("ProjectName", createSolutionFile || installUmbracoTemplate ? "MyProject" : "");
            var solutionName = request.Query.GetStringValue("SolutionName", "MySolution");
            var databaseType = request.Query.GetStringValue("DatabaseType", "LocalDb");
            var userFriendlyName = request.Query.GetStringValue("UserFriendlyName", "Administrator");
            var userEmail = request.Query.GetStringValue("UserEmail", "admin@example.com");
            var userPassword = request.Query.GetStringValue("UserPassword", "1234567890");
            var packages = request.Query.GetStringValue("Packages", "");

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
                SolutionName = solutionName
            };
            return packageOptions;
        }

        public QueryString GenerateQueryStringFromModel(PackagesViewModel model)
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

            return queryString;
        }
    }
}
