namespace DataManager.Host.WA.Modules.Translations.Models;

public class ExportTranslationsModel
{
    public ExportFormat ExportFormat { get; set; } = ExportFormat.Excel;
    public ExportType ExportType { get; set; } = ExportType.All;
    public bool UseCurrentFilters { get; set; }
    public string BaseCulture { get; set; } = "en-US";
}

public enum ExportFormat
{
    Csv,
    Excel
}

public enum ExportType
{
    All,
    Visible
}
