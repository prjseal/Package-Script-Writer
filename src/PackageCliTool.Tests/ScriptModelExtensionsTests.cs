using FluentAssertions;
using PackageCliTool.Extensions;
using PackageCliTool.Models.Api;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for ScriptModelExtensions.ToViewModel()
/// </summary>
public class ScriptModelExtensionsTests
{
    [Fact]
    public void ToViewModel_WithSkipDotnetRunTrue_MapsCorrectly()
    {
        // Arrange
        var scriptModel = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            SkipDotnetRun = true
        };

        // Act
        var viewModel = scriptModel.ToViewModel();

        // Assert
        viewModel.SkipDotnetRun.Should().BeTrue();
    }

    [Fact]
    public void ToViewModel_WithSkipDotnetRunFalse_MapsCorrectly()
    {
        // Arrange
        var scriptModel = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject",
            SkipDotnetRun = false
        };

        // Act
        var viewModel = scriptModel.ToViewModel();

        // Assert
        viewModel.SkipDotnetRun.Should().BeFalse();
    }

    [Fact]
    public void ToViewModel_WithDefaultScriptModel_SkipDotnetRunIsFalse()
    {
        // Arrange
        var scriptModel = new ScriptModel
        {
            TemplateName = "Umbraco.Templates",
            ProjectName = "TestProject"
        };

        // Act
        var viewModel = scriptModel.ToViewModel();

        // Assert
        viewModel.SkipDotnetRun.Should().BeFalse();
    }

    [Fact]
    public void ToViewModel_MapsAllProperties()
    {
        // Arrange
        var scriptModel = new ScriptModel
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
            Packages = "uSync",
            IncludeStarterKit = true,
            StarterKitPackage = "clean",
            IncludeDockerfile = true,
            IncludeDockerCompose = true,
            EnableContentDeliveryApi = true,
            SkipDotnetRun = true
        };

        // Act
        var viewModel = scriptModel.ToViewModel();

        // Assert
        viewModel.TemplateName.Should().Be(scriptModel.TemplateName);
        viewModel.TemplateVersion.Should().Be(scriptModel.TemplateVersion);
        viewModel.CreateSolutionFile.Should().Be(scriptModel.CreateSolutionFile);
        viewModel.SolutionName.Should().Be(scriptModel.SolutionName);
        viewModel.ProjectName.Should().Be(scriptModel.ProjectName);
        viewModel.UseUnattendedInstall.Should().Be(scriptModel.UseUnattendedInstall);
        viewModel.DatabaseType.Should().Be(scriptModel.DatabaseType);
        viewModel.ConnectionString.Should().Be(scriptModel.ConnectionString);
        viewModel.UserFriendlyName.Should().Be(scriptModel.UserFriendlyName);
        viewModel.UserEmail.Should().Be(scriptModel.UserEmail);
        viewModel.UserPassword.Should().Be(scriptModel.UserPassword);
        viewModel.Packages.Should().Be(scriptModel.Packages);
        viewModel.IncludeStarterKit.Should().Be(scriptModel.IncludeStarterKit);
        viewModel.StarterKitPackage.Should().Be(scriptModel.StarterKitPackage);
        viewModel.IncludeDockerfile.Should().Be(scriptModel.IncludeDockerfile);
        viewModel.IncludeDockerCompose.Should().Be(scriptModel.IncludeDockerCompose);
        viewModel.EnableContentDeliveryApi.Should().Be(scriptModel.EnableContentDeliveryApi);
        viewModel.SkipDotnetRun.Should().Be(scriptModel.SkipDotnetRun);
    }
}
