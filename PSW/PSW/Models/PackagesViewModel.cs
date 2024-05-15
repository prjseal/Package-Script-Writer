using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

using static PSW.Models.PackageFeed;

namespace PSW.Models;
public class PackagesViewModel
{
    public PackagesViewModel() { }

    public PackagesViewModel(GeneratorApiRequest apiRequest)
    {
        TemplateName = apiRequest.TemplateName;
        TemplateVersion = apiRequest.TemplateVersion;
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
        OnelinerOutput = apiRequest.OnelinerOutput;
        RemoveComments = apiRequest.RemoveComments;
    }

    public List<SelectListItem> TemplateNames { get; set; }

    [Display(Name = "Template Name:")]
    public string TemplateName { get; set; }

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

    [Display(Name = "Starter Kit Package Id:")]
    public string? StarterKitPackage { get; set; }

    public string? Output { get; set; }

    [Display(Name = "Use Unattended Install:")]
    public bool UseUnattendedInstall { get; set; }

    [Display(Name = "Database Type")]
    public string? DatabaseType { get; set; }
    public List<string>? UmbracoVersions { get; set; }

    [Display(Name = "Output to single line command")]
    public bool OnelinerOutput { get; set; }
    [Display(Name = "Remove comments")]
    public bool RemoveComments { get; set; }
    public bool HasQueryString { get; set; }
}