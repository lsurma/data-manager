namespace DataManager.Host.WA.Components;

public record FilterSettings
{
    public required string Id { get; set; }
    public required string Label { get; set; }
    public bool Visible { get; set; } = true;
    public int OrderIndex { get; set; }
}
