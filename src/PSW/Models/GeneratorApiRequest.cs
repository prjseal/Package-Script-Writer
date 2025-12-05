namespace PSW.Models;

/// <summary>
/// Request model for generating package installation scripts
/// </summary>
public class GeneratorApiRequest
{
    public bool IsEmpty
    {
        get
        {
            return string.IsNullOrWhiteSpace(TemplateName) && !CreateSolutionFile
                && !IncludeStarterKit && !IncludeDockerfile && !IncludeDockerCompose && !UseUnattendedInstall
                && TemplateName?.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) == true && string.IsNullOrWhiteSpace(TemplateVersion + Packages
                + SolutionName + ProjectName + UserFriendlyName + UserEmail
                + UserPassword + StarterKitPackage + DatabaseType)
                && !string.IsNullOrWhiteSpace(TemplateName) && !TemplateName.Equals(GlobalConstants.TEMPLATE_NAME_UMBRACO) &&
                string.IsNullOrWhiteSpace(TemplateVersion + Packages
                + SolutionName + ProjectName + UserFriendlyName + UserEmail
                + UserPassword + DatabaseType);
        }
    }

    /// <summary>The name of the template to use</summary>
    public string? TemplateName { get; set; }

    /// <summary>The version of the template</summary>
    public string? TemplateVersion { get; set; }

    /// <summary>Comma-separated list of packages to install</summary>
    public string? Packages { get; set; }

    /// <summary>Whether to create a solution file</summary>
    public bool CreateSolutionFile { get; set; }

    /// <summary>The name for the solution</summary>
    public string? SolutionName { get; set; }

    /// <summary>The name for the project</summary>
    public string? ProjectName { get; set; }

    /// <summary>Database connection string</summary>
    public string? ConnectionString { get; set; }

    /// <summary>User-friendly name for the installation</summary>
    public string? UserFriendlyName { get; set; }

    /// <summary>User email address</summary>
    public string? UserEmail { get; set; }

    /// <summary>User password</summary>
    public string? UserPassword { get; set; }

    /// <summary>Whether to include a starter kit</summary>
    public bool IncludeStarterKit { get; set; }

    /// <summary>Whether to include a Dockerfile</summary>
    public bool IncludeDockerfile { get; set; }

    /// <summary>Whether to include Docker Compose configuration</summary>
    public bool IncludeDockerCompose { get; set; }

    /// <summary>The starter kit package to use</summary>
    public string? StarterKitPackage { get; set; }

    /// <summary>Whether to use unattended installation</summary>
    public bool UseUnattendedInstall { get; set; }

    /// <summary>The type of database to use</summary>
    public string? DatabaseType { get; set; }

    /// <summary>Whether to output as a one-liner script</summary>
    public bool OnelinerOutput { get; set; }

    /// <summary>Whether to remove comments from the generated script</summary>
    public bool RemoveComments { get; set; }
}