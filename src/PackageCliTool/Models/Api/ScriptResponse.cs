using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Response model for generated installation scripts
/// </summary>
public class ScriptResponse
{
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;
}
