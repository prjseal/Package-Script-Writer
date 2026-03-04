namespace PackageCliTool.Models;

/// <summary>
/// Defines exit codes for the CLI tool.
/// Distinct codes allow AI agents and scripts to programmatically
/// determine the type of failure without parsing output text.
/// </summary>
public static class ExitCodes
{
    /// <summary>Operation completed successfully</summary>
    public const int Success = 0;

    /// <summary>General or unknown error</summary>
    public const int GeneralError = 1;

    /// <summary>Invalid arguments or input validation failed</summary>
    public const int ValidationError = 2;

    /// <summary>Network or API communication error</summary>
    public const int NetworkError = 3;

    /// <summary>Generated script execution failed</summary>
    public const int ScriptExecutionError = 4;

    /// <summary>File system or permission error</summary>
    public const int FileSystemError = 5;
}
