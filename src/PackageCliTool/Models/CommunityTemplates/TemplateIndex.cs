namespace PackageCliTool.Models.CommunityTemplates;

/// <summary>
/// Represents the index.json file containing all community templates
/// </summary>
public class TemplateIndex
{
    /// <summary>
    /// List of available community templates
    /// </summary>
    public List<CommunityTemplateMetadata> Templates { get; set; } = new();

    /// <summary>
    /// When the index was last updated
    /// </summary>
    public string LastUpdated { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
}
