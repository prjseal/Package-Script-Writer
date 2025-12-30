using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Model for script generation configuration
/// </summary>
public class ScriptModel
{
    /// <summary>Gets or sets the Umbraco template package name</summary>
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>Gets or sets the template version</summary>
    [JsonPropertyName("templateVersion")]
    public string TemplateVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets whether to create a solution file</summary>
    [JsonPropertyName("createSolutionFile")]
    public bool CreateSolutionFile { get; set; }

    /// <summary>Gets or sets the solution name</summary>
    [JsonPropertyName("solutionName")]
    public string? SolutionName { get; set; }

    /// <summary>Gets or sets the project name</summary>
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Gets or sets whether to use unattended installation</summary>
    [JsonPropertyName("useUnattendedInstall")]
    public bool UseUnattendedInstall { get; set; }

    /// <summary>Gets or sets the database type (e.g., SQLite, SQL Server)</summary>
    [JsonPropertyName("databaseType")]
    public string? DatabaseType { get; set; }

    /// <summary>Gets or sets the database connection string</summary>
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    /// <summary>Gets or sets the Umbraco admin user friendly name</summary>
    [JsonPropertyName("userFriendlyName")]
    public string? UserFriendlyName { get; set; }

    /// <summary>Gets or sets the Umbraco admin user email</summary>
    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    /// <summary>Gets or sets the Umbraco admin user password</summary>
    [JsonPropertyName("userPassword")]
    public string? UserPassword { get; set; }

    /// <summary>Gets or sets the comma-separated list of packages to install</summary>
    [JsonPropertyName("packages")]
    public string? Packages { get; set; }

    /// <summary>Gets or sets whether to include a starter kit</summary>
    [JsonPropertyName("includeStarterKit")]
    public bool IncludeStarterKit { get; set; }

    /// <summary>Gets or sets the starter kit package name</summary>
    [JsonPropertyName("starterKitPackage")]
    public string? StarterKitPackage { get; set; }

    /// <summary>Gets or sets whether Docker support can be included for this template version</summary>
    [JsonPropertyName("canIncludeDocker")]
    public bool CanIncludeDocker { get; set; }

    /// <summary>Gets or sets whether to include a Dockerfile</summary>
    [JsonPropertyName("includeDockerfile")]
    public bool IncludeDockerfile { get; set; }

    /// <summary>Gets or sets whether to include Docker Compose configuration</summary>
    [JsonPropertyName("includeDockerCompose")]
    public bool IncludeDockerCompose { get; set; }

    /// <summary>Gets or sets whether to enable the Content Delivery API</summary>
    [JsonPropertyName("enableContentDeliveryApi")]
    public bool EnableContentDeliveryApi { get; set; }

    /// <summary>Gets or sets whether to output the script as a single line command</summary>
    [JsonPropertyName("onelinerOutput")]
    public bool OnelinerOutput { get; set; }

    /// <summary>Gets or sets whether to remove comments from the generated script</summary>
    [JsonPropertyName("removeComments")]
    public bool RemoveComments { get; set; }
}
