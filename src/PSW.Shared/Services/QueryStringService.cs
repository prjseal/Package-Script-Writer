using Microsoft.AspNetCore.Http;
using PSW.Shared.Constants;
using PSW.Shared.Models;
using PSW.Shared.Extensions;

namespace PSW.Shared.Services;

public class QueryStringService : IQueryStringService
{
    public PackagesViewModel LoadModelFromQueryString(HttpRequest request)
    {
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.IncludeStarterKit)], out var includeStarterKit);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.IncludeDockerfile)], out var includeDockerfile);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.IncludeDockerCompose)], out var includeDockerCompose);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.EnableContentDeliveryApi)], out var enableContentDeliveryApi);
        _ = bool.TryParse(request.Query["InstallUmbracoTemplate"], out var installUmbracoTemplate); // Fallback from older property
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.CreateSolutionFile)], out var createSolutionFile);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.UseUnattendedInstall)], out var useUnattendedInstall);
        _ = bool.TryParse(request.Query[nameof(PackagesViewModel.OnelinerOutput)], out var onelinerOutput);

        if (request.Query.Count == 0)
        {
            includeStarterKit = true;
            includeDockerfile = false;
            includeDockerCompose = false;
            enableContentDeliveryApi = false;
            createSolutionFile = true;
            useUnattendedInstall = true;
            onelinerOutput = false;
        }

        var templateName = installUmbracoTemplate ? GlobalConstants.TEMPLATE_NAME_UMBRACO : request.Query.GetStringValue("TemplateName", DefaultValues.TemplateName);
        var umbracoTemplateVersion = request.Query.GetStringValue("UmbracoTemplateVersion", ""); // Fallback from older property
        var templateVersion = string.IsNullOrEmpty(umbracoTemplateVersion) ? request.Query.GetStringValue(nameof(PackagesViewModel.TemplateVersion), "") : "";
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
            TemplateName = templateName ?? string.Empty,
            TemplateVersion = templateVersion,
            IncludeStarterKit = includeStarterKit,
            IncludeDockerfile = includeDockerfile,
            IncludeDockerCompose = includeDockerCompose,
            EnableContentDeliveryApi = enableContentDeliveryApi,
            StarterKitPackage = starterKitPackage,
            UseUnattendedInstall = useUnattendedInstall,
            DatabaseType = databaseType,
            ConnectionString = connectionString,
            UserFriendlyName = userFriendlyName,
            UserPassword = userPassword,
            UserEmail = userEmail,
            ProjectName = projectName,
            CreateSolutionFile = createSolutionFile,
            SolutionName = solutionName,
            OnelinerOutput = onelinerOutput
        };
        return packageOptions;
    }

    public QueryString GenerateQueryStringFromModel(PackagesViewModel model)
    {
        var queryString = new QueryString();

        queryString = queryString.Add(nameof(model.TemplateName), model.TemplateName ?? "");
        queryString = queryString.AddValueIfNotEmpty(nameof(model.TemplateVersion), model.TemplateVersion ?? "");
        queryString = queryString.Add(nameof(model.IncludeStarterKit), model.IncludeStarterKit.ToString());
        queryString = queryString.Add(nameof(model.IncludeDockerfile), model.IncludeDockerfile.ToString());
        queryString = queryString.Add(nameof(model.IncludeDockerCompose), model.IncludeDockerCompose.ToString());
        queryString = queryString.Add(nameof(model.EnableContentDeliveryApi), model.EnableContentDeliveryApi.ToString());
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
        queryString = queryString.Add(nameof(model.OnelinerOutput), model.OnelinerOutput.ToString());

        return queryString;
    }
}