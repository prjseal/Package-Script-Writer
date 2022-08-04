using PSW.Extensions;
using PSW.Models;

namespace PSW.Services;
public class QueryStringService : IQueryStringService
{
    public PackagesViewModel LoadModelFromQueryString(HttpRequest request)
    {
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.IncludeStarterKit)], out var includeStarterKit);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.InstallUmbracoTemplate)], out var installUmbracoTemplate);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.CreateSolutionFile)], out var createSolutionFile);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.UseUnattendedInstall)], out var useUnattendedInstall);

        if (request.Query.Count == 0)
        {
            includeStarterKit = true;
            installUmbracoTemplate = true;
            createSolutionFile = true;
            useUnattendedInstall = true;
        }

        var umbracoTemplateVersion = request.Query.GetStringValue(nameof(PackagesViewModel.UmbracoTemplateVersion), "");
        var starterKitPackage = request.Query.GetStringValue(nameof(PackagesViewModel.StarterKitPackage), DefaultValues.StarterKitPackage);
        var projectName = request.Query.GetStringValue(nameof(PackagesViewModel.ProjectName), createSolutionFile || installUmbracoTemplate ? DefaultValues.ProjectName : "");
        var solutionName = request.Query.GetStringValue(nameof(PackagesViewModel.SolutionName), DefaultValues.SolutionName);
        var databaseType = request.Query.GetStringValue(nameof(PackagesViewModel.DatabaseType), DefaultValues.DatabaseType);
        var connectionString = request.Query.GetStringValue(nameof(PackagesViewModel.ConnectionString), DefaultValues.ConnectionString);
        var userFriendlyName = request.Query.GetStringValue(nameof(PackagesViewModel.UserFriendlyName), DefaultValues.UserFriendlyName);
        var userEmail = request.Query.GetStringValue(nameof(PackagesViewModel.UserEmail), DefaultValues.UserEmail);
        var userPassword = request.Query.GetStringValue(nameof(PackagesViewModel.UserPassword), DefaultValues.UserPassword);
        var packages = request.Query.GetStringValue(nameof(PackagesViewModel.Packages), "");

        var packageOptions = new PackagesViewModel()
        {
            Packages = packages,
            InstallUmbracoTemplate = installUmbracoTemplate,
            UmbracoTemplateVersion = umbracoTemplateVersion,
            IncludeStarterKit = includeStarterKit,
            StarterKitPackage = starterKitPackage,
            UseUnattendedInstall = useUnattendedInstall,
            DatabaseType = databaseType,
            ConnectionString = connectionString,
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

        queryString = queryString.Add(nameof(model.InstallUmbracoTemplate), model.InstallUmbracoTemplate.ToString());
        queryString = queryString.AddValueIfNotEmpty(nameof(model.UmbracoTemplateVersion), model.UmbracoTemplateVersion ?? "");
        queryString = queryString.Add(nameof(model.IncludeStarterKit), model.IncludeStarterKit.ToString());
        queryString = queryString.AddValueIfNotEmpty(nameof(model.StarterKitPackage), model.StarterKitPackage ?? "");
        queryString = queryString.Add(nameof(model.UseUnattendedInstall), model.UseUnattendedInstall.ToString());
        queryString = queryString.AddValueIfNotEmpty(nameof(model.DatabaseType), model.DatabaseType ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.UserEmail), model.UserEmail ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.UserFriendlyName), model.UserFriendlyName ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.UserPassword), model.UserPassword ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.ProjectName), model.ProjectName ?? "");
        queryString = queryString.Add(nameof(model.CreateSolutionFile), model.CreateSolutionFile.ToString());
        queryString = queryString.AddValueIfNotEmpty(nameof(model.SolutionName), model.SolutionName ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.Packages), model.Packages ?? "");

        return queryString;
    }
}