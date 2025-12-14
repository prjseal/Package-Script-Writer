using FluentAssertions;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.History;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for HistoryEntry model
/// </summary>
public class HistoryEntryTests
{
    [Fact]
    public void HistoryEntry_Initialization_GeneratesIdAndTimestamp()
    {
        // Act
        var entry = new HistoryEntry();

        // Assert
        entry.Id.Should().NotBeNullOrWhiteSpace("ID should be auto-generated");
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entry.ScriptModel.Should().NotBeNull();
        entry.Tags.Should().NotBeNull().And.BeEmpty();
        entry.WasExecuted.Should().BeFalse();
        entry.ExitCode.Should().BeNull();
    }

    [Fact]
    public void GetDisplayName_WithDescription_ReturnsDescription()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            Description = "My custom description",
            TemplateName = "SomeTemplate",
            ScriptModel = new ScriptModel { ProjectName = "MyProject" }
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("My custom description",
            "description takes precedence over template name and project name");
    }

    [Fact]
    public void GetDisplayName_WithTemplateNameOnly_ReturnsTemplateFormat()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = "Umbraco.Community.Templates.Clean",
            ScriptModel = new ScriptModel { ProjectName = "MyProject" }
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("From template: Umbraco.Community.Templates.Clean",
            "template name should be used when no description is provided");
    }

    [Fact]
    public void GetDisplayName_WithEmptyDescription_UsesTemplateName()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            Description = "",
            TemplateName = "Umbraco.Templates",
            ScriptModel = new ScriptModel { ProjectName = "MyProject" }
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("From template: Umbraco.Templates",
            "empty description should be treated as null");
    }

    [Fact]
    public void GetDisplayName_WithWhitespaceDescription_UsesTemplateName()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            Description = "   ",
            TemplateName = "Umbraco.Templates",
            ScriptModel = new ScriptModel { ProjectName = "MyProject" }
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("From template: Umbraco.Templates",
            "whitespace-only description should be treated as null");
    }

    [Fact]
    public void GetDisplayName_WithProjectNameOnly_ReturnsProjectAndTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = null,
            ScriptModel = new ScriptModel { ProjectName = "MyUmbracoProject" },
            Timestamp = timestamp
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("MyUmbracoProject - 2024-01-15 10:30",
            "should use project name and formatted timestamp when no description or template");
    }

    [Fact]
    public void GetDisplayName_WithNullProjectName_ReturnsDefaultWithTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2024, 3, 20, 14, 45, 30, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = null,
            ScriptModel = new ScriptModel { ProjectName = null! },
            Timestamp = timestamp
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("Script - 2024-03-20 14:45",
            "should use 'Script' as fallback when project name is null");
    }

    [Fact]
    public void GetDisplayName_WithEmptyProjectName_ReturnsDefaultWithTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2024, 6, 1, 8, 15, 0, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = null,
            ScriptModel = new ScriptModel { ProjectName = "" },
            Timestamp = timestamp
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("Script - 2024-06-01 08:15",
            "should use 'Script' as fallback when project name is empty");
    }

    [Fact]
    public void GetDisplayName_WithWhitespaceProjectName_ReturnsDefaultWithTimestamp()
    {
        // Arrange
        var timestamp = new DateTime(2024, 7, 4, 16, 30, 0, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = null,
            ScriptModel = new ScriptModel { ProjectName = "   " },
            Timestamp = timestamp
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("Script - 2024-07-04 16:30",
            "should use 'Script' as fallback when project name is whitespace-only");
    }

    [Fact]
    public void GetDisplayName_WithEmptyTemplateName_SkipsToProjectName()
    {
        // Arrange
        var timestamp = new DateTime(2024, 12, 25, 12, 0, 0, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Description = null,
            TemplateName = "",
            ScriptModel = new ScriptModel { ProjectName = "ChristmasProject" },
            Timestamp = timestamp
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Be("ChristmasProject - 2024-12-25 12:00",
            "empty template name should be treated as null");
    }

    [Fact]
    public void GetDisplayName_PriorityOrder_DescriptionOverTemplateOverProject()
    {
        // Arrange - All three are set
        var entry = new HistoryEntry
        {
            Description = "Custom Description",
            TemplateName = "Some Template",
            ScriptModel = new ScriptModel { ProjectName = "Some Project" }
        };

        // Act & Assert - Description wins
        entry.GetDisplayName().Should().Be("Custom Description");

        // Remove description - Template should win
        entry.Description = null;
        entry.GetDisplayName().Should().Be("From template: Some Template");

        // Remove template - Project should win
        entry.TemplateName = null;
        entry.GetDisplayName().Should().Contain("Some Project");
    }

    [Fact]
    public void HistoryEntry_WithExecutionInfo_StoresCorrectly()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            WasExecuted = true,
            ExecutionDirectory = "/home/user/projects",
            ExitCode = 0
        };

        // Act & Assert
        entry.WasExecuted.Should().BeTrue();
        entry.ExecutionDirectory.Should().Be("/home/user/projects");
        entry.ExitCode.Should().Be(0);
    }

    [Fact]
    public void HistoryEntry_WithFailedExecution_StoresNonZeroExitCode()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            WasExecuted = true,
            ExecutionDirectory = "/home/user/projects",
            ExitCode = 1
        };

        // Act & Assert
        entry.WasExecuted.Should().BeTrue();
        entry.ExitCode.Should().Be(1, "failed execution should have non-zero exit code");
    }

    [Fact]
    public void HistoryEntry_WithTags_StoresAndRetrievesCorrectly()
    {
        // Arrange
        var entry = new HistoryEntry
        {
            Tags = new List<string> { "production", "umbraco-14", "clean-template" }
        };

        // Act & Assert
        entry.Tags.Should().HaveCount(3);
        entry.Tags.Should().Contain("production");
        entry.Tags.Should().Contain("umbraco-14");
        entry.Tags.Should().Contain("clean-template");
    }

    [Fact]
    public void HistoryEntry_GeneratesUniqueIds()
    {
        // Arrange & Act
        var entry1 = new HistoryEntry();
        var entry2 = new HistoryEntry();
        var entry3 = new HistoryEntry();

        // Assert
        var ids = new[] { entry1.Id, entry2.Id, entry3.Id };
        ids.Should().OnlyHaveUniqueItems("each entry should have a unique ID");
        ids.Should().AllSatisfy(id => Guid.TryParse(id, out _).Should().BeTrue("ID should be a valid GUID"));
    }

    [Fact]
    public void HistoryEntry_TimestampFormat_IsCorrect()
    {
        // Arrange
        var expectedTime = new DateTime(2024, 11, 5, 16, 22, 33, DateTimeKind.Utc);
        var entry = new HistoryEntry
        {
            Timestamp = expectedTime,
            ScriptModel = new ScriptModel { ProjectName = "Test" }
        };

        // Act
        var displayName = entry.GetDisplayName();

        // Assert
        displayName.Should().Contain("2024-11-05 16:22",
            "timestamp should be formatted as yyyy-MM-dd HH:mm");
    }
}
