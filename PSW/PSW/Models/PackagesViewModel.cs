using System.ComponentModel.DataAnnotations;
using static PSW.Models.PackageFeed;

namespace PSW.Models;
public class PackagesViewModel
{
    public PackagesViewModel() { }

    public PackagesViewModel(GeneratorApiRequest apiRequest)
    {
        InstallUmbracoTemplate = apiRequest.InstallUmbracoTemplate;
        UmbracoTemplateVersion = apiRequest.UmbracoTemplateVersion;
        CreateSolutionFile = apiRequest.CreateSolutionFile;
        SolutionName = apiRequest.SolutionName;
        ProjectName = apiRequest.ProjectName;
        ConnectionString = apiRequest.ConnectionString;
        UserFriendlyName = apiRequest.UserFriendlyName;
        UserEmail = apiRequest.UserEmail;
        UserPassword = apiRequest.UserPassword;
        IncludeStarterKit = apiRequest.IncludeStarterKit;
        StarterKitPackage = apiRequest.StarterKitPackage;
        UseUnattendedInstall = apiRequest.UseUnattendedInstall;
        DatabaseType = apiRequest.DatabaseType;
        Packages = apiRequest.Packages;
    }

    [Display(Name = "Install an Umbraco Template:")]
    public bool InstallUmbracoTemplate { get; set; }

    [Display(Name = "Umbraco Template Version:")]
    public string? UmbracoTemplateVersion { get; set; }

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

    [Display(Name = "Umbraco User Password:")]
    public string? UserPassword { get; set; }

    [Display(Name = "Include a Starter Kit:")]
    public bool IncludeStarterKit { get; set; }

    [Display(Name = "Starter Kit Package Id:")]
    public string? StarterKitPackage { get; set; }

    public string? Output { get; set; }

    [Display(Name = "Use Unattended Install:")]
    public bool UseUnattendedInstall { get; set; }

    [Display(Name = "Database Type")]
    public string? DatabaseType { get; set; }
    public List<string>? UmbracoVersions { get; set; }
}