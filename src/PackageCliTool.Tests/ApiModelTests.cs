using System.Text.Json;
using FluentAssertions;
using PackageCliTool.Models.Api;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for API model classes
/// </summary>
public class ApiModelTests
{
    [Fact]
    public void PackageVersionRequest_DefaultValues_AreCorrect()
    {
        // Act
        var request = new PackageVersionRequest();

        // Assert
        request.PackageId.Should().Be(string.Empty);
        request.IncludePrerelease.Should().BeFalse();
    }

    [Fact]
    public void PackageVersionRequest_Serialization_UsesJsonPropertyNames()
    {
        // Arrange
        var request = new PackageVersionRequest
        {
            PackageId = "uSync",
            IncludePrerelease = true
        };

        // Act
        var json = JsonSerializer.Serialize(request);

        // Assert
        json.Should().Contain("\"packageId\":", "should use camelCase JSON property name");
        json.Should().Contain("\"includePrerelease\":");
    }

    [Fact]
    public void PackageVersionRequest_Deserialization_MapsJsonPropertiesCorrectly()
    {
        // Arrange
        var json = @"{
            ""packageId"": ""Umbraco.Forms"",
            ""includePrerelease"": true
        }";

        // Act
        var request = JsonSerializer.Deserialize<PackageVersionRequest>(json);

        // Assert
        request.Should().NotBeNull();
        request!.PackageId.Should().Be("Umbraco.Forms");
        request.IncludePrerelease.Should().BeTrue();
    }

    [Fact]
    public void PackageVersionRequest_WithoutPrerelease_SerializesCorrectly()
    {
        // Arrange
        var request = new PackageVersionRequest
        {
            PackageId = "uSync",
            IncludePrerelease = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<PackageVersionRequest>(json);

        // Assert
        deserialized!.PackageId.Should().Be("uSync");
        deserialized.IncludePrerelease.Should().BeFalse();
    }

    [Fact]
    public void PackageVersionRequest_RoundTrip_PreservesAllData()
    {
        // Arrange
        var original = new PackageVersionRequest
        {
            PackageId = "Umbraco.Deploy",
            IncludePrerelease = true
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<PackageVersionRequest>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void PackageVersionResponse_DefaultValues_AreCorrect()
    {
        // Act
        var response = new PackageVersionResponse();

        // Assert
        response.Versions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void PackageVersionResponse_Serialization_UsesJsonPropertyNames()
    {
        // Arrange
        var response = new PackageVersionResponse
        {
            Versions = new List<string> { "17.0.0", "17.0.1", "17.0.2" }
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"versions\":", "should use camelCase JSON property name");
    }

    [Fact]
    public void PackageVersionResponse_Deserialization_MapsJsonPropertiesCorrectly()
    {
        // Arrange
        var json = @"{
            ""versions"": [""14.0.0"", ""14.0.1"", ""14.0.2"", ""14.1.0""]
        }";

        // Act
        var response = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response!.Versions.Should().HaveCount(4);
        response.Versions.Should().Contain("14.0.0");
        response.Versions.Should().Contain("14.1.0");
    }

    [Fact]
    public void PackageVersionResponse_WithEmptyVersions_SerializesCorrectly()
    {
        // Arrange
        var response = new PackageVersionResponse
        {
            Versions = new List<string>()
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        deserialized!.Versions.Should().BeEmpty();
    }

    [Fact]
    public void PackageVersionResponse_WithMultipleVersions_SerializesCorrectly()
    {
        // Arrange
        var response = new PackageVersionResponse
        {
            Versions = new List<string>
            {
                "10.0.0",
                "10.1.0",
                "10.2.0",
                "11.0.0",
                "11.1.0",
                "12.0.0-beta",
                "12.0.0"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        deserialized!.Versions.Should().HaveCount(7);
        deserialized.Versions.Should().ContainInOrder(response.Versions);
    }

    [Fact]
    public void PackageVersionResponse_WithPrereleaseVersions_SerializesCorrectly()
    {
        // Arrange
        var response = new PackageVersionResponse
        {
            Versions = new List<string>
            {
                "14.0.0",
                "14.1.0-beta1",
                "14.1.0-beta2",
                "14.1.0-rc1",
                "14.1.0"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        deserialized!.Versions.Should().HaveCount(5);
        deserialized.Versions.Should().Contain("14.1.0-beta1");
        deserialized.Versions.Should().Contain("14.1.0-rc1");
    }

    [Fact]
    public void PackageVersionResponse_RoundTrip_PreservesAllData()
    {
        // Arrange
        var original = new PackageVersionResponse
        {
            Versions = new List<string> { "1.0.0", "1.1.0", "2.0.0" }
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void PackageVersionRequest_Deserialization_IsCaseInsensitive()
    {
        // Arrange - Use PascalCase property names
        var json = @"{
            ""PackageId"": ""uSync"",
            ""IncludePrerelease"": true
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Act
        var request = JsonSerializer.Deserialize<PackageVersionRequest>(json, options);

        // Assert
        request.Should().NotBeNull();
        request!.PackageId.Should().Be("uSync");
        request.IncludePrerelease.Should().BeTrue();
    }

    [Fact]
    public void PackageVersionResponse_Deserialization_IsCaseInsensitive()
    {
        // Arrange - Use PascalCase property names
        var json = @"{
            ""Versions"": [""1.0.0"", ""2.0.0""]
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Act
        var response = JsonSerializer.Deserialize<PackageVersionResponse>(json, options);

        // Assert
        response.Should().NotBeNull();
        response!.Versions.Should().HaveCount(2);
    }

    [Fact]
    public void PackageVersionRequest_WithSpecialCharacters_SerializesCorrectly()
    {
        // Arrange
        var request = new PackageVersionRequest
        {
            PackageId = "Umbraco.Community.Package-Name_123",
            IncludePrerelease = false
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<PackageVersionRequest>(json);

        // Assert
        deserialized!.PackageId.Should().Be("Umbraco.Community.Package-Name_123");
    }

    [Fact]
    public void PackageVersionResponse_WithSemanticVersions_SerializesCorrectly()
    {
        // Arrange
        var response = new PackageVersionResponse
        {
            Versions = new List<string>
            {
                "1.0.0",
                "1.0.1",
                "1.1.0",
                "2.0.0-alpha",
                "2.0.0-beta.1",
                "2.0.0-rc.1+build.123",
                "2.0.0"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<PackageVersionResponse>(json);

        // Assert
        deserialized!.Versions.Should().HaveCount(7);
        deserialized.Versions.Should().Contain("2.0.0-rc.1+build.123", "should preserve full semantic version");
    }
}
