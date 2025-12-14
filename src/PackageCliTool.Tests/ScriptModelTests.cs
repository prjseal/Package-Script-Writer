using System.Text.Json;
using FluentAssertions;
using PackageCliTool.Models.Api;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for ScriptModel JSON serialization
/// </summary>
public class ScriptModelTests
{
    [Fact]
    public void ScriptModel_Serialization_UsesJsonPropertyNames()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "14.0.0",
            ProjectName = "TestProject",
            CreateSolutionFile = true,
            SolutionName = "TestSolution"
        };

        // Act
        var json = JsonSerializer.Serialize(model);

        // Assert
        json.Should().Contain("\"templateName\":", "should use camelCase JSON property name");
        json.Should().Contain("\"templateVersion\":");
        json.Should().Contain("\"projectName\":");
        json.Should().Contain("\"createSolutionFile\":");
        json.Should().Contain("\"solutionName\":");
    }

    [Fact]
    public void ScriptModel_Deserialization_MapsJsonPropertiesCorrectly()
    {
        // Arrange
        var json = @"{
            ""templateName"": ""Umbraco.Templates"",
            ""templateVersion"": ""14.0.0"",
            ""projectName"": ""TestProject"",
            ""createSolutionFile"": true,
            ""solutionName"": ""TestSolution""
        }";

        // Act
        var model = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        model.Should().NotBeNull();
        model!.TemplateName.Should().Be("Umbraco.Templates");
        model.TemplateVersion.Should().Be("14.0.0");
        model.ProjectName.Should().Be("TestProject");
        model.CreateSolutionFile.Should().BeTrue();
        model.SolutionName.Should().Be("TestSolution");
    }

    [Fact]
    public void ScriptModel_Serialization_IncludesAllProperties()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "14.0.0",
            ProjectName = "MyProject",
            CreateSolutionFile = true,
            SolutionName = "MySolution",
            UseUnattendedInstall = true,
            DatabaseType = "SQLite",
            ConnectionString = "Data Source=umbraco.db",
            UserFriendlyName = "Admin User",
            UserEmail = "admin@test.com",
            UserPassword = "SecurePassword123",
            Packages = "uSync|17.0.0,Umbraco.Forms",
            IncludeStarterKit = true,
            StarterKitPackage = "clean",
            CanIncludeDocker = true,
            IncludeDockerfile = true,
            IncludeDockerCompose = true,
            OnelinerOutput = true,
            RemoveComments = true
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.TemplateName.Should().Be(model.TemplateName);
        deserialized.TemplateVersion.Should().Be(model.TemplateVersion);
        deserialized.ProjectName.Should().Be(model.ProjectName);
        deserialized.CreateSolutionFile.Should().Be(model.CreateSolutionFile);
        deserialized.SolutionName.Should().Be(model.SolutionName);
        deserialized.UseUnattendedInstall.Should().Be(model.UseUnattendedInstall);
        deserialized.DatabaseType.Should().Be(model.DatabaseType);
        deserialized.ConnectionString.Should().Be(model.ConnectionString);
        deserialized.UserFriendlyName.Should().Be(model.UserFriendlyName);
        deserialized.UserEmail.Should().Be(model.UserEmail);
        deserialized.UserPassword.Should().Be(model.UserPassword);
        deserialized.Packages.Should().Be(model.Packages);
        deserialized.IncludeStarterKit.Should().Be(model.IncludeStarterKit);
        deserialized.StarterKitPackage.Should().Be(model.StarterKitPackage);
        deserialized.CanIncludeDocker.Should().Be(model.CanIncludeDocker);
        deserialized.IncludeDockerfile.Should().Be(model.IncludeDockerfile);
        deserialized.IncludeDockerCompose.Should().Be(model.IncludeDockerCompose);
        deserialized.OnelinerOutput.Should().Be(model.OnelinerOutput);
        deserialized.RemoveComments.Should().Be(model.RemoveComments);
    }

    [Fact]
    public void ScriptModel_Deserialization_WithMissingOptionalFields_UsesDefaults()
    {
        // Arrange - Minimal JSON
        var json = @"{
            ""templateName"": ""Umbraco.Templates"",
            ""projectName"": ""MyProject""
        }";

        // Act
        var model = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        model.Should().NotBeNull();
        model!.TemplateName.Should().Be("Umbraco.Templates");
        model.ProjectName.Should().Be("MyProject");
        model.TemplateVersion.Should().Be(string.Empty, "default value for string");
        model.CreateSolutionFile.Should().BeFalse("default value for bool");
        model.SolutionName.Should().BeNull("optional string should be null");
    }

    [Fact]
    public void ScriptModel_WithUnattendedInstall_SerializesCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            UseUnattendedInstall = true,
            DatabaseType = "SqlServer",
            ConnectionString = "Server=localhost;Database=Umbraco;",
            UserFriendlyName = "Administrator",
            UserEmail = "admin@umbraco.com",
            UserPassword = "Admin1234!"
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.UseUnattendedInstall.Should().BeTrue();
        deserialized.DatabaseType.Should().Be("SqlServer");
        deserialized.ConnectionString.Should().Be("Server=localhost;Database=Umbraco;");
        deserialized.UserFriendlyName.Should().Be("Administrator");
        deserialized.UserEmail.Should().Be("admin@umbraco.com");
        deserialized.UserPassword.Should().Be("Admin1234!");
    }

    [Fact]
    public void ScriptModel_WithPackages_SerializesCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            Packages = "uSync|17.0.0,Umbraco.Forms|14.2.0,Clean --prerelease"
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.Packages.Should().Be(model.Packages);
    }

    [Fact]
    public void ScriptModel_WithStarterKit_SerializesCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            IncludeStarterKit = true,
            StarterKitPackage = "clean"
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.IncludeStarterKit.Should().BeTrue();
        deserialized.StarterKitPackage.Should().Be("clean");
    }

    [Fact]
    public void ScriptModel_WithDockerOptions_SerializesCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            CanIncludeDocker = true,
            IncludeDockerfile = true,
            IncludeDockerCompose = true
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.CanIncludeDocker.Should().BeTrue();
        deserialized.IncludeDockerfile.Should().BeTrue();
        deserialized.IncludeDockerCompose.Should().BeTrue();
    }

    [Fact]
    public void ScriptModel_WithOutputOptions_SerializesCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            OnelinerOutput = true,
            RemoveComments = true
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.OnelinerOutput.Should().BeTrue();
        deserialized.RemoveComments.Should().BeTrue();
    }

    [Fact]
    public void ScriptModel_RoundTrip_PreservesAllData()
    {
        // Arrange - Create model with all properties set
        var original = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "14.0.0",
            CreateSolutionFile = true,
            SolutionName = "MySolution",
            ProjectName = "MyProject",
            UseUnattendedInstall = true,
            DatabaseType = "SQLite",
            ConnectionString = "Data Source=umbraco.db",
            UserFriendlyName = "Admin",
            UserEmail = "admin@test.com",
            UserPassword = "password123",
            Packages = "uSync,Umbraco.Forms|14.0.0",
            IncludeStarterKit = true,
            StarterKitPackage = "clean",
            CanIncludeDocker = true,
            IncludeDockerfile = true,
            IncludeDockerCompose = true,
            OnelinerOutput = false,
            RemoveComments = true
        };

        // Act - Serialize and deserialize
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert - All properties should match
        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void ScriptModel_Deserialization_IsCaseInsensitive()
    {
        // Arrange - Use PascalCase property names (shouldn't match JsonPropertyName)
        var json = @"{
            ""TemplateName"": ""Umbraco.Templates"",
            ""ProjectName"": ""TestProject""
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Act
        var model = JsonSerializer.Deserialize<ScriptModel>(json, options);

        // Assert - Should still work with case-insensitive option
        model.Should().NotBeNull();
        model!.TemplateName.Should().Be("Umbraco.Templates");
        model.ProjectName.Should().Be("TestProject");
    }

    [Fact]
    public void ScriptModel_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var model = new ScriptModel();

        // Assert
        model.TemplateName.Should().Be(string.Empty);
        model.TemplateVersion.Should().Be(string.Empty);
        model.ProjectName.Should().Be(string.Empty);
        model.CreateSolutionFile.Should().BeFalse();
        model.SolutionName.Should().BeNull();
        model.UseUnattendedInstall.Should().BeFalse();
        model.DatabaseType.Should().BeNull();
        model.ConnectionString.Should().BeNull();
        model.UserFriendlyName.Should().BeNull();
        model.UserEmail.Should().BeNull();
        model.UserPassword.Should().BeNull();
        model.Packages.Should().BeNull();
        model.IncludeStarterKit.Should().BeFalse();
        model.StarterKitPackage.Should().BeNull();
        model.CanIncludeDocker.Should().BeFalse();
        model.IncludeDockerfile.Should().BeFalse();
        model.IncludeDockerCompose.Should().BeFalse();
        model.OnelinerOutput.Should().BeFalse();
        model.RemoveComments.Should().BeFalse();
    }

    [Fact]
    public void ScriptModel_NullValues_SerializeCorrectly()
    {
        // Arrange
        var model = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            SolutionName = null,
            Packages = null,
            StarterKitPackage = null
        };

        // Act
        var json = JsonSerializer.Serialize(model);
        var deserialized = JsonSerializer.Deserialize<ScriptModel>(json);

        // Assert
        deserialized!.SolutionName.Should().BeNull();
        deserialized.Packages.Should().BeNull();
        deserialized.StarterKitPackage.Should().BeNull();
    }
}
