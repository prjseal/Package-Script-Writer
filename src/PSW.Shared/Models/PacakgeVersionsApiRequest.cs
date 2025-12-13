namespace PSW.Shared.Models;

/// <summary>
/// Request model for retrieving package versions
/// </summary>
public class PackageVersionsApiRequest
{
    /// <summary>The NuGet package ID to retrieve versions for</summary>
    public string PackageId { get; set; } = string.Empty;
}