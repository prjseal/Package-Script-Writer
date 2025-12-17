namespace PackageCliTool.Models.CommunityTemplates;

/// <summary>
/// Metadata for a community template from index.json
/// </summary>
public class CommunityTemplateMetadata
{
    /// <summary>
    /// Unique template name (kebab-case identifier)
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Display name for UI (Title Case)
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Template description
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Template author
    /// </summary>
    public string Author { get; set; } = "";

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// YAML file name
    /// </summary>
    public string FileName { get; set; } = "";

    /// <summary>
    /// When the template was created
    /// </summary>
    public string Created { get; set; } = "";
}
