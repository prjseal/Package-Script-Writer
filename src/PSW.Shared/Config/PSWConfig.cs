namespace PSW.Shared.Configuration;

public class PSWConfig
{
    public static string SectionName => "PSW";
    public int CachingTimeInMins { get; set; }
    public string CommunityTemplatesApiUrl { get; set; } = "https://packagescriptwriter.com";
    public List<UmbracoVersion> UmbracoVersions { get; set; } = new();
}

public class UmbracoVersion
{
    public int Version { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? ReleaseType { get; set; }
    public DateTime? SupportPhase { get; set; }
    public DateTime? SecurityPhase { get; set; }
    public DateTime EndOfLife { get; set; }
    public string? Url { get; set; }
}