namespace DataManager.Host.WA.Components;

public record ColumnSettings
{
    public string? Title { get; set; }
    public required string UniqueID { get; set; }
    public bool Visible { get; set; }
    
    public int OrderIndex { get; set; }
    public string? Width { get; set; }
}