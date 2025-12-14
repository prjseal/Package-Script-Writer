using FluentAssertions;
using PackageCliTool.Models;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for CommandLineOptions parsing
/// </summary>
public class CommandLineOptionsTests
{
    [Fact]
    public void Parse_WithNoArguments_ReturnsEmptyOptions()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.Should().NotBeNull();
        options.ShowHelp.Should().BeFalse();
        options.ShowVersion.Should().BeFalse();
        options.UseDefault.Should().BeFalse();
        options.HasAnyOptions().Should().BeFalse();
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public void Parse_WithHelpFlag_SetsShowHelp(string helpFlag)
    {
        // Arrange
        var args = new[] { helpFlag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.ShowHelp.Should().BeTrue();
    }

    [Theory]
    [InlineData("-v")]
    [InlineData("--version")]
    public void Parse_WithVersionFlag_SetsShowVersion(string versionFlag)
    {
        // Arrange
        var args = new[] { versionFlag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.ShowVersion.Should().BeTrue();
    }

    [Theory]
    [InlineData("-d")]
    [InlineData("--default")]
    public void Parse_WithDefaultFlag_SetsUseDefault(string defaultFlag)
    {
        // Arrange
        var args = new[] { defaultFlag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.UseDefault.Should().BeTrue();
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void Parse_WithTemplatePackageFlag_SetsTemplatePackageName()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Templates" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void Parse_WithCommunityTemplatePackage_SetsCorrectValue()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Community.Templates.Clean" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.Clean");
    }

    [Fact]
    public void Parse_WithoutTemplatePackageFlag_LeavesTemplatePackageNameNull()
    {
        // Arrange
        var args = new[] { "-n", "MyProject" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().BeNull();
    }

    [Theory]
    [InlineData("-t", "Umbraco.Templates|17.0.3")]
    public void Parse_WithTemplateVersionFlag_SetsTemplateVersion(string flag, string version)
    {
        // Arrange
        var args = new[] { flag, version };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplateVersion.Should().Be("17.0.3");
    }

    [Theory]
    [InlineData("-p", "uSync,Umbraco.Forms")]
    [InlineData("--packages", "uSync,Umbraco.Forms")]
    public void Parse_WithPackagesFlag_SetsPackages(string flag, string packages)
    {
        // Arrange
        var args = new[] { flag, packages };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.Packages.Should().Be(packages);
    }

    [Fact]
    public void Parse_WithPackagesAndVersions_ParsesCorrectly()
    {
        // Arrange
        var args = new[] { "-p", "uSync|17.0.0,clean|7.0.1" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.Packages.Should().Be("uSync|17.0.0,clean|7.0.1");
    }

    [Theory]
    [InlineData("-n", "MyProject")]
    [InlineData("--project-name", "MyProject")]
    public void Parse_WithProjectNameFlag_SetsProjectName(string flag, string projectName)
    {
        // Arrange
        var args = new[] { flag, projectName };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.ProjectName.Should().Be(projectName);
    }

    [Fact]
    public void Parse_WithSolutionNameFlag_SetsSolutionName()
    {
        // Arrange
        var args = new[] { "-s", "MySolution" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.SolutionName.Should().Be("MySolution");
    }

    [Fact]
    public void Parse_WithStarterKitFlag_SetsStarterKitPackage()
    {
        // Arrange
        var args = new[] { "-k", "clean" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.IncludeStarterKit.Should().BeTrue();
        options.StarterKitPackage.Should().Be("clean");
    }

    [Theory]
    [InlineData("-u")]
    [InlineData("--unattended-defaults")]
    public void Parse_WithUnattendedFlag_SetsUseUnattended(string flag)
    {
        // Arrange
        var args = new[] { flag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.UseUnattended.Should().BeTrue();
        options.DatabaseType.Should().Be("SQLite");
        options.AdminEmail.Should().Be("admin@example.com"); 
        options.AdminPassword.Should().Be("1234567890");
    }

    [Fact]
    public void Parse_WithDatabaseTypeFlag_SetsDatabaseType()
    {
        // Arrange
        var args = new[] { "--database-type", "SQLite" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.DatabaseType.Should().Be("SQLite");
    }

    [Fact]
    public void Parse_WithAdminEmailFlag_SetsAdminEmail()
    {
        // Arrange
        var args = new[] { "--admin-email", "admin@test.com" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.AdminEmail.Should().Be("admin@test.com");
    }

    [Fact]
    public void Parse_WithAdminPasswordFlag_SetsAdminPassword()
    {
        // Arrange
        var args = new[] { "--admin-password", "SecurePass123!" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.AdminPassword.Should().Be("SecurePass123!");
    }

    [Theory]
    [InlineData("-o")]
    [InlineData("--oneliner")]
    public void Parse_WithOnelinerFlag_SetsOnelinerOutput(string flag)
    {
        // Arrange
        var args = new[] { flag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.OnelinerOutput.Should().BeTrue();
    }

    [Theory]
    [InlineData("-r")]
    [InlineData("--remove-comments")]
    public void Parse_WithRemoveCommentsFlag_SetsRemoveComments(string flag)
    {
        // Arrange
        var args = new[] { flag };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.RemoveComments.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithAutoRunFlag_SetsAutoRun()
    {
        // Arrange
        var args = new[] { "--auto-run" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.AutoRun.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithRunDirectoryFlag_SetsRunDirectory()
    {
        // Arrange
        var args = new[] { "--run-dir", "/path/to/dir" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.RunDirectory.Should().Be("/path/to/dir");
    }

    [Fact]
    public void Parse_WithVerboseFlag_SetsVerbose()
    {
        // Arrange
        var args = new[] { "--verbose" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.VerboseMode.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithMultipleFlags_ParsesAllCorrectly()
    {
        // Arrange
        var args = new[]
        {
            "--template-package", "Umbraco.Templates",
            "-t", "14.3.0",
            "-p", "uSync|17.0.0,Umbraco.Forms|14.2.0",
            "-n", "MyProject",
            "-s", "MySolution",
            "-u",
            "--database-type", "SQLite",
            "--admin-email", "admin@test.com"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.TemplateVersion.Should().Be("14.3.0");
        options.Packages.Should().Be("uSync|17.0.0,Umbraco.Forms|14.2.0");
        options.ProjectName.Should().Be("MyProject");
        options.CreateSolution.Should().BeTrue();
        options.SolutionName.Should().Be("MySolution");
        options.UseUnattended.Should().BeTrue();
        options.DatabaseType.Should().Be("SQLite");
        options.AdminEmail.Should().Be("admin@test.com");
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void Parse_WithTemplatePackageAndNoVersion_AllowsBothToBeSet()
    {
        // Arrange
        var args = new[]
        {
            "--template-package", "Umbraco.Community.Templates.Clean",
            "-n", "MyProject"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().Be("Umbraco.Community.Templates.Clean");
        options.TemplateVersion.Should().BeNull();
        options.ProjectName.Should().Be("MyProject");
    }

    [Fact]
    public void Parse_WithOnlyPackagesNoTemplate_WorksCorrectly()
    {
        // Arrange
        var args = new[]
        {
            "-p", "uSync|17.0.0",
            "-n", "MyProject"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.TemplatePackageName.Should().BeNull();
        options.Packages.Should().Be("uSync|17.0.0");
        options.ProjectName.Should().Be("MyProject");
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void HasAnyOptions_WithOnlyHelpFlag_ReturnsFalse()
    {
        // Arrange
        var args = new[] { "-h" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.HasAnyOptions().Should().BeFalse();
    }

    [Fact]
    public void HasAnyOptions_WithTemplatePackageFlag_ReturnsTrue()
    {
        // Arrange
        var args = new[] { "--template-package", "Umbraco.Templates" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void HasAnyOptions_WithProjectNameOnly_ReturnsTrue()
    {
        // Arrange
        var args = new[] { "-n", "MyProject" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.HasAnyOptions().Should().BeTrue();
    }

    [Fact]
    public void Parse_WithDockerFlags_SetsDockerOptions()
    {
        // Arrange
        var args = new[] { "--dockerfile", "--docker-compose" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.IncludeDockerfile.Should().BeTrue();
        options.IncludeDockerCompose.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithClearCacheFlag_SetsClearCache()
    {
        // Arrange
        var args = new[] { "--clear-cache" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.ClearCache.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithIncludePrereleaseFlag_SetsIncludePrerelease()
    {
        // Arrange
        var args = new[] { "--include-prerelease" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.IncludePrerelease.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithStarterKitPipeSeparatedVersion_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[] { "-k", "clean|7.0.3" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.StarterKitPackage.Should().Be("clean");
        options.StarterKitVersion.Should().Be("7.0.3");
        options.IncludeStarterKit.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithStarterKitPipeSeparatedVersion_UsingLongForm_ShouldParseCorrectly()
    {
        // Arrange
        var args = new[] { "--starter-kit", "clean|7.0.3" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.StarterKitPackage.Should().Be("clean");
        options.StarterKitVersion.Should().Be("7.0.3");
        options.IncludeStarterKit.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithStarterKitPipeSeparatedVersion_InComplexCommand_ShouldParseWithOtherFlags()
    {
        // Arrange
        var args = new[]
        {
            "-k", "clean|7.0.3",
            "-t", "Umbraco.Templates|17.0.2",
            "-p", "uSync|17.0.0",
            "-n", "MyProject"
        };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.StarterKitPackage.Should().Be("clean");
        options.StarterKitVersion.Should().Be("7.0.3");
        options.IncludeStarterKit.Should().BeTrue();
        options.TemplatePackageName.Should().Be("Umbraco.Templates");
        options.TemplateVersion.Should().Be("17.0.2");
        options.Packages.Should().Be("uSync|17.0.0");
        options.ProjectName.Should().Be("MyProject");
    }

    [Fact]
    public void Parse_WithStarterKitWithoutVersion_ShouldOnlySetPackageName()
    {
        // Arrange
        var args = new[] { "-k", "clean" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.StarterKitPackage.Should().Be("clean");
        options.StarterKitVersion.Should().BeNull();
        options.IncludeStarterKit.Should().BeTrue();
    }

    [Fact]
    public void Parse_WithStarterKitInvalidPipeFormat_ShouldSetAsPackageName()
    {
        // Arrange - Empty version after pipe
        var args = new[] { "-k", "clean|" };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert - Should treat the whole thing as package name since format is invalid
        options.StarterKitPackage.Should().Be("clean|");
        options.StarterKitVersion.Should().BeNull();
        options.IncludeStarterKit.Should().BeTrue();
    }

    [Theory]
    [InlineData("clean|7.0.3", "clean", "7.0.3")]
    [InlineData("minimal|8.0.0", "minimal", "8.0.0")]
    [InlineData("CustomStarterKit|1.0.0", "CustomStarterKit", "1.0.0")]
    public void Parse_WithVariousStarterKitPipeSeparatedVersions_ShouldParseCorrectly(
        string starterKitArg, string expectedName, string expectedVersion)
    {
        // Arrange
        var args = new[] { "-k", starterKitArg };

        // Act
        var options = CommandLineOptions.Parse(args);

        // Assert
        options.StarterKitPackage.Should().Be(expectedName);
        options.StarterKitVersion.Should().Be(expectedVersion);
        options.IncludeStarterKit.Should().BeTrue();
    }
}
