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

    #region Test Endpoint Tests

    [Fact]
    public async Task Test_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Hello, world!", content);
        Assert.Contains("The time is", content);
    }

    [Fact]
    public async Task Test_ReturnsNonEmptyContent()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/test");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    #endregion

    #region ClearCache Endpoint Tests

    [Fact]
    public async Task ClearCache_ReturnsSuccessAndConfirmationMessage()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/clearcache");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cache Cleared at", content);
    }

    [Fact]
    public async Task ClearCache_ReturnsExpectedStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/ScriptGeneratorApi/clearcache");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    #endregion

    #region GenerateScript Endpoint Tests

    [Fact]
    public async Task GenerateScript_WithValidRequest_Returns200OK()
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
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithEmptyRequest_Returns200OKWithDefaults()
    {
        // Arrange - Create an empty request (all default values)
        var request = new GeneratorApiRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithDockerfileEnabled_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            IncludeDockerfile = true,
            IncludeDockerCompose = false,
            ProjectName = "DockerProject",
            SolutionName = "DockerSolution"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithDockerComposeEnabled_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            IncludeDockerfile = true,
            IncludeDockerCompose = true,
            ProjectName = "ComposeProject",
            SolutionName = "ComposeSolution"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithStarterKit_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            IncludeStarterKit = true,
            StarterKitPackage = "TestStarterKit",
            ProjectName = "StarterKitProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithUnattendedInstall_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            UseUnattendedInstall = true,
            UserFriendlyName = "Test User",
            UserEmail = "test@example.com",
            UserPassword = "TestPassword123!",
            ProjectName = "UnattendedProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithDatabaseConfiguration_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            DatabaseType = "SQLite",
            ConnectionString = "Data Source=test.db",
            ProjectName = "DatabaseProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithOnelinerOutput_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            OnelinerOutput = true,
            ProjectName = "OnelinerProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithRemoveComments_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            RemoveComments = true,
            ProjectName = "NoCommentsProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithTemplateName_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            TemplateName = "Umbraco.Templates.UmbracoCms",
            TemplateVersion = "13.0.0",
            ProjectName = "TemplateProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithPackages_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            Packages = "Package1,Package2,Package3",
            ProjectName = "PackagesProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GenerateScript_WithSolutionFile_ReturnsScriptContent()
    {
        // Arrange
        var request = new GeneratorApiRequest
        {
            CreateSolutionFile = true,
            SolutionName = "MySolution",
            ProjectName = "MyProject"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/generatescript", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }

    #endregion

    #region GetPackageVersions Endpoint Tests

    [Fact]
    public async Task GetPackageVersions_WithValidPackageId_Returns200OKAndVersionList()
    {
        // Arrange
        var request = new PackageVersionsApiRequest
        {
            PackageId = "Newtonsoft.Json"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/getpackageversions", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());

        var versions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(versions);
        Assert.NotEmpty(versions);
    }

    [Fact]
    public async Task GetPackageVersions_ReturnsValidJsonArray()
    {
        // Arrange
        var request = new PackageVersionsApiRequest
        {
            PackageId = "Newtonsoft.Json"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/getpackageversions", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var versions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(versions);
        Assert.IsType<List<string>>(versions);
        Assert.All(versions, version => Assert.False(string.IsNullOrWhiteSpace(version)));
    }

    [Fact]
    public async Task GetPackageVersions_WithPopularPackage_ReturnsManyVersions()
    {
        // Arrange
        var request = new PackageVersionsApiRequest
        {
            PackageId = "Newtonsoft.Json"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/getpackageversions", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var versions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(versions);
        Assert.True(versions.Count > 0, "Expected at least one version for Newtonsoft.Json");
    }

    [Fact]
    public async Task GetPackageVersions_WithDifferentCasing_ReturnsVersionList()
    {
        // Arrange - Test case insensitivity
        var request = new PackageVersionsApiRequest
        {
            PackageId = "NEWTONSOFT.JSON"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ScriptGeneratorApi/getpackageversions", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var versions = await response.Content.ReadFromJsonAsync<List<string>>();
        Assert.NotNull(versions);
        Assert.NotEmpty(versions);
    }

    #endregion
}
