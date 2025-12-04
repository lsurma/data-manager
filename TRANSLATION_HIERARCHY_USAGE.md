# Translation Hierarchy Feature Usage Guide

## Overview

The Translation entity now supports tracking the source dataset for translations and provides a core method to fetch translations from a dataset hierarchy with proper deduplication.

## New Fields

### Translation Entity
- **SourceDataSetId** (Guid?, nullable): Optional reference to the source DataSet where this translation was fetched from. When null, indicates this is an "original" translation; when set, indicates it was fetched from another dataset.
- **SourceDataSet** (DataSet?, nullable): Navigation property to the source DataSet.
- **SourceDataSetLastSyncedAt** (DateTimeOffset?, nullable): Timestamp of the last sync from the source DataSet.

## Core Method: GetTranslationsFromHierarchyAsync

### Purpose
Fetches translations from a DataSet hierarchy with automatic deduplication based on priority.

### Method Signature
```csharp
public async Task<List<Translation>> GetTranslationsFromHierarchyAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```

### How It Works

1. **Hierarchy Traversal**: Uses breadth-first search starting from the root dataset
2. **Translation Fetching**: Loads all translations from datasets in the hierarchy
3. **Deduplication**: Applies deduplication logic where translations from higher priority datasets take precedence
4. **Deduplication Key**: `(ResourceName, CultureName, TranslationName)`

### Example Scenario

Consider this hierarchy:
- **Final** (root) includes **GlobalData**
- **GlobalData** includes **A** and **B**

Hierarchy order: `[Final, GlobalData, A, B]`

If the same translation exists in multiple datasets:
- Translation from **Final** has highest priority
- Translation from **GlobalData** is used only if not in **Final**
- Translations from **A** and **B** are used only if not in **Final** or **GlobalData**

### Usage Example

```csharp
// Inject TranslationsQueryService
public class MyHandler
{
    private readonly TranslationsQueryService _translationsQueryService;

    public MyHandler(TranslationsQueryService translationsQueryService)
    {
        _translationsQueryService = translationsQueryService;
    }

    public async Task<List<Translation>> GetAllTranslations(Guid dataSetId)
    {
        // This method omits authorization - use with caution!
        var translations = await _translationsQueryService
            .GetTranslationsFromHierarchyAsync(dataSetId, CancellationToken.None);
        
        return translations;
    }
}
```

## Important Notes

### Authorization
⚠️ **WARNING**: `GetTranslationsFromHierarchyAsync` is a CORE method that **omits authorization checks**. Use with caution and ensure authorization is handled at a higher level if needed.

### Performance Considerations
- The method loads all translations from the hierarchy into memory before deduplication
- For most applications, this is acceptable as dataset hierarchies are typically small
- If performance becomes an issue:
  - Consider limiting the depth of hierarchy traversal
  - Implement caching at a higher level
  - For very large datasets, the SQL-based deduplication approach might be needed (would require more complex implementation)

### Current Version Only
The method only fetches translations where `IsCurrentVersion = true`. Draft and old versions are excluded.

## Database Migration

The migration `20251204230333_AddSourceDataSetToTranslation` adds:
- `SourceDataSetId` column with foreign key to DataSets table
- `SourceDataSetLastSyncedAt` column
- Index on `SourceDataSetId` for query performance
