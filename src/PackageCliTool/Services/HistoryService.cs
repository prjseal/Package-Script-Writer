using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PackageCliTool.Models.History;
using PackageCliTool.Models.Api;
using PackageCliTool.Exceptions;

namespace PackageCliTool.Services;

/// <summary>
/// Service for managing script generation history
/// </summary>
public class HistoryService
{
    private readonly string _historyFile;
    private readonly ILogger? _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;
    private ScriptHistory _history;

    public HistoryService(string? historyDirectory = null, ILogger? logger = null)
    {
        // Default to ~/.psw/history/
        var historyDir = historyDirectory ?? GetDefaultHistoryDirectory();
        _historyFile = Path.Combine(historyDir, "history.yaml");
        _logger = logger;

        // Initialize YAML serializer/deserializer
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        EnsureDirectoryExists(historyDir);
        _history = LoadHistory();
    }

    /// <summary>
    /// Gets the default history directory path
    /// </summary>
    private static string GetDefaultHistoryDirectory()
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(homeDir, ".psw", "history");
    }

    /// <summary>
    /// Ensures the history directory exists
    /// </summary>
    private void EnsureDirectoryExists(string directory)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _logger?.LogInformation("Created history directory: {Directory}", directory);
        }
    }

    /// <summary>
    /// Loads history from disk
    /// </summary>
    private ScriptHistory LoadHistory()
    {
        if (!File.Exists(_historyFile))
        {
            _logger?.LogDebug("History file not found, creating new history");
            return new ScriptHistory();
        }

        try
        {
            var yaml = File.ReadAllText(_historyFile);
            var history = _deserializer.Deserialize<ScriptHistory>(yaml);
            _logger?.LogDebug("Loaded {Count} history entries", history.Entries.Count);
            return history;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load history file, creating new history");
            return new ScriptHistory();
        }
    }

    /// <summary>
    /// Saves history to disk
    /// </summary>
    private void SaveHistory()
    {
        try
        {
            var yaml = _serializer.Serialize(_history);
            File.WriteAllText(_historyFile, yaml);
            _logger?.LogDebug("Saved history with {Count} entries", _history.Entries.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save history file");
        }
    }

    /// <summary>
    /// Adds a new history entry
    /// </summary>
    public HistoryEntry AddEntry(
        string scriptContent,
        ScriptModel scriptModel,
        string? templateName = null,
        string? description = null,
        List<string>? tags = null)
    {
        var entry = new HistoryEntry
        {
            ScriptContent = scriptContent,
            ScriptModel = scriptModel,
            TemplateName = templateName,
            Description = description,
            Tags = tags ?? new List<string>()
        };

        _history.AddEntry(entry);
        SaveHistory();

        _logger?.LogInformation("Added history entry: {Id}", entry.Id);
        return entry;
    }

    /// <summary>
    /// Updates an entry with execution information
    /// </summary>
    public void UpdateExecution(string id, string executionDirectory, int exitCode)
    {
        var entry = _history.GetEntry(id);
        if (entry != null)
        {
            entry.WasExecuted = true;
            entry.ExecutionDirectory = executionDirectory;
            entry.ExitCode = exitCode;
            SaveHistory();
            _logger?.LogInformation("Updated execution info for history entry: {Id}", id);
        }
    }

    /// <summary>
    /// Gets all history entries
    /// </summary>
    public List<HistoryEntry> GetAllEntries()
    {
        return _history.Entries;
    }

    /// <summary>
    /// Gets an entry by ID or index
    /// </summary>
    public HistoryEntry? GetEntry(string idOrIndex)
    {
        // Try to parse as index first
        if (int.TryParse(idOrIndex, out int index))
        {
            return _history.GetEntryByIndex(index);
        }

        // Otherwise try as ID
        return _history.GetEntry(idOrIndex);
    }

    /// <summary>
    /// Gets recent entries
    /// </summary>
    public List<HistoryEntry> GetRecentEntries(int count = 10)
    {
        return _history.Entries.Take(count).ToList();
    }

    /// <summary>
    /// Gets entries by tag
    /// </summary>
    public List<HistoryEntry> GetEntriesByTag(string tag)
    {
        return _history.Entries
            .Where(e => e.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets entries by template name
    /// </summary>
    public List<HistoryEntry> GetEntriesByTemplate(string templateName)
    {
        return _history.Entries
            .Where(e => e.TemplateName?.Equals(templateName, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
    }

    /// <summary>
    /// Deletes an entry
    /// </summary>
    public bool DeleteEntry(string idOrIndex)
    {
        var entry = GetEntry(idOrIndex);
        if (entry != null)
        {
            _history.RemoveEntry(entry.Id);
            SaveHistory();
            _logger?.LogInformation("Deleted history entry: {Id}", entry.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears all history
    /// </summary>
    public void ClearAll()
    {
        _history.Clear();
        SaveHistory();
        _logger?.LogInformation("Cleared all history");
    }

    /// <summary>
    /// Gets the total count of history entries
    /// </summary>
    public int GetCount()
    {
        return _history.Entries.Count;
    }

    /// <summary>
    /// Gets the maximum number of entries to keep
    /// </summary>
    public int GetMaxEntries()
    {
        return _history.MaxEntries;
    }

    /// <summary>
    /// Sets the maximum number of entries to keep
    /// </summary>
    public void SetMaxEntries(int maxEntries)
    {
        if (maxEntries < 1)
        {
            throw new PswException("Max entries must be at least 1", "Provide a valid number");
        }

        _history.MaxEntries = maxEntries;

        // Trim if necessary
        while (_history.Entries.Count > maxEntries)
        {
            _history.Entries.RemoveAt(_history.Entries.Count - 1);
        }

        SaveHistory();
        _logger?.LogInformation("Set max entries to {Max}", maxEntries);
    }

    /// <summary>
    /// Adds tags to an entry
    /// </summary>
    public bool AddTags(string idOrIndex, params string[] tags)
    {
        var entry = GetEntry(idOrIndex);
        if (entry != null)
        {
            foreach (var tag in tags)
            {
                if (!entry.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                {
                    entry.Tags.Add(tag);
                }
            }
            SaveHistory();
            _logger?.LogInformation("Added tags to history entry: {Id}", entry.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the description for an entry
    /// </summary>
    public bool SetDescription(string idOrIndex, string description)
    {
        var entry = GetEntry(idOrIndex);
        if (entry != null)
        {
            entry.Description = description;
            SaveHistory();
            _logger?.LogInformation("Updated description for history entry: {Id}", entry.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Exports history to a file
    /// </summary>
    public void ExportHistory(string outputPath)
    {
        var yaml = _serializer.Serialize(_history);
        File.WriteAllText(outputPath, yaml);
        _logger?.LogInformation("Exported history to {Path}", outputPath);
    }

    /// <summary>
    /// Gets statistics about the history
    /// </summary>
    public HistoryStats GetStats()
    {
        return new HistoryStats
        {
            TotalEntries = _history.Entries.Count,
            ExecutedCount = _history.Entries.Count(e => e.WasExecuted),
            SuccessfulCount = _history.Entries.Count(e => e.WasExecuted && e.ExitCode == 0),
            FailedCount = _history.Entries.Count(e => e.WasExecuted && e.ExitCode != 0),
            FromTemplateCount = _history.Entries.Count(e => !string.IsNullOrWhiteSpace(e.TemplateName)),
            MostRecentDate = _history.Entries.FirstOrDefault()?.Timestamp,
            OldestDate = _history.Entries.LastOrDefault()?.Timestamp
        };
    }
}

/// <summary>
/// Statistics about the history
/// </summary>
public class HistoryStats
{
    public int TotalEntries { get; set; }
    public int ExecutedCount { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int FromTemplateCount { get; set; }
    public DateTime? MostRecentDate { get; set; }
    public DateTime? OldestDate { get; set; }
}
