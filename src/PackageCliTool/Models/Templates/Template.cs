namespace PackageCliTool.Models.Templates;

/// <summary>
/// Represents a complete template with metadata and configuration
/// </summary>
public class Template
{
    /// <summary>
    /// Template metadata (name, description, author, etc.)
    /// </summary>
    public TemplateMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Template configuration (packages, project settings, etc.)
    /// </summary>
    public TemplateConfiguration Configuration { get; set; } = new();
}
