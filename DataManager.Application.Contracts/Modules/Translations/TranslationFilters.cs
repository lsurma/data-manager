namespace DataManager.Application.Contracts.Modules.Translations;

/// <summary>
/// Filter by DataSet ID
/// </summary>
public class DataSetIdFilter : TranslationFilterBase<DataSetIdFilter>
{
    public DataSetIdFilter()
    {
        
    }

    public DataSetIdFilter(Guid id)
    {
        Value = id;
    }
    
    public Guid? Value { get; set; }
    
    public override bool IsActive() => Value.HasValue;
}

/// <summary>
/// Filter by Culture Name
/// </summary>
public class CultureNameFilter : TranslationFilterBase<CultureNameFilter>
{
    public CultureNameFilter()
    {
        
    }

    public CultureNameFilter(string cultureName)
    {
        Value = cultureName;
    }
    
    public string? Value { get; set; }
    
    public override bool IsActive() => !string.IsNullOrWhiteSpace(Value);
}

/// <summary>
/// Filter for base translations (SourceId is null) that match the specified culture or have null culture.
/// Used to find potential source translations for linking.
/// </summary>
public class BaseTranslationFilter : TranslationFilterBase<BaseTranslationFilter>
{
    /// <summary>
    /// The culture to match. Translations with this culture OR null culture will be included.
    /// </summary>
    public string? CultureName { get; set; }
    
    /// <summary>
    /// This filter is always active when present - it always filters for SourceId = null
    /// </summary>
    public override bool IsActive() => true;
}

/// <summary>
/// Filter by version status (current, draft, old)
/// </summary>
public class VersionStatusFilter : TranslationFilterBase<VersionStatusFilter>
{
    /// <summary>
    /// Include current versions
    /// </summary>
    public bool? IncludeCurrentVersions { get; set; }

    /// <summary>
    /// Include draft versions
    /// </summary>
    public bool? IncludeDraftVersions { get; set; }

    /// <summary>
    /// Include old versions
    /// </summary>
    public bool? IncludeOldVersions { get; set; }
    
    public override bool IsActive() => IncludeCurrentVersions.HasValue || IncludeDraftVersions.HasValue || IncludeOldVersions.HasValue;
}
