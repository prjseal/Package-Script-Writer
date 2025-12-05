using System.Net;
using System.Net.Http.Json;
using PSW.Models;

namespace PSW.IntegrationTests;

/// <summary>
/// Integration tests for the ScriptGeneratorApi endpoints
/// </summary>
public class ScriptGeneratorApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ScriptGeneratorApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Test_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/test");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello, world!", content);
        Assert.Contains("The time is", content);
    }

    [Fact]
    public async Task ClearCache_ReturnsSuccessAndConfirmationMessage()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/clearcache");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cache Cleared at", content);
    }

    [Fact]
    public async Task GenerateScript_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            IncludeStarterKit = true,
            IncludeDockerfile = false,
            IncludeDockerCompose = false,
            CreateSolutionFile = true,
            UseUnattendedInstall = true,
            OnelinerOutput = false,
            ProjectName = "TestProject",
            SolutionName = "TestSolution"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithEmptyRequest_ReturnsScriptWithDefaults()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            IsEmpty = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GetPackageVersions_WithValidPackageId_ReturnsVersionList()
    {
        // Arrange
        var request = new PackageVersionsApiRequest
        {
            PackageId = "Newtonsoft.Json"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/getpackageversions", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var versions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(versions);
        Assert.NotEmpty(versions);
    }
}
