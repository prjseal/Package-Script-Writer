using PSW.Shared.Models;

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
    /// The generated script content
    /// </summary>
    public string ScriptContent { get; set; } = "";

    /// <summary>
    /// The script model used to generate the script
    /// </summary>
    public GeneratorApiRequest ScriptModel { get; set; } = new();

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

        return $"{GeneratorApiRequest?.ProjectName ?? "Script"} - {Timestamp:yyyy-MM-dd HH:mm}";
    }

    /// <summary>
    /// Gets a short summary of this entry
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(GeneratorApiRequest?.ProjectName))
        {
            parts.Add($"Project: {GeneratorApiRequest?.ProjectName}");
        }

        if (!string.IsNullOrWhiteSpace(TemplateName))
        {
            parts.Add($"Template: {TemplateName}");
        }

        if (WasExecuted)
        {
            parts.Add($"Executed (exit code: {ExitCode ?? 0})");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "No details";
    }
}
