namespace PackageCliTool.Models.Templates;

/// <summary>
/// Metadata about a template
/// </summary>
public class TemplateMetadata
{
    /// <summary>
    /// Template name (unique identifier)
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Author email or name
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// When the template was created
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the template was last modified
    /// </summary>
    public DateTime Modified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Template version (semver)
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Tags for categorization and filtering
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
