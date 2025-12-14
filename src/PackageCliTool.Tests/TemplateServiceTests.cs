using FluentAssertions;
using PackageCliTool.Exceptions;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.Templates;
using PackageCliTool.Services;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for TemplateService
/// </summary>
public class TemplateServiceTests : IDisposable
{
    private readonly string _testTemplatesDirectory;

    public TemplateServiceTests()
    {
        // Create a unique temporary directory for each test run
        _testTemplatesDirectory = Path.Combine(Path.GetTempPath(), $"psw-test-templates-{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        // Clean up test templates directory after each test
        if (Directory.Exists(_testTemplatesDirectory))
        {
            Directory.Delete(_testTemplatesDirectory, recursive: true);
        }
    }

    [Fact]
    public void TemplateService_Initialization_CreatesDirectory()
    {
        // Act
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Assert
        Directory.Exists(_testTemplatesDirectory).Should().BeTrue("templates directory should be created");
    }

    [Fact]
    public async Task SaveTemplateAsync_WithValidTemplate_SavesSuccessfully()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("test-template");

        // Act
        await templateService.SaveTemplateAsync(template);

        // Assert
        var templatePath = Path.Combine(_testTemplatesDirectory, "test-template.yaml");
        File.Exists(templatePath).Should().BeTrue("template file should be created");
    }

    [Fact]
    public async Task SaveTemplateAsync_UpdatesModifiedTimestamp()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("test-template");
        var originalModified = template.Metadata.Modified;

        // Wait a bit to ensure timestamp difference
        await Task.Delay(100);

        // Act
        await templateService.SaveTemplateAsync(template);

        // Assert
        template.Metadata.Modified.Should().BeAfter(originalModified, "modified timestamp should be updated");
    }

    [Fact]
    public async Task SaveTemplateAsync_WithEmptyName_ThrowsPswException()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("");

        // Act
        var act = async () => await templateService.SaveTemplateAsync(template);

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .WithMessage("*Provide a valid template name*");
    }

    [Fact]
    public async Task SaveTemplateAsync_WithWhitespaceName_ThrowsPswException()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("   ");

        // Act
        var act = async () => await templateService.SaveTemplateAsync(template);

        // Assert
        await act.Should().ThrowAsync<PswException>();
    }

    [Fact]
    public async Task LoadTemplateAsync_WithExistingTemplate_LoadsSuccessfully()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("load-test");
        await templateService.SaveTemplateAsync(template);

        // Act
        var loaded = await templateService.LoadTemplateAsync("load-test");

        // Assert
        loaded.Should().NotBeNull();
        loaded.Metadata.Name.Should().Be("load-test");
        loaded.Metadata.Description.Should().Be("Test template description");
    }

    [Fact]
    public async Task LoadTemplateAsync_WithNonExistentTemplate_ThrowsPswException()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Act
        var act = async () => await templateService.LoadTemplateAsync("non-existent");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .WithMessage("*Available templates:*");
    }

    [Fact]
    public async Task TemplateExists_WithExistingTemplate_ReturnsTrue()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("exists-test");
        await templateService.SaveTemplateAsync(template);

        // Act
        var exists = templateService.TemplateExists("exists-test");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void TemplateExists_WithNonExistentTemplate_ReturnsFalse()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Act
        var exists = templateService.TemplateExists("non-existent");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTemplate_WithExistingTemplate_DeletesSuccessfully()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("delete-test");
        await templateService.SaveTemplateAsync(template);

        // Act
        templateService.DeleteTemplate("delete-test");

        // Assert
        templateService.TemplateExists("delete-test").Should().BeFalse();
    }

    [Fact]
    public void DeleteTemplate_WithNonExistentTemplate_ThrowsPswException()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Act
        var act = () => templateService.DeleteTemplate("non-existent");

        // Assert
        act.Should().Throw<PswException>()
            .WithMessage("*Available templates:*");
    }

    [Fact]
    public async Task ListTemplates_WithNoTemplates_ReturnsEmptyList()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Act
        var templates = templateService.ListTemplates();

        // Assert
        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task ListTemplates_WithMultipleTemplates_ReturnsAllTemplates()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        await templateService.SaveTemplateAsync(CreateTestTemplate("template1"));
        await templateService.SaveTemplateAsync(CreateTestTemplate("template2"));
        await templateService.SaveTemplateAsync(CreateTestTemplate("template3"));

        // Act
        var templates = templateService.ListTemplates();

        // Assert
        templates.Should().HaveCount(3);
        templates.Select(t => t.Name).Should().Contain(new[] { "template1", "template2", "template3" });
    }

    [Fact]
    public async Task ListTemplates_ReturnsSortedByName()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        await templateService.SaveTemplateAsync(CreateTestTemplate("zebra"));
        await templateService.SaveTemplateAsync(CreateTestTemplate("alpha"));
        await templateService.SaveTemplateAsync(CreateTestTemplate("bravo"));

        // Act
        var templates = templateService.ListTemplates();

        // Assert
        templates.Select(t => t.Name).Should().ContainInOrder("alpha", "bravo", "zebra");
    }

    [Fact]
    public async Task ListTemplates_WithTagFilter_ReturnsOnlyMatchingTemplates()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        var template1 = CreateTestTemplate("prod-template");
        template1.Metadata.Tags = new List<string> { "production", "umbraco-14" };

        var template2 = CreateTestTemplate("dev-template");
        template2.Metadata.Tags = new List<string> { "development", "testing" };

        var template3 = CreateTestTemplate("prod-template2");
        template3.Metadata.Tags = new List<string> { "production", "umbraco-15" };

        await templateService.SaveTemplateAsync(template1);
        await templateService.SaveTemplateAsync(template2);
        await templateService.SaveTemplateAsync(template3);

        // Act
        var productionTemplates = templateService.ListTemplates("production");

        // Assert
        productionTemplates.Should().HaveCount(2);
        productionTemplates.Select(t => t.Name).Should().Contain(new[] { "prod-template", "prod-template2" });
    }

    [Fact]
    public async Task GetAllTemplatesAsync_ReturnsAllTemplateMetadata()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        await templateService.SaveTemplateAsync(CreateTestTemplate("template1"));
        await templateService.SaveTemplateAsync(CreateTestTemplate("template2"));

        // Act
        var templates = await templateService.GetAllTemplatesAsync();

        // Assert
        templates.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExportTemplateAsync_ExportsToSpecifiedPath()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("export-test");
        await templateService.SaveTemplateAsync(template);

        var exportPath = Path.Combine(Path.GetTempPath(), $"export-{Guid.NewGuid()}.yaml");

        try
        {
            // Act
            await templateService.ExportTemplateAsync("export-test", exportPath);

            // Assert
            File.Exists(exportPath).Should().BeTrue("exported file should exist");
            var content = await File.ReadAllTextAsync(exportPath);
            content.Should().Contain("export-test", "exported content should contain template name");
        }
        finally
        {
            // Cleanup
            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }
        }
    }

    [Fact]
    public async Task ImportTemplateAsync_ImportsFromFile()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var originalTemplate = CreateTestTemplate("import-test");

        // Create a temporary file to import from
        var tempImportFile = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}.yaml");
        await templateService.SaveTemplateAsync(originalTemplate);
        await templateService.ExportTemplateAsync("import-test", tempImportFile);

        // Clear templates directory
        Directory.Delete(_testTemplatesDirectory, true);

        try
        {
            // Act
            var templateService2 = new TemplateService(templatesDirectory: _testTemplatesDirectory);
            var imported = await templateService2.ImportTemplateAsync(tempImportFile);

            // Assert
            imported.Should().NotBeNull();
            imported.Metadata.Name.Should().Be("import-test");
            templateService2.TemplateExists("import-test").Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempImportFile))
            {
                File.Delete(tempImportFile);
            }
        }
    }

    [Fact]
    public async Task ImportTemplateAsync_WithNewName_RenamesTemplate()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var originalTemplate = CreateTestTemplate("original-name");

        var tempImportFile = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}.yaml");
        await templateService.SaveTemplateAsync(originalTemplate);
        await templateService.ExportTemplateAsync("original-name", tempImportFile);

        try
        {
            // Act
            var imported = await templateService.ImportTemplateAsync(tempImportFile, "renamed-template");

            // Assert
            imported.Metadata.Name.Should().Be("renamed-template");
            templateService.TemplateExists("renamed-template").Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempImportFile))
            {
                File.Delete(tempImportFile);
            }
        }
    }

    [Fact]
    public async Task ImportTemplateAsync_WithNonExistentFile_ThrowsPswException()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Act
        var act = async () => await templateService.ImportTemplateAsync("/nonexistent/path.yaml");

        // Assert
        await act.Should().ThrowAsync<PswException>()
            .WithMessage("*Provide a valid template file path*");
    }

    [Fact]
    public void ToScriptModel_ConvertsTemplateCorrectly()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("script-model-test");
        template.Configuration.Project.Name = "TestProject";
        template.Configuration.Template.Version = "14.0.0";

        // Act
        var scriptModel = templateService.ToScriptModel(template);

        // Assert
        scriptModel.Should().NotBeNull();
        scriptModel.ProjectName.Should().Be("TestProject");
        scriptModel.TemplateVersion.Should().Be("14.0.0");
    }

    [Fact]
    public void ToScriptModel_WithOverrides_AppliesOverrides()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var template = CreateTestTemplate("override-test");
        template.Configuration.Project.Name = "OriginalProject";

        var overrides = new Dictionary<string, object>
        {
            ["projectname"] = "OverriddenProject",
            ["databasetype"] = "SqlServer"
        };

        // Act
        var scriptModel = templateService.ToScriptModel(template, overrides);

        // Assert
        scriptModel.ProjectName.Should().Be("OverriddenProject");
        scriptModel.DatabaseType.Should().Be("SqlServer");
    }

    [Fact]
    public async Task TemplateService_RoundTrip_SaveAndLoad()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);
        var original = CreateTestTemplate("roundtrip-test");
        original.Metadata.Description = "Test roundtrip serialization";
        original.Metadata.Tags = new List<string> { "test", "roundtrip", "yaml" };

        // Act
        await templateService.SaveTemplateAsync(original);
        var loaded = await templateService.LoadTemplateAsync("roundtrip-test");

        // Assert
        loaded.Metadata.Name.Should().Be(original.Metadata.Name);
        loaded.Metadata.Description.Should().Be(original.Metadata.Description);
        loaded.Metadata.Tags.Should().BeEquivalentTo(original.Metadata.Tags);
    }

    [Fact]
    public async Task ListTemplates_WithCorruptedTemplateFile_SkipsCorruptedFile()
    {
        // Arrange
        var templateService = new TemplateService(templatesDirectory: _testTemplatesDirectory);

        // Create a valid template
        await templateService.SaveTemplateAsync(CreateTestTemplate("valid-template"));

        // Create a corrupted YAML file
        var corruptedFile = Path.Combine(_testTemplatesDirectory, "corrupted.yaml");
        File.WriteAllText(corruptedFile, "invalid: yaml: {{{");

        // Act
        var templates = templateService.ListTemplates();

        // Assert
        templates.Should().HaveCount(1, "corrupted file should be skipped");
        templates[0].Name.Should().Be("valid-template");
    }

    /// <summary>
    /// Helper method to create a test template
    /// </summary>
    private static Template CreateTestTemplate(string name)
    {
        return new Template
        {
            Metadata = new TemplateMetadata
            {
                Name = name,
                Description = "Test template description",
                Author = "Test Author",
                Version = "1.0.0",
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Tags = new List<string>()
            },
            Configuration = new TemplateConfiguration
            {
                Template = new TemplateInfo
                {
                    Name = "Umbraco.Templates",
                    Version = "14.0.0"
                },
                Project = new ProjectInfo
                {
                    Name = "TestProject",
                    CreateSolution = true,
                    SolutionName = "TestSolution"
                },
                Packages = new List<PackageConfig>
                {
                    new PackageConfig { Name = "uSync", Version = "latest" }
                },
                StarterKit = new StarterKitConfig
                {
                    Enabled = false,
                    Package = null
                },
                Docker = new DockerConfig
                {
                    Dockerfile = false,
                    DockerCompose = false
                },
                Unattended = new UnattendedConfig
                {
                    Enabled = true,
                    Database = new DatabaseConfig
                    {
                        Type = "SQLite",
                        ConnectionString = null
                    },
                    Admin = new AdminConfig
                    {
                        Name = "Administrator",
                        Email = "admin@example.com",
                        Password = "test123"
                    }
                },
                Output = new OutputConfig
                {
                    Oneliner = false,
                    RemoveComments = false,
                    IncludePrerelease = false
                },
                Execution = new ExecutionConfig
                {
                    AutoRun = false,
                    RunDirectory = "."
                }
            }
        };
    }
}
