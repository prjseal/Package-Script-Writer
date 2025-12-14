using FluentAssertions;
using PackageCliTool.Models.Api;
using PackageCliTool.Models.History;
using PackageCliTool.Services;
using Xunit;

namespace PackageCliTool.Tests;

/// <summary>
/// Unit tests for HistoryService
/// </summary>
public class HistoryServiceTests : IDisposable
{
    private readonly string _testHistoryDirectory;

    public HistoryServiceTests()
    {
        // Create a unique temporary directory for each test run
        _testHistoryDirectory = Path.Combine(Path.GetTempPath(), $"psw-test-history-{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        // Clean up test history directory after each test
        if (Directory.Exists(_testHistoryDirectory))
        {
            Directory.Delete(_testHistoryDirectory, recursive: true);
        }
    }

    [Fact]
    public void HistoryService_Initialization_CreatesDirectory()
    {
        // Act
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Assert
        Directory.Exists(_testHistoryDirectory).Should().BeTrue("history directory should be created");
    }

    [Fact]
    public void AddEntry_CreatesEntryWithValidId()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var scriptModel = CreateTestScriptModel("TestProject");

        // Act
        var entry = historyService.AddEntry(scriptModel, "TestTemplate", "Test description");

        // Assert
        entry.Should().NotBeNull();
        entry.Id.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(entry.Id, out _).Should().BeTrue("ID should be a valid GUID");
        entry.ScriptModel.Should().Be(scriptModel);
        entry.TemplateName.Should().Be("TestTemplate");
        entry.Description.Should().Be("Test description");
    }

    [Fact]
    public void AddEntry_WithTags_StoresTagsCorrectly()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var scriptModel = CreateTestScriptModel("TestProject");
        var tags = new List<string> { "production", "umbraco-14", "migration" };

        // Act
        var entry = historyService.AddEntry(scriptModel, tags: tags);

        // Assert
        entry.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public void GetEntry_ById_ReturnsCorrectEntry()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var scriptModel = CreateTestScriptModel("TestProject");
        var addedEntry = historyService.AddEntry(scriptModel);

        // Act
        var retrieved = historyService.GetEntry(addedEntry.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(addedEntry.Id);
        retrieved.ScriptModel.ProjectName.Should().Be("TestProject");
    }

    [Fact]
    public void GetEntry_ByIndex_ReturnsCorrectEntry()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        historyService.AddEntry(CreateTestScriptModel("Project1"));
        historyService.AddEntry(CreateTestScriptModel("Project2"));
        var thirdEntry = historyService.AddEntry(CreateTestScriptModel("Project3"));

        // Act - Index 1 should be the most recent (Project3)
        var retrieved = historyService.GetEntry("1");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.ScriptModel.ProjectName.Should().Be("Project3", "index 1 should be the most recent entry");
    }

    [Fact]
    public void GetEntry_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        var retrieved = historyService.GetEntry("non-existent-id");

        // Assert
        retrieved.Should().BeNull();
    }

    [Fact]
    public void GetRecentEntries_ReturnsCorrectCount()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        for (int i = 1; i <= 15; i++)
        {
            historyService.AddEntry(CreateTestScriptModel($"Project{i}"));
        }

        // Act
        var recent = historyService.GetRecentEntries(5);

        // Assert
        recent.Should().HaveCount(5);
        recent[0].ScriptModel.ProjectName.Should().Be("Project15", "most recent should be first");
        recent[4].ScriptModel.ProjectName.Should().Be("Project11");
    }

    [Fact]
    public void GetRecentEntries_WithDefaultCount_Returns10()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        for (int i = 1; i <= 20; i++)
        {
            historyService.AddEntry(CreateTestScriptModel($"Project{i}"));
        }

        // Act
        var recent = historyService.GetRecentEntries();

        // Assert
        recent.Should().HaveCount(10, "default count should be 10");
    }

    [Fact]
    public void UpdateExecution_UpdatesEntryCorrectly()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var entry = historyService.AddEntry(CreateTestScriptModel("TestProject"));
        var executionDir = "/home/user/projects/test";
        var exitCode = 0;

        // Act
        historyService.UpdateExecution(entry.Id, executionDir, exitCode);

        // Assert
        var updated = historyService.GetEntry(entry.Id);
        updated.Should().NotBeNull();
        updated!.WasExecuted.Should().BeTrue();
        updated.ExecutionDirectory.Should().Be(executionDir);
        updated.ExitCode.Should().Be(exitCode);
    }

    [Fact]
    public void UpdateExecution_WithFailedExecution_StoresNonZeroExitCode()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var entry = historyService.AddEntry(CreateTestScriptModel("TestProject"));

        // Act
        historyService.UpdateExecution(entry.Id, "/tmp/test", exitCode: 1);

        // Assert
        var updated = historyService.GetEntry(entry.Id);
        updated!.ExitCode.Should().Be(1);
        updated.WasExecuted.Should().BeTrue();
    }

    [Fact]
    public void UpdateExecution_WithNonExistentId_DoesNotThrow()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        var act = () => historyService.UpdateExecution("non-existent-id", "/tmp", 0);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task GetAllHistoryAsync_ReturnsAllEntries()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        historyService.AddEntry(CreateTestScriptModel("Project1"));
        historyService.AddEntry(CreateTestScriptModel("Project2"));
        historyService.AddEntry(CreateTestScriptModel("Project3"));

        // Act
        var allHistory = await historyService.GetAllHistoryAsync();

        // Assert
        allHistory.Should().HaveCount(3);
    }

    [Fact]
    public void DeleteEntry_ById_RemovesEntry()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var entry = historyService.AddEntry(CreateTestScriptModel("TestProject"));
        var initialCount = historyService.GetCount();

        // Act
        var result = historyService.DeleteEntry(entry.Id);

        // Assert
        result.Should().BeTrue();
        historyService.GetCount().Should().Be(initialCount - 1);
        historyService.GetEntry(entry.Id).Should().BeNull();
    }

    [Fact]
    public void DeleteEntry_ByIndex_RemovesEntry()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        historyService.AddEntry(CreateTestScriptModel("Project1"));
        historyService.AddEntry(CreateTestScriptModel("Project2"));
        historyService.AddEntry(CreateTestScriptModel("Project3"));

        // Act - Delete index 2 (second most recent)
        var result = historyService.DeleteEntry("2");

        // Assert
        result.Should().BeTrue();
        historyService.GetCount().Should().Be(2);
    }

    [Fact]
    public void DeleteEntry_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        var result = historyService.DeleteEntry("non-existent-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ClearAll_RemovesAllEntries()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        historyService.AddEntry(CreateTestScriptModel("Project1"));
        historyService.AddEntry(CreateTestScriptModel("Project2"));
        historyService.AddEntry(CreateTestScriptModel("Project3"));

        // Act
        historyService.ClearAll();

        // Assert
        historyService.GetCount().Should().Be(0);
    }

    [Fact]
    public void GetCount_ReturnsCorrectCount()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act & Assert
        historyService.GetCount().Should().Be(0, "new history should be empty");

        historyService.AddEntry(CreateTestScriptModel("Project1"));
        historyService.GetCount().Should().Be(1);

        historyService.AddEntry(CreateTestScriptModel("Project2"));
        historyService.GetCount().Should().Be(2);
    }

    [Fact]
    public void GetMaxEntries_ReturnsDefaultValue()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        var maxEntries = historyService.GetMaxEntries();

        // Assert
        maxEntries.Should().Be(50, "default max entries should be 50");
    }

    [Fact]
    public void GetStats_CalculatesCorrectly()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Add 3 entries: 2 executed (1 success, 1 failure), 1 from template
        var entry1 = historyService.AddEntry(CreateTestScriptModel("Project1"), "Template1");
        historyService.UpdateExecution(entry1.Id, "/tmp", exitCode: 0);

        var entry2 = historyService.AddEntry(CreateTestScriptModel("Project2"));
        historyService.UpdateExecution(entry2.Id, "/tmp", exitCode: 1);

        var entry3 = historyService.AddEntry(CreateTestScriptModel("Project3"));

        // Act
        var stats = historyService.GetStats();

        // Assert
        stats.TotalEntries.Should().Be(3);
        stats.ExecutedCount.Should().Be(2);
        stats.SuccessfulCount.Should().Be(1);
        stats.FailedCount.Should().Be(1);
        stats.FromTemplateCount.Should().Be(1);
        stats.MostRecentDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        stats.OldestDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GetStats_WithEmptyHistory_ReturnsZeros()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        var stats = historyService.GetStats();

        // Assert
        stats.TotalEntries.Should().Be(0);
        stats.ExecutedCount.Should().Be(0);
        stats.SuccessfulCount.Should().Be(0);
        stats.FailedCount.Should().Be(0);
        stats.FromTemplateCount.Should().Be(0);
        stats.MostRecentDate.Should().BeNull();
        stats.OldestDate.Should().BeNull();
    }

    [Fact]
    public void HistoryService_PersistsToDisk()
    {
        // Arrange
        var scriptModel = CreateTestScriptModel("PersistentProject");
        string entryId;

        // Act - Create service, add entry, dispose
        using (var historyService1 = new HistoryService(historyDirectory: _testHistoryDirectory))
        {
            var entry = historyService1.AddEntry(scriptModel, "PersistentTemplate");
            entryId = entry.Id;
        }

        // Create new instance - should load from disk
        using (var historyService2 = new HistoryService(historyDirectory: _testHistoryDirectory))
        {
            var retrieved = historyService2.GetEntry(entryId);

            // Assert
            retrieved.Should().NotBeNull();
            retrieved!.ScriptModel.ProjectName.Should().Be("PersistentProject");
            retrieved.TemplateName.Should().Be("PersistentTemplate");
        }
    }

    [Fact]
    public void HistoryService_LoadsExistingHistoryOnInitialization()
    {
        // Arrange - Create and populate history
        using (var historyService1 = new HistoryService(historyDirectory: _testHistoryDirectory))
        {
            historyService1.AddEntry(CreateTestScriptModel("Project1"));
            historyService1.AddEntry(CreateTestScriptModel("Project2"));
        }

        // Act - Create new instance
        using (var historyService2 = new HistoryService(historyDirectory: _testHistoryDirectory))
        {
            // Assert
            historyService2.GetCount().Should().Be(2, "existing history should be loaded on initialization");
        }
    }

    [Fact]
    public void HistoryService_WithCorruptedHistoryFile_CreatesNewHistory()
    {
        // Arrange - Create history directory and write invalid YAML
        Directory.CreateDirectory(_testHistoryDirectory);
        var historyFile = Path.Combine(_testHistoryDirectory, "history.yaml");
        File.WriteAllText(historyFile, "corrupt: yaml: data: {{{");

        // Act - Should handle corrupted file gracefully
        var act = () => new HistoryService(historyDirectory: _testHistoryDirectory);

        // Assert
        act.Should().NotThrow("should handle corrupted history file gracefully");
        using (var historyService = new HistoryService(historyDirectory: _testHistoryDirectory))
        {
            historyService.GetCount().Should().Be(0, "should start with empty history");
        }
    }

    [Fact]
    public void HistoryService_MaintainsOrderNewestFirst()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);

        // Act
        historyService.AddEntry(CreateTestScriptModel("First"));
        Thread.Sleep(10); // Ensure different timestamps
        historyService.AddEntry(CreateTestScriptModel("Second"));
        Thread.Sleep(10);
        historyService.AddEntry(CreateTestScriptModel("Third"));

        var recent = historyService.GetRecentEntries(3);

        // Assert
        recent[0].ScriptModel.ProjectName.Should().Be("Third", "newest should be first");
        recent[1].ScriptModel.ProjectName.Should().Be("Second");
        recent[2].ScriptModel.ProjectName.Should().Be("First", "oldest should be last");
    }

    [Fact]
    public void AddEntry_WithNullTemplateName_WorksCorrectly()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var scriptModel = CreateTestScriptModel("TestProject");

        // Act
        var entry = historyService.AddEntry(scriptModel, templateName: null);

        // Assert
        entry.Should().NotBeNull();
        entry.TemplateName.Should().BeNull();
        entry.ScriptModel.ProjectName.Should().Be("TestProject");
    }

    [Fact]
    public void AddEntry_WithEmptyTagsList_WorksCorrectly()
    {
        // Arrange
        var historyService = new HistoryService(historyDirectory: _testHistoryDirectory);
        var scriptModel = CreateTestScriptModel("TestProject");

        // Act
        var entry = historyService.AddEntry(scriptModel, tags: new List<string>());

        // Assert
        entry.Should().NotBeNull();
        entry.Tags.Should().BeEmpty();
    }

    /// <summary>
    /// Helper method to create a test ScriptModel
    /// </summary>
    private static ScriptModel CreateTestScriptModel(string projectName)
    {
        return new ScriptModel
        {
            ProjectName = projectName,
            TemplateName = "Umbraco.Templates",
            TemplateVersion = "14.0.0",
            CreateSolutionFile = true,
            UseUnattendedInstall = true,
            DatabaseType = "SQLite"
        };
    }
}
