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

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryService"/> class
    /// </summary>
    /// <param name="historyDirectory">The directory to store history files (defaults to ~/.psw/history/)</param>
    /// <param name="logger">Optional logger instance</param>
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
        ScriptModel scriptModel,
        string? templateName = null,
        string? description = null,
        List<string>? tags = null)
    {
        var entry = new HistoryEntry
        {
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
    /// Asynchronously retrieves all history entries.
    /// </summary>
    /// <returns>A list of <see cref="HistoryEntry"/> representing all history entries.</returns>
    public async Task<List<HistoryEntry>> GetAllHistoryAsync()
    {
        return await Task.Run(() => _history.Entries);
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
}
