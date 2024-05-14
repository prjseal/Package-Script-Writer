namespace PSW.Configuration
{
    public class PSWConfig
    {
        public static string SectionName => "PSW";
        public List<UmbracoVersion> UmbracoVersions { get; set; }
    }

    public class UmbracoVersion
    {
        public int Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? ReleaseType { get; set; }
        public DateTime? SupportPhase { get; set; }
        public DateTime? SecurityPhase { get; set; }
        public DateTime EndOfLife { get; set; }
    }
}