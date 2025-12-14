using FluentAssertions;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.History;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for ScriptHistory model
/// </summary>
public class ScriptHistoryTests
{
    [Fact]
    public void ScriptHistory_Initialization_CreatesEmptyHistory()
    {
        // Act
        var history = new ScriptHistory();

        // Assert
        history.Version.Should().Be("1.0.0");
        history.Entries.Should().BeEmpty();
        history.MaxEntries.Should().Be(50);
    }

    [Fact]
    public void AddEntry_WithSingleEntry_AddsToBeginning()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry = CreateTestEntry("Project1");

        // Act
        history.AddEntry(entry);

        // Assert
        history.Entries.Should().HaveCount(1);
        history.Entries[0].Should().Be(entry);
    }

    [Fact]
    public void AddEntry_WithMultipleEntries_MaintainsNewestFirst()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");
        var entry3 = CreateTestEntry("Project3");

        // Act
        history.AddEntry(entry1);
        Thread.Sleep(10); // Ensure different timestamps
        history.AddEntry(entry2);
        Thread.Sleep(10);
        history.AddEntry(entry3);

        // Assert
        history.Entries.Should().HaveCount(3);
        history.Entries[0].Should().Be(entry3, "most recent entry should be first");
        history.Entries[1].Should().Be(entry2);
        history.Entries[2].Should().Be(entry1, "oldest entry should be last");
    }

    [Fact]
    public void AddEntry_WhenExceedingMaxEntries_RemovesOldest()
    {
        // Arrange
        var history = new ScriptHistory { MaxEntries = 3 };
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");
        var entry3 = CreateTestEntry("Project3");
        var entry4 = CreateTestEntry("Project4");

        // Act
        history.AddEntry(entry1);
        history.AddEntry(entry2);
        history.AddEntry(entry3);
        history.AddEntry(entry4);

        // Assert
        history.Entries.Should().HaveCount(3, "should not exceed MaxEntries");
        history.Entries[0].Should().Be(entry4, "newest entry should be first");
        history.Entries[1].Should().Be(entry3);
        history.Entries[2].Should().Be(entry2);
        history.Entries.Should().NotContain(entry1, "oldest entry should be removed");
    }

    [Fact]
    public void AddEntry_WhenAtMaxEntries_MaintainsExactCount()
    {
        // Arrange
        var history = new ScriptHistory { MaxEntries = 2 };

        // Act
        for (int i = 1; i <= 5; i++)
        {
            history.AddEntry(CreateTestEntry($"Project{i}"));
        }

        // Assert
        history.Entries.Should().HaveCount(2, "should maintain exactly MaxEntries");
    }

    [Fact]
    public void GetEntry_WithValidId_ReturnsCorrectEntry()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");
        history.AddEntry(entry1);
        history.AddEntry(entry2);

        // Act
        var found = history.GetEntry(entry1.Id);

        // Assert
        found.Should().NotBeNull();
        found.Should().Be(entry1);
    }

    [Fact]
    public void GetEntry_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var history = new ScriptHistory();
        history.AddEntry(CreateTestEntry("Project1"));

        // Act
        var found = history.GetEntry("non-existent-id");

        // Assert
        found.Should().BeNull();
    }

    [Fact]
    public void GetEntry_CaseInsensitiveId_FindsEntry()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry = CreateTestEntry("Project1");
        entry.Id = "ABC-123-DEF";
        history.AddEntry(entry);

        // Act
        var foundLower = history.GetEntry("abc-123-def");
        var foundUpper = history.GetEntry("ABC-123-DEF");
        var foundMixed = history.GetEntry("AbC-123-DeF");

        // Assert
        foundLower.Should().Be(entry, "should find with lowercase");
        foundUpper.Should().Be(entry, "should find with uppercase");
        foundMixed.Should().Be(entry, "should find with mixed case");
    }

    [Fact]
    public void GetEntryByIndex_WithValidIndex_ReturnsCorrectEntry()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");
        var entry3 = CreateTestEntry("Project3");
        history.AddEntry(entry1);
        history.AddEntry(entry2);
        history.AddEntry(entry3);

        // Act
        var first = history.GetEntryByIndex(1);
        var second = history.GetEntryByIndex(2);
        var third = history.GetEntryByIndex(3);

        // Assert
        first.Should().Be(entry3, "index 1 should be the most recent");
        second.Should().Be(entry2);
        third.Should().Be(entry1, "index 3 should be the oldest");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void GetEntryByIndex_WithZeroOrNegativeIndex_ReturnsNull(int index)
    {
        // Arrange
        var history = new ScriptHistory();
        history.AddEntry(CreateTestEntry("Project1"));

        // Act
        var found = history.GetEntryByIndex(index);

        // Assert
        found.Should().BeNull($"index {index} is invalid");
    }

    [Fact]
    public void GetEntryByIndex_WithIndexGreaterThanCount_ReturnsNull()
    {
        // Arrange
        var history = new ScriptHistory();
        history.AddEntry(CreateTestEntry("Project1"));
        history.AddEntry(CreateTestEntry("Project2"));

        // Act
        var found = history.GetEntryByIndex(10);

        // Assert
        found.Should().BeNull("index exceeds entry count");
    }

    [Fact]
    public void GetEntryByIndex_WithEmptyHistory_ReturnsNull()
    {
        // Arrange
        var history = new ScriptHistory();

        // Act
        var found = history.GetEntryByIndex(1);

        // Assert
        found.Should().BeNull("history is empty");
    }

    [Fact]
    public void RemoveEntry_WithValidId_RemovesEntryAndReturnsTrue()
    {
        // Arrange
        var history = new ScriptHistory();
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");
        history.AddEntry(entry1);
        history.AddEntry(entry2);

        // Act
        var result = history.RemoveEntry(entry1.Id);

        // Assert
        result.Should().BeTrue();
        history.Entries.Should().HaveCount(1);
        history.Entries.Should().NotContain(entry1);
        history.Entries.Should().Contain(entry2);
    }

    [Fact]
    public void RemoveEntry_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var history = new ScriptHistory();
        history.AddEntry(CreateTestEntry("Project1"));

        // Act
        var result = history.RemoveEntry("non-existent-id");

        // Assert
        result.Should().BeFalse();
        history.Entries.Should().HaveCount(1, "no entry should be removed");
    }

    [Fact]
    public void RemoveEntry_FromEmptyHistory_ReturnsFalse()
    {
        // Arrange
        var history = new ScriptHistory();

        // Act
        var result = history.RemoveEntry("any-id");

        // Assert
        result.Should().BeFalse();
        history.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        var history = new ScriptHistory();
        history.AddEntry(CreateTestEntry("Project1"));
        history.AddEntry(CreateTestEntry("Project2"));
        history.AddEntry(CreateTestEntry("Project3"));

        // Act
        history.Clear();

        // Assert
        history.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Clear_OnEmptyHistory_DoesNotThrow()
    {
        // Arrange
        var history = new ScriptHistory();

        // Act
        var act = () => history.Clear();

        // Assert
        act.Should().NotThrow();
        history.Entries.Should().BeEmpty();
    }

    [Fact]
    public void ScriptHistory_WithCustomMaxEntries_RespectsLimit()
    {
        // Arrange
        var history = new ScriptHistory { MaxEntries = 5 };

        // Act
        for (int i = 1; i <= 10; i++)
        {
            history.AddEntry(CreateTestEntry($"Project{i}"));
        }

        // Assert
        history.Entries.Should().HaveCount(5);
        history.Entries[0].ScriptModel.ProjectName.Should().Be("Project10");
        history.Entries[4].ScriptModel.ProjectName.Should().Be("Project6");
    }

    [Fact]
    public void AddEntry_WithMaxEntriesSetToOne_KeepsOnlyLatest()
    {
        // Arrange
        var history = new ScriptHistory { MaxEntries = 1 };
        var entry1 = CreateTestEntry("Project1");
        var entry2 = CreateTestEntry("Project2");

        // Act
        history.AddEntry(entry1);
        history.AddEntry(entry2);

        // Assert
        history.Entries.Should().HaveCount(1);
        history.Entries[0].Should().Be(entry2);
    }

    /// <summary>
    /// Helper method to create a test history entry
    /// </summary>
    private static HistoryEntry CreateTestEntry(string projectName)
    {
        return new HistoryEntry
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            ScriptModel = new ScriptModel
            {
                ProjectName = projectName,
                TemplateName = "Umbraco.Templates",
                TemplateVersion = "14.0.0"
            }
        };
    }
}
