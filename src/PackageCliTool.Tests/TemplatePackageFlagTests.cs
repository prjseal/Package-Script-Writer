using FluentAssertions;
using PackageCliTool.Models;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Specific tests for the --template-package flag functionality
/// </summary>
public class TemplatePackageFlagTests
{
    [Fact]
    public void TemplatePackageFlag_WhenNotProvided_ShouldBeNull()
    {
        // Arrange
        var args = new[] { "-n", "MyProject" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().BeNull("template package should be optional");
    }

    [Fact]
    public void TemplatePackageFlag_WithUmbracoTemplates_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Templates" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
    }

    [Fact]
    public void TemplatePackageFlag_WithCommunityTemplate_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Community.Templates.Clean" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.Clean");
    }

    [Fact]
    public void TemplatePackageFlag_WithUmBootstrapTemplate_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Community.Templates.UmBootstrap" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.UmBootstrap");
    }

    [Fact]
    public void TemplatePackageFlag_WithVersionFlag_BothShouldBeParsed()
    {
        // Arrange
        var args = new[]
        {
            "--template-package", "Umbraco.Templates",
            "-t", "14.3.0"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.TemplateVersion.Should().Be("14.3.0");
    }

    [Fact]
    public void TemplatePackageFlag_WithoutVersion_VersionShouldBeNull()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Templates" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.TemplateVersion.Should().BeNull();
    }

    [Fact]
    public void TemplateVersionFlag_WithoutTemplatePackage_ShouldStillBeParsed()
    {
        // Arrange
        var args = new[] { "-t", "14.3.0" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().BeNull();
        options.TemplateVersion.Should().Be("14.3.0");
    }

    [Fact]
    public void TemplatePackageFlag_InComplexCommand_ShouldParseWithOtherFlags()
    {
        // Arrange
        var args = new[]
        {
            "--template-package", "Umbraco.Community.Templates.Clean",
            "-t", "14.3.0",
            "-p", "uSync|17.0.0,Umbraco.Forms",
            "-n", "MyCleanProject",
            "-s", "MyCleanSolution",
            "-u",
            "--database-type", "SQLite"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.Clean");
        options.TemplateVersion.Should().Be("14.3.0");
        options.Packages.Should().Be("uSync|17.0.0,Umbraco.Forms");
        options.ProjectName.Should().Be("MyCleanProject");
        options.CreateSolution.Should().BeTrue();
        options.SolutionName.Should().Be("MyCleanSolution");
        options.UseUnattended.Should().BeTrue();
        options.DatabaseType.Should().Be("SQLite");
    }

    [Theory]
    [InlineData("Umbraco.Templates")]
    [InlineData("Umbraco.Community.Templates.Clean")]
    [InlineData("Umbraco.Community.Templates.UmBootstrap")]
    [InlineData("CustomTemplate.Package")]
    public void TemplatePackageFlag_WithVariousTemplateNames_ShouldParseCorrectly(string templateName)
    {
        // Arrange
        var args = new[] { "--template-package", templateName };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be(templateName);
    }

    [Fact]
    public void TemplatePackageFlag_WithSpacesInValue_ShouldPreserveValue()
    {
        // Arrange - This shouldn't happen normally, but testing edge case
        var args = new[] { "--template-package", "Template With Spaces" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Template With Spaces");
    }

    [Fact]
    public void TemplatePackageFlag_IncludedInHasAnyOptions_WhenSet()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Templates" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.HasAnyOptions().Should().BeTrue("--template-package should be considered a configuration option");
    }

    [Fact]
    public void ScriptGeneration_WithoutTemplatePackage_ShouldSkipTemplateInstallation()
    {
        // Arrange - Simulating a script without template
        var args = new[]
        {
            "-p", "uSync|17.0.0",
            "-n", "MyProject"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().BeNull("should allow scripts without template installation");
        options.Packages.Should().NotBeNullOrEmpty("should still have packages");
        options.ProjectName.Should().Be("MyProject");
    }

    [Fact]
    public void ScriptGeneration_WithTemplatePackageOnly_ShouldWork()
    {
        // Arrange - Only template, no packages
        var args = new[]
        {
            "--template-package", "Umbraco.Templates",
            "-n", "MyProject"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.Packages.Should().BeNull();
        options.ProjectName.Should().Be("MyProject");
    }

    [Fact]
    public void TemplatePackageFlag_WithDefaultFlag_ShouldBothWork()
    {
        // Arrange
        var args = new[]
        {
            "--default",
            "--template-package", "Umbraco.Community.Templates.Clean"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.UseDefault.Should().BeTrue();
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.Clean",
            "explicit template package should override default");
    }

    [Fact]
    public void TemplatePackageFlag_OrderDoesNotMatter()
    {
        // Arrange - Template package at the end
        var args1 = new[]
        {
            "-n", "MyProject",
            "-p", "uSync",
            "--template-package", "Umbraco.Templates"
        };

        // Arrange - Template package at the beginning
        var args2 = new[]
        {
            "--template-package", "Umbraco.Templates",
            "-n", "MyProject",
            "-p", "uSync"
        };

        // Act
        var options1 = CommandLineOptions.Parse(args1);
        var options2 = CommandLineOptions.Parse(args2);

        // Assert
        options1.TemplatePackageName.Should().Be(options2.TemplatePackageName);
        options1.ProjectName.Should().Be(options2.ProjectName);
        options1.Packages.Should().Be(options2.Packages);
    }
}
