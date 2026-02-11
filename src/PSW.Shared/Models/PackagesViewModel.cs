using Microsoft.AspNetCore.Mvc.Rendering;
using PSW.Shared.Constants;
using System.ComponentModel.DataAnnotations;
using static PSW.Shared.Models.PackageFeed;

namespace PSW.Shared.Models;

public class PackagesViewModel
{
    public PackagesViewModel() { }

    public PackagesViewModel(GeneratorApiRequest apiRequest)
    {
        TemplateName = apiRequest.TemplateName ?? string.Empty;
        TemplateVersion = apiRequest.TemplateVersion;
        CreateSolutionFile = apiRequest.CreateSolutionFile;
        SolutionName = apiRequest.SolutionName;
        ProjectName = apiRequest.ProjectName;
        ConnectionString = apiRequest.ConnectionString;
        UserFriendlyName = apiRequest.UserFriendlyName;
        UserEmail = apiRequest.UserEmail;
        UserPassword = apiRequest.UserPassword;
        IncludeStarterKit = apiRequest.IncludeStarterKit;
        IncludeDockerfile = apiRequest.IncludeDockerfile;
        IncludeDockerCompose = apiRequest.IncludeDockerCompose;
        EnableContentDeliveryApi = apiRequest.EnableContentDeliveryApi;
        StarterKitPackage = apiRequest.StarterKitPackage;
        UseUnattendedInstall = apiRequest.UseUnattendedInstall;
        DatabaseType = apiRequest.DatabaseType;
        Packages = apiRequest.Packages;
        OnelinerOutput = apiRequest.OnelinerOutput;
        RemoveComments = apiRequest.RemoveComments;
    }

    public List<SelectListItem> TemplateNames { get; set; } = new();

    [Display(Name = "Template Name:")]
    public string TemplateName { get; set; } = string.Empty;

    [Display(Name = "Template Version:")]
    public string? TemplateVersion { get; set; }

    public List<PagedPackagesPackage>? AllPackages { get; set; }
    public string? Packages { get; set; }

    [Display(Name = "Create a Solution File:")]
    public bool CreateSolutionFile { get; set; }

    [Display(Name = "Solution Name:")]
    public string? SolutionName { get; set; }

    [Display(Name = "Project Name:")]
    public string? ProjectName { get; set; }

    [Display(Name = "Connection String:")]
    public string? ConnectionString { get; set; }

    [Display(Name = "Umbraco User Friendly Name:")]
    public string? UserFriendlyName { get; set; }

    [Display(Name = "Umbraco User Email:")]
    public string? UserEmail { get; set; }

    [Display(Name = "Umbraco User Password (min 10 characters):")]
    public string? UserPassword { get; set; }

    [Display(Name = "Include a Starter Kit:")]
    public bool IncludeStarterKit { get; set; }

    [Display(Name = "Include Dockerfile")]
    public bool IncludeDockerfile { get; set; }

    [Display(Name = "Include Docker Compose")]
    public bool IncludeDockerCompose { get; set; }

    [Display(Name = "Enable Content Delivery API")]
    public bool EnableContentDeliveryApi { get; set; }

    // Computed property for enabling/disabling Docker support checkbox on the view. 
    // The bulk of the logic is done here to keep the view clean, and is communicatd to the 
    // view with a hidden control : CanIncludeDockerfile
    public bool CanIncludeDocker
    {
        get
        {
            if (!TemplateName.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO))
            {
                return false;
            }

            // No template specified => Latest version, so can support docker
            if (string.IsNullOrWhiteSpace(TemplateVersion))
            {
                return true;
            }

            // Parse major version number : Check that version >= 15 which supports Docker
            // Technically 14.3 or above supports docker, but 14
            // is an STS release, and most people would be on 15 anyway, for simplicity keeping
            // it on 15 as a minimum
            if (int.TryParse(TemplateVersion.Split('.')[0], out var majorVersion))
            {
                return majorVersion >= 15;
            }

            return false;
        }
    }

    [Display(Name = "Starter Kit Package Id:")]
    public string? StarterKitPackage { get; set; }

    public string? Output { get; set; }

    [Display(Name = "Use Unattended Install:")]
    public bool UseUnattendedInstall { get; set; }

    [Display(Name = "Database Type")]
    public string? DatabaseType { get; set; }
    public List<string>? UmbracoVersions { get; set; }
    public string? LatestLTSUmbracoVersion { get; set; }

    [Display(Name = "Output to single line command")]
    public bool OnelinerOutput { get; set; }
    [Display(Name = "Remove comments")]
    public bool RemoveComments { get; set; }
    public bool HasQueryString { get; set; }

    /// <summary>
    /// When true, the generated script will not include the 'dotnet run' command.
    /// Used by the CLI --no-run flag for automation scenarios where the server should not be started.
    /// </summary>
    public bool SkipDotnetRun { get; set; }
}