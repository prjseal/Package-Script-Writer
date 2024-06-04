﻿using System.Text.Json.Serialization;

namespace PSW.Models.NuGet;

public class PackageVersions
{
    public PackageVersions(string[] versions)
    {
        Versions = versions;
    }

    [JsonPropertyName("versions")]
    public string[] Versions { get; set; }
}