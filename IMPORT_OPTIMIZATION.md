# Import Translations Optimization

## Overview

This document explains the optimization implemented for the `ImportTranslationsCommandHandler` to improve performance when importing large batches of translations.

## Problem

The original implementation had a performance issue:
- Each translation imported resulted in 2-3 database queries:
  1. Authorization check to get accessible TranslationsSets
  2. Query to check if translation already exists (by ID or by unique keys)
  3. Additional authorization checks during the query
- For an import of N translations, this resulted in 2N-3N database queries
- Each query also repeated the same authorization check

## Solution

The optimization uses three key techniques:

### 1. Local View (Change Tracker) Support

Added a `UseLocalView` option to `QueryOptions` that allows queries to fetch entities from EF Core's Local view (the change tracker) instead of hitting the database.

**Key files modified:**
- `DataManager.Application.Core/Common/QueryOptions.cs` - Added `UseLocalView` property
- `DataManager.Application.Core/Common/QueryHelper.cs` - Added `LocalQuery` property and `GetLocalQuery()` method
- `DataManager.Application.Core/Modules/Translations/TranslationsQueryService.cs` - Overrode `LocalQuery` to return `_context.Translations.Local.AsQueryable()`

**How it works:**
- When `UseLocalView = true`, queries fetch from the change tracker instead of the database
- Authorization is still applied, but in-memory rather than via SQL WHERE clauses
- This is very fast when entities are already loaded into the change tracker
- Falls back to database queries if entities aren't loaded

### 2. Prefetching in ImportTranslationsCommandHandler

Added a `PrefetchExistingTranslationsAsync` method that loads all potentially affected translations in a single database query before processing the import.

**Key features:**
- Builds a single query that matches all unique keys (ResourceName, TranslationName, CultureName) from the import data
- Authorization is applied during the prefetch, so no additional checks needed
- Uses efficient SQL with IN clauses for bulk matching
- Loads translations into the change tracker with tracking enabled

**Performance impact:**
- **Before:** N translations × 2-3 queries = 2N-3N database queries
- **After:** 1 prefetch query + 0 database queries for existing translations = 1 database query total
- **Improvement:** Reduces database load by 2N-3N times for imports of N existing translations

### 3. Optimized SaveSingleTranslationCommandHandler

Updated to try the local view first before querying the database:

**Line 43 - GetByIdAsync:**
```csharp
// Try local view first
translation = await _queryService.GetByIdAsync(
    request.Id.Value,
    options: new QueryOptions<Translation, Guid> { UseLocalView = true },
    cancellationToken: cancellationToken);

// Fall back to database if not found
if (translation == null)
{
    translation = await _queryService.GetByIdAsync(
        request.Id.Value,
        cancellationToken: cancellationToken);
}
```

**Line 56 - PrepareQueryAsync:**
```csharp
// Try local view first
var queryLocal = await _queryService.PrepareQueryAsync(
    options: new QueryOptions<Translation, Guid> { UseLocalView = true },
    cancellationToken: cancellationToken);

translation = queryLocal.FirstOrDefault(/* filters */);

// Fall back to database if not found
if (translation == null)
{
    var query = await _queryService.PrepareQueryAsync(
        options: new QueryOptions<Translation, Guid> { AsNoTracking = false },
        cancellationToken: cancellationToken);
    
    translation = await query.FirstOrDefaultAsync(/* filters */);
}
```

## Security Considerations

**Authorization is maintained:**
- The prefetch query applies authorization filters before loading translations
- The local view queries use `PrepareQueryAsync`, which applies authorization in-memory
- `OmitAuthorizationScope` is NOT used - all authorization checks still happen
- Users can only see translations from TranslationsSets they have access to

**No security vulnerabilities introduced:**
- Authorization happens during prefetch (database level)
- Local view queries still filter by authorized TranslationsSet IDs (memory level)
- The optimization is purely about where the filtering happens (database vs. memory)

## Usage Example

### Before (Slow)
```csharp
// Import 100 translations
var command = new ImportTranslationsCommand
{
    TranslationsSetId = datasetId,
    Translations = translations // 100 items
};

// Results in 200-300 database queries
var result = await _mediator.Send(command);
```

### After (Fast)
```csharp
// Import 100 translations
var command = new ImportTranslationsCommand
{
    TranslationsSetId = datasetId,
    Translations = translations // 100 items
};

// Results in 1 prefetch query + saves only
var result = await _mediator.Send(command);
```

## Typical Performance Improvement

| Import Size | Database Queries Before | Database Queries After | Improvement |
|-------------|------------------------|------------------------|-------------|
| 10 items    | 20-30                 | 1                      | 20-30x      |
| 50 items    | 100-150               | 1                      | 100-150x    |
| 100 items   | 200-300               | 1                      | 200-300x    |
| 500 items   | 1000-1500             | 1                      | 1000-1500x  |

*Note: These numbers assume most translations already exist and are being updated. For new translations, the improvement is less dramatic but still significant.*

## Implementation Notes

1. **Local view synchronous operations:** The local view queries use synchronous `FirstOrDefault()` instead of async `FirstOrDefaultAsync()` because they operate on in-memory collections. This is more efficient and avoids unnecessary async overhead.

2. **Change tracker vs. AsNoTracking:** The prefetch uses tracking (not `AsNoTracking`) so entities are loaded into the change tracker. This is intentional and necessary for the optimization to work.

3. **Fallback behavior:** If an entity isn't found in the local view, the code falls back to a database query. This ensures correctness even if prefetch logic changes or is bypassed.

4. **Thread safety:** The local view is scoped to the DbContext, which is scoped per request. No threading issues arise because each request has its own DbContext instance.

## Future Enhancements

Possible future optimizations:
1. Batch the prefetch for extremely large imports (1000+ translations) to avoid query parameter limits
2. Add metrics/logging to track cache hit rate (local view vs. database)
3. Consider using `DbContext.ChangeTracker.Entries<Translation>()` for more direct access in some scenarios
