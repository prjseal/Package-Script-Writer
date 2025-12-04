using System.Reflection;

using PSW.Configuration;

namespace PSW.Extensions;

public static class StringExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) { return value; }

        if (value.Length > maxLength) { return value.Substring(0, maxLength) + "..."; }

        return value.Substring(0, value.Length);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "<Pending>")]
    public static string GetVersionText(this string version, string templateName, PSWConfig pswConfig)
    {
        if (string.IsNullOrWhiteSpace(version)) { return version; }

        if (templateName != "Umbraco.Templates") { return version; }

        var majorVersionNumberAsString = version.Split('.').FirstOrDefault();

        if (string.IsNullOrWhiteSpace(majorVersionNumberAsString)) { return version; }

        _ = int.TryParse(majorVersionNumberAsString, out var majorVersionNumber);

        if (pswConfig.UmbracoVersions == null || !pswConfig.UmbracoVersions.Any()) return version;

        if (!pswConfig.UmbracoVersions.Select(x => x.Version).Contains(majorVersionNumber)) { return version; }

        var versionInUse = pswConfig.UmbracoVersions.FirstOrDefault(x => x.Version == majorVersionNumber);

        var oneYearFromNow = DateTime.UtcNow.AddYears(1);
        var isLTS = versionInUse.ReleaseType == "LTS";
        var isEndOfLife = DateTime.UtcNow >= versionInUse.EndOfLife;
        var isSTS = !isLTS;
        var willEOLInLessThanAYear = !isEndOfLife && oneYearFromNow > versionInUse.EndOfLife;
        var isFutureRelease = versionInUse.ReleaseDate > DateTime.UtcNow;

        var emoji = "";

        if (isEndOfLife)
        {
            emoji = "💀";
        }
        else if ((isSTS && !isFutureRelease) || willEOLInLessThanAYear)
        {
            emoji = "✔️";
        }
        else
        {
            emoji = isFutureRelease ? "🔮" : "✅";
        }

        return emoji + " " + version;
    }
}