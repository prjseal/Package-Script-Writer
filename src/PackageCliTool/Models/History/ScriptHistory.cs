namespace PackageCliTool.Models.History;

/// <summary>
/// Container for all script generation history
/// </summary>
public class ScriptHistory
{
    /// <summary>
    /// Version of the history file format
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// All history entries
    /// </summary>
    public List<HistoryEntry> Entries { get; set; } = new();

    /// <summary>
    /// Maximum number of entries to keep
    /// </summary>
    public int MaxEntries { get; set; } = 50;

    /// <summary>
    /// Adds a new history entry and maintains the maximum count
    /// </summary>
    public void AddEntry(HistoryEntry entry)
    {
        Entries.Insert(0, entry); // Add to beginning (most recent first)

        // Remove oldest entries if we exceed max
        while (Entries.Count > MaxEntries)
        {
            Entries.RemoveAt(Entries.Count - 1);
        }
    }

    /// <summary>
    /// Gets an entry by ID
    /// </summary>
    public HistoryEntry? GetEntry(string id)
    {
        return Entries.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets an entry by index (1-based)
    /// </summary>
    public HistoryEntry? GetEntryByIndex(int index)
    {
        if (index < 1 || index > Entries.Count)
        {
            return null;
        }

        return Entries[index - 1];
    }

    /// <summary>
    /// Removes an entry by ID
    /// </summary>
    public bool RemoveEntry(string id)
    {
        var entry = GetEntry(id);
        if (entry != null)
        {
            Entries.Remove(entry);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clears all history
    /// </summary>
    public void Clear()
    {
        Entries.Clear();
    }
}
