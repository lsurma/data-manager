namespace DataManager.Host.WA.Components;

/// <summary>
/// Application-specific settings for DataGrid that are managed separately from Radzen's DataGridSettings.
/// This allows us to track and persist custom UI preferences.
/// </summary>
public record AppDataGridSettings
{
    /// <summary>
    /// Column visibility and order settings
    /// </summary>
    public List<ColumnSettings> Columns { get; set; } = new();

    // Future settings can be added here:
    // public int PageSize { get; set; } = 20;
    // public string? DefaultSortColumn { get; set; }
    // public string? DefaultSortDirection { get; set; }
    // public bool CompactMode { get; set; }
    // public Dictionary<string, object> CustomFilters { get; set; } = new();
}
