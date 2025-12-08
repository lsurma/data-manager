# Translation Hierarchy Feature Usage Guide

## Overview

The Translation entity now supports tracking the source translation for materialized translations and provides core methods to:
1. Fetch translations from a dataset hierarchy with proper deduplication (virtual view)
2. Materialize translations from hierarchy into the root dataset (materialized view)

## New Fields

### Translation Entity
- **SourceTranslationId** (Guid?, nullable): Optional reference to the source Translation from which this translation was materialized. When null, indicates this is an "original" translation; when set, points directly to the source translation entity from another dataset.
- **SourceTranslation** (Translation?, nullable): Navigation property to the source Translation.
- **SourceTranslationLastSyncedAt** (DateTimeOffset?, nullable): Timestamp of the last sync from the source Translation.

## Core Methods

### 1. GetTranslationsFromHierarchyAsync (Virtual View)

#### Purpose
Fetches translations from a TranslationsSet hierarchy with automatic deduplication based on priority. This is a "virtual view" - translations are fetched on-demand without being stored in the root dataset.

#### Method Signature
```csharp
public async Task<List<Translation>> GetTranslationsFromHierarchyAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```

#### How It Works

1. **Hierarchy Traversal**: Uses breadth-first search starting from the root dataset
2. **Translation Fetching**: Loads translations from datasets in the hierarchy
3. **Deduplication**: Applies deduplication logic where translations from higher priority datasets take precedence
4. **Deduplication Key**: `(ResourceName, CultureName, TranslationName)`

#### Usage Example

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

### 2. MaterializeTranslationsFromHierarchyAsync (Materialized View)

#### Purpose
Materializes (copies) translations from included datasets into the root dataset. This creates a "materialized view" where translations are physically stored in the root dataset, allowing simple queries without hierarchy traversal.

#### Method Signature
```csharp
public async Task<int> MaterializeTranslationsFromHierarchyAsync(
    Guid rootDataSetId,
    CancellationToken cancellationToken = default)
```

#### How It Works

1. **Hierarchy Traversal**: Identifies all included datasets
2. **Deduplication Check**: Skips translations that already exist as originals in the root dataset
3. **Copy Process**: 
   - Creates new Translation entities in the root dataset
   - Sets `SourceTranslationId` to point directly to the source translation entity
   - Sets `SourceTranslationLastSyncedAt` to current timestamp
4. **Update Existing**: Updates previously materialized translations if they changed in source datasets
5. **Returns**: Count of translations materialized (added or updated)

#### Benefits

- **Performance**: Simple queries on root dataset return all translations without hierarchy traversal
- **Offline Access**: Root dataset contains all data even if source datasets are unavailable
- **Direct Tracking**: `SourceTranslationId` points directly to the source translation entity, enabling easy navigation and comparison
- **Auditing**: Can trace back to exact source translation and see which dataset it came from via the source translation's DataSetId

#### Usage Example

```csharp
public class SyncHandler
{
    private readonly TranslationsQueryService _translationsQueryService;

    public SyncHandler(TranslationsQueryService translationsQueryService)
    {
        _translationsQueryService = translationsQueryService;
    }

    public async Task<int> SyncDataSet(Guid dataSetId)
    {
        // Materialize all translations from hierarchy into the root dataset
        var count = await _translationsQueryService
            .MaterializeTranslationsFromHierarchyAsync(dataSetId, CancellationToken.None);
        
        Console.WriteLine($"Materialized {count} translations");
        return count;
    }
}
```

## Example Scenarios

### Scenario: Custom Dataset with Included Datasets

Consider this hierarchy:
- **some-custom-data-set** (root) includes **data-set-a** and **data-set-b**

Hierarchy order: `[some-custom-data-set, data-set-a, data-set-b]`

**Virtual View (GetTranslationsFromHierarchyAsync):**
- Queries translations from all three datasets on-demand
- Deduplicates based on priority
- Does not modify database

**Materialized View (MaterializeTranslationsFromHierarchyAsync):**
- Copies translations from data-set-a and data-set-b into some-custom-data-set
- Marks copies with `SourceTranslationId` pointing to the source translation
- After materialization, a simple query on some-custom-data-set returns all translations

### Translation Priority

If the same translation key exists in multiple datasets:
- Translation from **some-custom-data-set** (original, `SourceTranslationId = null`) has highest priority
- Translation from **data-set-a** is used only if not in some-custom-data-set
- Translation from **data-set-b** is used only if not in some-custom-data-set or data-set-a

## Important Notes

### Authorization
⚠️ **WARNING**: Both methods are CORE methods that **omit authorization checks**. Use with caution and ensure authorization is handled at a higher level if needed.

### Performance Considerations
- Virtual view: Performs hierarchy traversal on each call, suitable for ad-hoc queries
- Materialized view: One-time cost to copy data, then fast queries; suitable for frequently accessed datasets
- For most applications, dataset hierarchies are small (dozens to hundreds of datasets)

### Current Version Only
Both methods only work with translations where `IsCurrentVersion = true`. Draft and old versions are excluded.

### Materialization Strategy
- Original translations (where `SourceTranslationId = null`) in root dataset always take precedence
- Materialized translations can be updated by re-running materialization
- Consider running materialization on a schedule or trigger when source datasets change
- Use `SourceTranslation` navigation property to access the original source translation and compare changes

## Database Migrations

### Migration: 20251204230333_AddSourceDataSetToTranslation (Superseded)
- Initial migration that added `SourceDataSetId` and `SourceDataSetLastSyncedAt`
- Superseded by the next migration

### Migration: 20251205184538_ReplaceSourceDataSetWithSourceTranslation
- Renamed `SourceDataSetId` to `SourceTranslationId`
- Renamed `SourceDataSetLastSyncedAt` to `SourceTranslationLastSyncedAt`
- Changed foreign key from TranslationsSets to Translations (self-referencing)
- Maintains the same index for query performance
