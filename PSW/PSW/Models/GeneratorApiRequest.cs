namespace PSW.Models;
public class GeneratorApiRequest
{
    public bool IsEmpty
    {
        get
        {
            return string.IsNullOrWhiteSpace(TemplateName) && !CreateSolutionFile
                && !IncludeStarterKit && !UseUnattendedInstall
                && TemplateName?.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) == true && string.IsNullOrWhiteSpace(TemplateVersion + Packages
                + SolutionName + ProjectName + UserFriendlyName + UserEmail
                + UserPassword + StarterKitPackage + DatabaseType)
                && !string.IsNullOrWhiteSpace(TemplateName) && !TemplateName.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) &&
                string.IsNullOrWhiteSpace(TemplateVersion + Packages
                + SolutionName + ProjectName + UserFriendlyName + UserEmail
                + UserPassword + DatabaseType);
        }
    }
    public string? TemplateName { get; set; }
    public string? TemplateVersion { get; set; }
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
    public bool OnelinerOutput { get; set; }
    public bool RemoveComments { get; set; }
}