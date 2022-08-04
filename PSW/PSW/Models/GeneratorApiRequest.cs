namespace PSW.Models;

public class GeneratorApiRequest
{
    public bool IsEmpty
    {
        get
        {
            return !InstallUmbracoTemplate && !CreateSolutionFile
                && !IncludeStarterKit && !UseUnattendedInstall
                && string.IsNullOrWhiteSpace(UmbracoTemplateVersion + Packages
                + SolutionName + ProjectName + UserFriendlyName + UserEmail
                + UserPassword + StarterKitPackage + DatabaseType);
        }
    }
    public bool InstallUmbracoTemplate { get; set; }
    public string? UmbracoTemplateVersion { get; set; }
    public string? Packages { get; set; }
    public bool CreateSolutionFile { get; set; }
    public string? SolutionName { get; set; }
    public string? ProjectName { get; set; }
    public string? ConnectionString { get; set; }
    public string? UserFriendlyName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPassword { get; set; }
    public bool IncludeStarterKit { get; set; }
    public string? StarterKitPackage { get; set; }
    public bool UseUnattendedInstall { get; set; }
    public string? DatabaseType { get; set; }
}