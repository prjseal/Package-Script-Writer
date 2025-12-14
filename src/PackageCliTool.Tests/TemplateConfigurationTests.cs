using FluentAssertions;
using PackageCliTool.Models.Templates;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for Template configuration models
/// </summary>
public class TemplateConfigurationTests
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public TemplateConfigurationTests()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    [Fact]
    public void TemplateConfiguration_DefaultValues_AreCorrect()
    {
        // Act
        var config = new TemplateConfiguration();

        // Assert
        config.Template.Should().NotBeNull();
        config.Project.Should().NotBeNull();
        config.Packages.Should().NotBeNull().And.BeEmpty();
        config.StarterKit.Should().NotBeNull();
        config.Docker.Should().NotBeNull();
        config.Unattended.Should().NotBeNull();
        config.Output.Should().NotBeNull();
        config.Execution.Should().NotBeNull();
    }

    [Fact]
    public void TemplateInfo_DefaultValues_AreCorrect()
    {
        // Act
        var templateInfo = new TemplateInfo();

        // Assert
        templateInfo.Name.Should().Be("Umbraco.Templates");
        templateInfo.Version.Should().Be("");
    }

    [Fact]
    public void ProjectInfo_DefaultValues_AreCorrect()
    {
        // Act
        var projectInfo = new ProjectInfo();

        // Assert
        projectInfo.Name.Should().Be("MyProject");
        projectInfo.CreateSolution.Should().BeTrue();
        projectInfo.SolutionName.Should().BeNull();
    }

    [Fact]
    public void PackageConfig_DefaultValues_AreCorrect()
    {
        // Act
        var packageConfig = new PackageConfig();

        // Assert
        packageConfig.Name.Should().Be("");
        packageConfig.Version.Should().Be("latest");
    }

    [Fact]
    public void StarterKitConfig_DefaultValues_AreCorrect()
    {
        // Act
        var starterKitConfig = new StarterKitConfig();

        // Assert
        starterKitConfig.Enabled.Should().BeFalse();
        starterKitConfig.Package.Should().BeNull();
    }

    [Fact]
    public void DockerConfig_DefaultValues_AreCorrect()
    {
        // Act
        var dockerConfig = new DockerConfig();

        // Assert
        dockerConfig.Dockerfile.Should().BeFalse();
        dockerConfig.DockerCompose.Should().BeFalse();
    }

    [Fact]
    public void UnattendedConfig_DefaultValues_AreCorrect()
    {
        // Act
        var unattendedConfig = new UnattendedConfig();

        // Assert
        unattendedConfig.Enabled.Should().BeFalse();
        unattendedConfig.Database.Should().NotBeNull();
        unattendedConfig.Admin.Should().NotBeNull();
    }

    [Fact]
    public void DatabaseConfig_DefaultValues_AreCorrect()
    {
        // Act
        var databaseConfig = new DatabaseConfig();

        // Assert
        databaseConfig.Type.Should().Be("SQLite");
        databaseConfig.ConnectionString.Should().BeNull();
    }

    [Fact]
    public void AdminConfig_DefaultValues_AreCorrect()
    {
        // Act
        var adminConfig = new AdminConfig();

        // Assert
        adminConfig.Name.Should().Be("Administrator");
        adminConfig.Email.Should().Be("admin@example.com");
        adminConfig.Password.Should().Be("<prompt>");
    }

    [Fact]
    public void OutputConfig_DefaultValues_AreCorrect()
    {
        // Act
        var outputConfig = new OutputConfig();

        // Assert
        outputConfig.Oneliner.Should().BeFalse();
        outputConfig.RemoveComments.Should().BeFalse();
        outputConfig.IncludePrerelease.Should().BeFalse();
    }

    [Fact]
    public void ExecutionConfig_DefaultValues_AreCorrect()
    {
        // Act
        var executionConfig = new ExecutionConfig();

        // Assert
        executionConfig.AutoRun.Should().BeFalse();
        executionConfig.RunDirectory.Should().Be(".");
    }

    [Fact]
    public void TemplateConfiguration_YamlSerialization_WorksCorrectly()
    {
        // Arrange
        var config = new TemplateConfiguration
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
                new() { Name = "uSync", Version = "17.0.0" },
                new() { Name = "Umbraco.Forms", Version = "latest" }
            }
        };

        // Act
        var yaml = _serializer.Serialize(config);
        var deserialized = _deserializer.Deserialize<TemplateConfiguration>(yaml);

        // Assert
        deserialized.Template.Name.Should().Be(config.Template.Name);
        deserialized.Template.Version.Should().Be(config.Template.Version);
        deserialized.Project.Name.Should().Be(config.Project.Name);
        deserialized.Packages.Should().HaveCount(2);
        deserialized.Packages[0].Name.Should().Be("uSync");
        deserialized.Packages[1].Version.Should().Be("latest");
    }

    [Fact]
    public void TemplateConfiguration_WithAllOptions_SerializesCorrectly()
    {
        // Arrange
        var config = new TemplateConfiguration
        {
            Template = new TemplateInfo { Name = "Umbraco.Templates", Version = "14.0.0" },
            Project = new ProjectInfo { Name = "MyApp", CreateSolution = true, SolutionName = "MyApp.sln" },
            Packages = new List<PackageConfig>
            {
                new() { Name = "uSync", Version = "17.0.0" }
            },
            StarterKit = new StarterKitConfig { Enabled = true, Package = "clean" },
            Docker = new DockerConfig { Dockerfile = true, DockerCompose = true },
            Unattended = new UnattendedConfig
            {
                Enabled = true,
                Database = new DatabaseConfig { Type = "SQLServer", ConnectionString = "Server=localhost;Database=Umbraco;" },
                Admin = new AdminConfig { Name = "Admin", Email = "admin@test.com", Password = "password123" }
            },
            Output = new OutputConfig { Oneliner = true, RemoveComments = true, IncludePrerelease = false },
            Execution = new ExecutionConfig { AutoRun = true, RunDirectory = "/app" }
        };

        // Act
        var yaml = _serializer.Serialize(config);
        var deserialized = _deserializer.Deserialize<TemplateConfiguration>(yaml);

        // Assert
        deserialized.Should().BeEquivalentTo(config);
    }

    [Fact]
    public void PackageConfig_WithDifferentVersions_SerializesCorrectly()
    {
        // Arrange
        var packages = new List<PackageConfig>
        {
            new() { Name = "uSync", Version = "17.0.0" },
            new() { Name = "Umbraco.Forms", Version = "latest" },
            new() { Name = "Umbraco.Deploy", Version = "prerelease" }
        };

        // Act
        var yaml = _serializer.Serialize(packages);
        var deserialized = _deserializer.Deserialize<List<PackageConfig>>(yaml);

        // Assert
        deserialized.Should().HaveCount(3);
        deserialized[0].Version.Should().Be("17.0.0");
        deserialized[1].Version.Should().Be("latest");
        deserialized[2].Version.Should().Be("prerelease");
    }

    [Fact]
    public void StarterKitConfig_WhenEnabled_HasPackage()
    {
        // Arrange
        var starterKit = new StarterKitConfig
        {
            Enabled = true,
            Package = "clean"
        };

        // Act
        var yaml = _serializer.Serialize(starterKit);
        var deserialized = _deserializer.Deserialize<StarterKitConfig>(yaml);

        // Assert
        deserialized.Enabled.Should().BeTrue();
        deserialized.Package.Should().Be("clean");
    }

    [Fact]
    public void DockerConfig_BothOptions_CanBeEnabled()
    {
        // Arrange
        var docker = new DockerConfig
        {
            Dockerfile = true,
            DockerCompose = true
        };

        // Act
        var yaml = _serializer.Serialize(docker);
        var deserialized = _deserializer.Deserialize<DockerConfig>(yaml);

        // Assert
        deserialized.Dockerfile.Should().BeTrue();
        deserialized.DockerCompose.Should().BeTrue();
    }

    [Fact]
    public void UnattendedConfig_WithSqlServer_SerializesCorrectly()
    {
        // Arrange
        var unattended = new UnattendedConfig
        {
            Enabled = true,
            Database = new DatabaseConfig
            {
                Type = "SqlServer",
                ConnectionString = "Server=localhost;Database=Umbraco;User Id=sa;Password=Pass123;"
            },
            Admin = new AdminConfig
            {
                Name = "Administrator",
                Email = "admin@umbraco.com",
                Password = "SecurePassword123!"
            }
        };

        // Act
        var yaml = _serializer.Serialize(unattended);
        var deserialized = _deserializer.Deserialize<UnattendedConfig>(yaml);

        // Assert
        deserialized.Enabled.Should().BeTrue();
        deserialized.Database.Type.Should().Be("SqlServer");
        deserialized.Database.ConnectionString.Should().Contain("Server=localhost");
        deserialized.Admin.Name.Should().Be("Administrator");
    }

    [Fact]
    public void AdminConfig_PasswordCanBePrompt()
    {
        // Arrange
        var admin = new AdminConfig
        {
            Name = "Admin",
            Email = "admin@test.com",
            Password = "<prompt>"
        };

        // Act
        var yaml = _serializer.Serialize(admin);
        var deserialized = _deserializer.Deserialize<AdminConfig>(yaml);

        // Assert
        deserialized.Password.Should().Be("<prompt>");
    }

    [Fact]
    public void AdminConfig_PasswordCanBeEnvironmentVariable()
    {
        // Arrange
        var admin = new AdminConfig
        {
            Name = "Admin",
            Email = "admin@test.com",
            Password = "${UMBRACO_ADMIN_PASSWORD}"
        };

        // Act
        var yaml = _serializer.Serialize(admin);
        var deserialized = _deserializer.Deserialize<AdminConfig>(yaml);

        // Assert
        deserialized.Password.Should().Be("${UMBRACO_ADMIN_PASSWORD}");
    }

    [Fact]
    public void OutputConfig_AllOptions_SerializeCorrectly()
    {
        // Arrange
        var output = new OutputConfig
        {
            Oneliner = true,
            RemoveComments = true,
            IncludePrerelease = true
        };

        // Act
        var yaml = _serializer.Serialize(output);
        var deserialized = _deserializer.Deserialize<OutputConfig>(yaml);

        // Assert
        deserialized.Oneliner.Should().BeTrue();
        deserialized.RemoveComments.Should().BeTrue();
        deserialized.IncludePrerelease.Should().BeTrue();
    }

    [Fact]
    public void ExecutionConfig_WithCustomRunDirectory_SerializesCorrectly()
    {
        // Arrange
        var execution = new ExecutionConfig
        {
            AutoRun = true,
            RunDirectory = "/home/user/projects/umbraco"
        };

        // Act
        var yaml = _serializer.Serialize(execution);
        var deserialized = _deserializer.Deserialize<ExecutionConfig>(yaml);

        // Assert
        deserialized.AutoRun.Should().BeTrue();
        deserialized.RunDirectory.Should().Be("/home/user/projects/umbraco");
    }

    [Fact]
    public void Template_WithMetadataAndConfiguration_SerializesCorrectly()
    {
        // Arrange
        var template = new Template
        {
            Metadata = new TemplateMetadata
            {
                Name = "production-template",
                Description = "Production deployment template",
                Author = "DevOps Team",
                Version = "2.1.0",
                Tags = new List<string> { "production", "umbraco-14", "docker" }
            },
            Configuration = new TemplateConfiguration
            {
                Template = new TemplateInfo { Name = "Umbraco.Templates", Version = "14.0.0" },
                Project = new ProjectInfo { Name = "ProdApp", CreateSolution = true }
            }
        };

        // Act
        var yaml = _serializer.Serialize(template);
        var deserialized = _deserializer.Deserialize<Template>(yaml);

        // Assert
        deserialized.Metadata.Name.Should().Be("production-template");
        deserialized.Metadata.Tags.Should().Contain("docker");
        deserialized.Configuration.Project.Name.Should().Be("ProdApp");
    }

    [Fact]
    public void TemplateMetadata_AllProperties_SerializeCorrectly()
    {
        // Arrange
        var metadata = new TemplateMetadata
        {
            Name = "test-template",
            Description = "Test template for unit testing",
            Author = "Test Author <test@example.com>",
            Created = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Modified = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Version = "1.5.2",
            Tags = new List<string> { "test", "development", "ci/cd" }
        };

        // Act
        var yaml = _serializer.Serialize(metadata);
        var deserialized = _deserializer.Deserialize<TemplateMetadata>(yaml);

        // Assert
        deserialized.Name.Should().Be(metadata.Name);
        deserialized.Description.Should().Be(metadata.Description);
        deserialized.Author.Should().Be(metadata.Author);
        deserialized.Version.Should().Be(metadata.Version);
        deserialized.Tags.Should().BeEquivalentTo(metadata.Tags);
        deserialized.Created.Should().BeCloseTo(metadata.Created, TimeSpan.FromSeconds(1));
        deserialized.Modified.Should().BeCloseTo(metadata.Modified, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TemplateMetadata_WithEmptyTags_SerializesCorrectly()
    {
        // Arrange
        var metadata = new TemplateMetadata
        {
            Name = "simple-template",
            Description = "Simple template",
            Tags = new List<string>()
        };

        // Act
        var yaml = _serializer.Serialize(metadata);
        var deserialized = _deserializer.Deserialize<TemplateMetadata>(yaml);

        // Assert
        deserialized.Tags.Should().BeEmpty();
    }

    [Fact]
    public void TemplateConfiguration_MinimalConfiguration_SerializesCorrectly()
    {
        // Arrange - Only set required fields
        var config = new TemplateConfiguration
        {
            Template = new TemplateInfo { Version = "14.0.0" },
            Project = new ProjectInfo { Name = "MinimalApp", CreateSolution = false }
        };

        // Act
        var yaml = _serializer.Serialize(config);
        var deserialized = _deserializer.Deserialize<TemplateConfiguration>(yaml);

        // Assert
        deserialized.Template.Version.Should().Be("14.0.0");
        deserialized.Project.Name.Should().Be("MinimalApp");
        deserialized.Project.CreateSolution.Should().BeFalse();
        deserialized.Packages.Should().BeEmpty();
    }
}
