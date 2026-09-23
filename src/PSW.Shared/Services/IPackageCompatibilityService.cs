namespace PSW.Shared.Services;

public interface IPackageCompatibilityService
{
    /// <summary>
    /// Finds the newest version of a package whose Umbraco.Cms dependency range accepts the given Umbraco version.
    /// </summary>
    Task<PackageCompatibilityResult> GetCompatibleVersionAsync(string packageId, string umbracoVersion);
}

public enum PackageCompatibilityStatus
{
    /// <summary>A compatible version was found.</summary>
    Compatible,

    /// <summary>The package depends on Umbraco, but no version accepts the given Umbraco version.</summary>
    NoCompatibleVersion,

    /// <summary>Compatibility can't be determined (no Umbraco dependency found, package not found, or NuGet unreachable).</summary>
    Unknown
}

public record PackageCompatibilityResult(PackageCompatibilityStatus Status, string? Version = null)
{
    public static PackageCompatibilityResult Unknown { get; } = new(PackageCompatibilityStatus.Unknown);
    public static PackageCompatibilityResult NoCompatibleVersion { get; } = new(PackageCompatibilityStatus.NoCompatibleVersion);
}
