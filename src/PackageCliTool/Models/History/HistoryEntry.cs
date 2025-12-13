using PackageCliTool.Models.Api;

namespace PackageCliTool.Models.History;

/// <summary>
/// Represents a single script generation history entry
/// </summary>
public class HistoryEntry
{
    /// <summary>
    /// Unique identifier for this history entry
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// When the script was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The script model used to generate the script
    /// </summary>
    public ScriptModel ScriptModel { get; set; } = new();

    /// <summary>
    /// Template name if generated from a template
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Whether the script was executed
    /// </summary>
    public bool WasExecuted { get; set; }

    /// <summary>
    /// Directory where the script was executed (if applicable)
    /// </summary>
    public string? ExecutionDirectory { get; set; }

    /// <summary>
    /// Exit code if the script was executed
    /// </summary>
    public int? ExitCode { get; set; }

    /// <summary>
    /// User-provided description or notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets a friendly display name for this history entry
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(Description))
        {
            return Description;
        }

        if (!string.IsNullOrWhiteSpace(TemplateName))
        {
            return $"From template: {TemplateName}";
        }

        return $"{ScriptModel.ProjectName ?? "Script"} - {Timestamp:yyyy-MM-dd HH:mm}";
    }
}
