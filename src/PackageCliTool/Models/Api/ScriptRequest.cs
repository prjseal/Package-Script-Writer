using System.Text.Json.Serialization;

namespace PackageCliTool.Models.Api;

/// <summary>
/// Request model for generating installation scripts
/// </summary>
public class ScriptRequest
{
    [JsonPropertyName("model")]
    public ScriptModel Model { get; set; } = new();
}
