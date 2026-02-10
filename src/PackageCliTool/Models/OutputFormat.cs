namespace PackageCliTool.Models;

/// <summary>
/// Specifies the output format for CLI results
/// </summary>
public enum OutputFormat
{
    /// <summary>Default rich Spectre.Console output with colours and panels</summary>
    Default,

    /// <summary>Plain text output with no ANSI codes or decoration</summary>
    Plain,

    /// <summary>Structured JSON output for machine consumption</summary>
    Json
}
