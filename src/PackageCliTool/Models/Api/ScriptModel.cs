using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Model for script generation configuration
/// </summary>
public class ScriptModel
{
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; } = string.Empty;

    [JsonPropertyName("templateVersion")]
    public string TemplateVersion { get; set; } = string.Empty;

    [JsonPropertyName("createSolutionFile")]
    public bool CreateSolutionFile { get; set; }

    [JsonPropertyName("solutionName")]
    public string? SolutionName { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("useUnattendedInstall")]
    public bool UseUnattendedInstall { get; set; }

    [JsonPropertyName("databaseType")]
    public string? DatabaseType { get; set; }

    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    [JsonPropertyName("userFriendlyName")]
    public string? UserFriendlyName { get; set; }

    [JsonPropertyName("userEmail")]
    public string? UserEmail { get; set; }

    [JsonPropertyName("userPassword")]
    public string? UserPassword { get; set; }

    [JsonPropertyName("packages")]
    public string? Packages { get; set; }

    [JsonPropertyName("includeStarterKit")]
    public bool IncludeStarterKit { get; set; }

    [JsonPropertyName("starterKitPackage")]
    public string? StarterKitPackage { get; set; }

    [JsonPropertyName("canIncludeDocker")]
    public bool CanIncludeDocker { get; set; }

    [JsonPropertyName("includeDockerfile")]
    public bool IncludeDockerfile { get; set; }

    [JsonPropertyName("includeDockerCompose")]
    public bool IncludeDockerCompose { get; set; }

    [JsonPropertyName("onelinerOutput")]
    public bool OnelinerOutput { get; set; }

    [JsonPropertyName("removeComments")]
    public bool RemoveComments { get; set; }
}
